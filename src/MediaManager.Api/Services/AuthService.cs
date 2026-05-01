using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace MediaManager.Api.Services;

/// <summary>
/// JWT 配置
/// </summary>
public class JwtConfig
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int AccessTokenExpirationMinutes { get; set; } = 60;
    public int RefreshTokenExpirationDays { get; set; } = 7;
}

/// <summary>
/// 认证服务接口
/// </summary>
public interface IAuthService
{
    Task<AuthResponse> LoginAsync(string username, string password);
    Task<AuthResponse> RegisterAsync(string username, string email, string password);
    Task<AuthResponse> RefreshTokenAsync(string token, string refreshToken);
    Task<bool> RevokeTokenAsync(Guid userId);
    Task<User?> GetUserByIdAsync(Guid userId);
    Task<User?> GetUserByUsernameAsync(string username);
    Task<bool> ChangePasswordAsync(Guid userId, string oldPassword, string newPassword);
    Task<bool> UpdateUserRoleAsync(Guid userId, UserRole newRole);
    Task<IEnumerable<User>> GetAllUsersAsync();
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
}

/// <summary>
/// 认证服务实现
/// </summary>
public class AuthService : IAuthService
{
    private readonly JwtConfig _jwtConfig;
    private readonly ILogger<AuthService> _logger;
    private readonly Dictionary<Guid, User> _users = new();
    private readonly Dictionary<string, Guid> _usernames = new(StringComparer.OrdinalIgnoreCase);

    public AuthService(JwtConfig jwtConfig, ILogger<AuthService> logger)
    {
        _jwtConfig = jwtConfig;
        _logger = logger;
        
        var adminUser = new User
        {
            Id = Guid.NewGuid(),
            Username = "admin",
            Email = "admin@example.com",
            PasswordHash = HashPassword("admin123"),
            Role = UserRole.Administrator,
            CreatedAt = DateTime.UtcNow
        };
        _users[adminUser.Id] = adminUser;
        _usernames[adminUser.Username] = adminUser.Id;
    }

    public async Task<AuthResponse> LoginAsync(string username, string password)
    {
        await Task.Delay(100);

        if (!_usernames.TryGetValue(username, out var userId))
        {
            return new AuthResponse
            {
                Success = false,
                Message = "用户名或密码错误"
            };
        }

        var user = _users[userId];
        if (!VerifyPassword(password, user.PasswordHash))
        {
            return new AuthResponse
            {
                Success = false,
                Message = "用户名或密码错误"
            };
        }

        if (!user.IsActive)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "账户已被禁用"
            };
        }

        user.LastLoginAt = DateTime.UtcNow;
        var token = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_jwtConfig.RefreshTokenExpirationDays);

        _logger.LogInformation("用户登录成功: {Username}", username);

        return new AuthResponse
        {
            Success = true,
            Message = "登录成功",
            Token = token,
            RefreshToken = refreshToken,
            User = MapToDto(user)
        };
    }

    public async Task<AuthResponse> RegisterAsync(string username, string email, string password)
    {
        await Task.Delay(100);

        if (_usernames.ContainsKey(username))
        {
            return new AuthResponse
            {
                Success = false,
                Message = "用户名已存在"
            };
        }

        if (_users.Values.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
        {
            return new AuthResponse
            {
                Success = false,
                Message = "邮箱已被注册"
            };
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = username,
            Email = email,
            PasswordHash = HashPassword(password),
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow
        };

        _users[user.Id] = user;
        _usernames[username] = user.Id;

        var token = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_jwtConfig.RefreshTokenExpirationDays);

        _logger.LogInformation("新用户注册: {Username}", username);

        return new AuthResponse
        {
            Success = true,
            Message = "注册成功",
            Token = token,
            RefreshToken = refreshToken,
            User = MapToDto(user)
        };
    }

    public async Task<AuthResponse> RefreshTokenAsync(string token, string refreshToken)
    {
        await Task.Delay(50);

        var principal = GetPrincipalFromExpiredToken(token);
        if (principal == null)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "无效的访问令牌"
            };
        }

        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return new AuthResponse
            {
                Success = false,
                Message = "无效的令牌内容"
            };
        }

        if (!_users.TryGetValue(userId, out var user))
        {
            return new AuthResponse
            {
                Success = false,
                Message = "用户不存在"
            };
        }

        if (user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            return new AuthResponse
            {
                Success = false,
                Message = "无效的刷新令牌"
            };
        }

        var newToken = GenerateAccessToken(user);
        var newRefreshToken = GenerateRefreshToken();
        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_jwtConfig.RefreshTokenExpirationDays);

        return new AuthResponse
        {
            Success = true,
            Message = "令牌刷新成功",
            Token = newToken,
            RefreshToken = newRefreshToken,
            User = MapToDto(user)
        };
    }

    public async Task<bool> RevokeTokenAsync(Guid userId)
    {
        await Task.Delay(50);

        if (_users.TryGetValue(userId, out var user))
        {
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            return true;
        }
        return false;
    }

    public Task<User?> GetUserByIdAsync(Guid userId)
    {
        _users.TryGetValue(userId, out var user);
        return Task.FromResult(user);
    }

    public Task<User?> GetUserByUsernameAsync(string username)
    {
        if (_usernames.TryGetValue(username, out var userId))
        {
            _users.TryGetValue(userId, out var user);
            return Task.FromResult(user);
        }
        return Task.FromResult<User?>(null);
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, string oldPassword, string newPassword)
    {
        await Task.Delay(50);

        if (!_users.TryGetValue(userId, out var user))
        {
            return false;
        }

        if (!VerifyPassword(oldPassword, user.PasswordHash))
        {
            return false;
        }

        user.PasswordHash = HashPassword(newPassword);
        _logger.LogInformation("用户密码已更改: {Username}", user.Username);
        return true;
    }

    public async Task<bool> UpdateUserRoleAsync(Guid userId, UserRole newRole)
    {
        await Task.Delay(50);

        if (!_users.TryGetValue(userId, out var user))
        {
            return false;
        }

        user.Role = newRole;
        _logger.LogInformation("用户角色已更新: {Username} -> {Role}", user.Username, newRole);
        return true;
    }

    public Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return Task.FromResult(_users.Values.AsEnumerable());
    }

    public string GenerateAccessToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtConfig.Issuer,
            audience: _jwtConfig.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtConfig.AccessTokenExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.SecretKey)),
            ValidateLifetime = false
        };

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
            
            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            return principal;
        }
        catch
        {
            return null;
        }
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "MediaManager_Salt"));
        return Convert.ToBase64String(bytes);
    }

    private static bool VerifyPassword(string password, string hash)
    {
        return HashPassword(password) == hash;
    }

    private static UserDto MapToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role.ToString(),
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            Avatar = user.Avatar
        };
    }
}
