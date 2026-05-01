using MediaManager.Api.BackgroundServices;
using MediaManager.Api.Hubs;
using MediaManager.Data.Extensions;
using MediaManager.Services.Extensions;
using MediaManager.Api.Components;
using MediaManager.Api.Services;
using Serilog;

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
builder.Services.AddSwaggerGen();

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

// 配置 OSS 服务
var ossConfig = new OssConfig
{
    AccessKeyId = builder.Configuration.GetValue<string>("Oss:AccessKeyId") ?? "",
    AccessKeySecret = builder.Configuration.GetValue<string>("Oss:AccessKeySecret") ?? "",
    Endpoint = builder.Configuration.GetValue<string>("Oss:Endpoint") ?? "",
    BucketName = builder.Configuration.GetValue<string>("Oss:BucketName") ?? "",
    PublicUrlPrefix = builder.Configuration.GetValue<string>("Oss:PublicUrlPrefix") ?? "",
    Enabled = builder.Configuration.GetValue<bool>("Oss:Enabled")
};

if (ossConfig.Enabled)
{
    builder.Services.AddSingleton(ossConfig);
    builder.Services.AddSingleton<IOssService, OssService>();
    Log.Information("阿里云 OSS 服务已启用");
}
else
{
    builder.Services.AddSingleton<IOssService>(new LocalStorageService());
    Log.Information("使用本地存储服务");
}

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

app.UseAuthorization();

app.MapControllers();

// 映射 SignalR Hub
app.MapHub<ScanProgressHub>("/hubs/scan-progress");

// 映射 Blazor 组件
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
