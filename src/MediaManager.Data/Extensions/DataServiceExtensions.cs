using MediaManager.Core.Interfaces.Repositories;
using MediaManager.Data.Context;
using MediaManager.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MediaManager.Data.Extensions;

/// <summary>
/// Data 层服务注册扩展。
/// 在 App.xaml.cs 中调用 services.AddDataServices(dbPath) 即可完成注册。
/// </summary>
public static class DataServiceExtensions
{
    public static IServiceCollection AddDataServices(this IServiceCollection services, string dbPath)
    {
        services.AddDbContext<MediaDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));

        services.AddScoped<IMediaRepository, MediaRepository>();
        services.AddScoped<ITagRepository, TagRepository>();
        services.AddScoped<IPlaylistRepository, PlaylistRepository>();

        return services;
    }

    /// <summary>确保数据库已创建并应用所有迁移</summary>
    public static async Task EnsureDatabaseMigratedAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MediaDbContext>();
        await db.Database.MigrateAsync();
    }
}
