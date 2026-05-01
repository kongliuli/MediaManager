using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using MediaManager.Api.Services;
using MediaManager.Api.Models;

namespace MediaManager.Api.Controllers;

/// <summary>
/// 认证 API 控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// 用户登录
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { Error = "用户名和密码不能为空" });
        }

        var result = await _authService.LoginAsync(request.Username, request.Password);
        
        if (!result.Success)
        {
            return Unauthorized(new { Error = result.Message });
        }

        return Ok(result);
    }

    /// <summary>
    /// 用户注册
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || 
            string.IsNullOrWhiteSpace(request.Email) || 
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { Error = "所有字段都是必填的" });
        }

        if (request.Password != request.ConfirmPassword)
        {
            return BadRequest(new { Error = "两次输入的密码不一致" });
        }

        if (request.Password.Length < 6)
        {
            return BadRequest(new { Error = "密码长度至少6位" });
        }

        var result = await _authService.RegisterAsync(request.Username, request.Email, request.Password);
        
        if (!result.Success)
        {
            return BadRequest(new { Error = result.Message });
        }

        return Ok(result);
    }

    /// <summary>
    /// 刷新令牌
    /// </summary>
    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var result = await _authService.RefreshTokenAsync(request.Token, request.RefreshToken);
        
        if (!result.Success)
        {
            return Unauthorized(new { Error = result.Message });
        }

        return Ok(result);
    }

    /// <summary>
    /// 注销登录
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
        {
            await _authService.RevokeTokenAsync(userId);
        }
        return Ok(new { Message = "注销成功" });
    }

    /// <summary>
    /// 获取当前用户信息
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized();
        }

        var user = await _authService.GetUserByIdAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        return Ok(new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role.ToString(),
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            Avatar = user.Avatar
        });
    }

    /// <summary>
    /// 修改密码
    /// </summary>
    [HttpPut("password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized();
        }

        if (request.NewPassword.Length < 6)
        {
            return BadRequest(new { Error = "新密码长度至少6位" });
        }

        var success = await _authService.ChangePasswordAsync(userId, request.OldPassword, request.NewPassword);
        if (!success)
        {
            return BadRequest(new { Error = "原密码错误" });
        }

        return Ok(new { Message = "密码修改成功" });
    }

    /// <summary>
    /// 获取所有用户（管理员）
    /// </summary>
    [HttpGet("users")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _authService.GetAllUsersAsync();
        var userDtos = users.Select(u => new UserDto
        {
            Id = u.Id,
            Username = u.Username,
            Email = u.Email,
            Role = u.Role.ToString(),
            CreatedAt = u.CreatedAt,
            LastLoginAt = u.LastLoginAt,
            Avatar = u.Avatar
        });
        return Ok(userDtos);
    }

    /// <summary>
    /// 更新用户角色（管理员）
    /// </summary>
    [HttpPut("users/{userId}/role")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> UpdateUserRole(Guid userId, [FromBody] UpdateRoleRequest request)
    {
        var success = await _authService.UpdateUserRoleAsync(userId, request.Role);
        if (!success)
        {
            return NotFound(new { Error = "用户不存在" });
        }
        return Ok(new { Message = "角色更新成功" });
    }
}

/// <summary>
/// 修改密码请求
/// </summary>
public class ChangePasswordRequest
{
    public string OldPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

/// <summary>
/// 更新角色请求
/// </summary>
public class UpdateRoleRequest
{
    public UserRole Role { get; set; }
}
