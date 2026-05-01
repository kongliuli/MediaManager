using MediaManager.Api.BackgroundServices;
using MediaManager.Api.Hubs;
using MediaManager.Data.Extensions;
using MediaManager.Services.Extensions;
using MediaManager.Api.Components;
using MediaManager.Api.Services;
using MediaManager.Api.Models;
using Serilog;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// 配置 Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/api-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// 添加服务到容器
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MediaManager API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// 配置 JWT 认证
var jwtConfig = new JwtConfig
{
    SecretKey = builder.Configuration["Jwt:SecretKey"] ?? "MediaManager_Default_Secret_Key_2024!",
    Issuer = builder.Configuration["Jwt:Issuer"] ?? "MediaManager",
    Audience = builder.Configuration["Jwt:Audience"] ?? "MediaManager",
    AccessTokenExpirationMinutes = builder.Configuration.GetValue<int>("Jwt:AccessTokenExpirationMinutes", 60),
    RefreshTokenExpirationDays = builder.Configuration.GetValue<int>("Jwt:RefreshTokenExpirationDays", 7)
};

builder.Services.AddSingleton(jwtConfig);
builder.Services.AddSingleton<IAuthService, AuthService>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtConfig.Issuer,
        ValidAudience = jwtConfig.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.SecretKey))
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdministratorRole", policy => policy.RequireRole("Administrator"));
    options.AddPolicy("RequireModeratorRole", policy => policy.RequireRole("Moderator", "Administrator"));
});

// 添加 Blazor Server 服务
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// 配置 CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 添加 SignalR
builder.Services.AddSignalR();

// 添加数据服务
var appConfig = new MediaManager.Core.AppConfig
{
    DatabasePath = builder.Configuration.GetValue<string>("AppConfig:DatabasePath") ?? "media.db",
    ThumbnailDirectory = builder.Configuration.GetValue<string>("AppConfig:ThumbnailDirectory") ?? "thumbnails",
    FfmpegPath = builder.Configuration.GetValue<string>("AppConfig:FfmpegPath") ?? "ffmpeg",
    FfprobePath = builder.Configuration.GetValue<string>("AppConfig:FfprobePath") ?? "ffprobe"
};

// 确保缩略图目录存在
if (!Directory.Exists(appConfig.ThumbnailDirectory))
{
    Directory.CreateDirectory(appConfig.ThumbnailDirectory);
}

builder.Services.AddSingleton(appConfig);
builder.Services.AddDataServices(appConfig.DatabasePath);
builder.Services.AddMediaServices();

// 配置云存储服务
var storageProvider = builder.Configuration.GetValue<string>("Storage:Provider") ?? "Local";
var cloudProvider = Enum.Parse<CloudStorageProvider>(storageProvider, true);
builder.Services.AddSingleton<IOssService>(sp => 
    CloudStorageFactory.CreateService(cloudProvider, sp, builder.Configuration));

// 添加后台服务
builder.Services.AddHostedService<ScanBackgroundService>();

// 添加扫描任务管理器
builder.Services.AddSingleton<ScanTaskManager>();

var app = builder.Build();

// 确保数据库已迁移
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<MediaManager.Data.Context.MediaDbContext>();
    dbContext.Database.EnsureCreated();
}

// 配置 HTTP 请求管道
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// 映射 SignalR Hub
app.MapHub<ScanProgressHub>("/hubs/scan-progress");

// 映射 Blazor 组件
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

Log.Information("MediaManager Web API 已启动");

app.Run();
