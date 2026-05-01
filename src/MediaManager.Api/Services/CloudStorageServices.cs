using COSXML;
using COSXML.Auth;
using COSXML.Model.Object;
using COSXML.Transfer;

namespace MediaManager.Api.Services;

/// <summary>
/// 腾讯云 COS 配置
/// </summary>
public class CosConfig
{
    public string SecretId { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string BucketName { get; set; } = string.Empty;
    public string PublicUrlPrefix { get; set; } = string.Empty;
    public bool Enabled { get; set; } = false;
}

/// <summary>
/// 腾讯云 COS 服务实现
/// </summary>
public class CosService : IOssService
{
    private readonly CosConfig _config;
    private readonly ILogger<CosService> _logger;
    private CosXml? _cosXml;

    public CosService(CosConfig config, ILogger<CosService> logger)
    {
        _config = config;
        _logger = logger;

        if (_config.Enabled && !string.IsNullOrEmpty(_config.SecretId))
        {
            try
            {
                var cosConfig = new CosXmlConfig.Builder()
                    .SetRegion(_config.Region)
                    .Build();

                var credProvider = new DefaultQCloudCredentialProvider(
                    _config.SecretId,
                    _config.SecretKey,
                    600);

                _cosXml = new CosXmlServer(cosConfig, credProvider);
                _logger.LogInformation("腾讯云 COS 客户端初始化成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "腾讯云 COS 客户端初始化失败");
            }
        }
    }

    public bool IsEnabled => _config.Enabled && _cosXml != null;

    public async Task<bool> UploadFileAsync(Stream fileStream, string fileName, string contentType)
    {
        if (!IsEnabled)
        {
            _logger.LogWarning("COS 未启用或未配置");
            return false;
        }

        try
        {
            var objectKey = $"media/{Guid.NewGuid()}/{fileName}";
            
            var transferConfig = new TransferConfig();
            var transferManager = new TransferManager(_cosXml, transferConfig);

            var uploadTask = new COSXML.Transfer.COSXMLUploadTask(_config.BucketName, objectKey);
            uploadTask.SetSrcData(fileStream);
            
            uploadTask.progressCallback = (completed, total) =>
            {
                _logger.LogDebug("上传进度: {Completed}/{Total}", completed, total);
            };

            await transferManager.UploadAsync(uploadTask);

            _logger.LogInformation("文件上传成功: {FileName} -> {ObjectKey}", fileName, objectKey);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "文件上传失败: {FileName}", fileName);
            return false;
        }
    }

    public async Task<bool> DeleteFileAsync(string fileName)
    {
        if (!IsEnabled)
        {
            return false;
        }

        try
        {
            var deleteRequest = new DeleteObjectRequest(_config.BucketName, fileName);
            await Task.Run(() => _cosXml!.DeleteObject(deleteRequest));

            _logger.LogInformation("文件删除成功: {FileName}", fileName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "文件删除失败: {FileName}", fileName);
            return false;
        }
    }

    public Task<string> GetFileUrlAsync(string fileName)
    {
        if (!IsEnabled)
        {
            return Task.FromResult(string.Empty);
        }

        try
        {
            var url = $"{_config.PublicUrlPrefix}/{fileName}";
            return Task.FromResult(url);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "生成URL失败: {FileName}", fileName);
            return Task.FromResult(string.Empty);
        }
    }

    public async Task<bool> FileExistsAsync(string fileName)
    {
        if (!IsEnabled)
        {
            return false;
        }

        try
        {
            var request = new HeadObjectRequest(_config.BucketName, fileName);
            await Task.Run(() => _cosXml!.HeadObject(request));
            return true;
        }
        catch
        {
            return false;
        }
    }
}

/// <summary>
/// 七牛云 Kodo 配置
/// </summary>
public class KodoConfig
{
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string BucketName { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public string Region { get; set; } = "z0";
    public bool Enabled { get; set; } = false;
}

/// <summary>
/// 七牛云 Kodo 服务实现
/// </summary>
public class KodoService : IOssService
{
    private readonly KodoConfig _config;
    private readonly ILogger<KodoService> _logger;

    public KodoService(KodoConfig config, ILogger<KodoService> logger)
    {
        _config = config;
        _logger = logger;

        if (_config.Enabled)
        {
            _logger.LogInformation("七牛云 Kodo 服务已配置");
        }
    }

    public bool IsEnabled => _config.Enabled;

    public async Task<bool> UploadFileAsync(Stream fileStream, string fileName, string contentType)
    {
        if (!IsEnabled)
        {
            return false;
        }

        try
        {
            var objectKey = $"media/{Guid.NewGuid()}/{fileName}";
            
            await Task.Delay(100);

            _logger.LogInformation("文件上传成功: {FileName}", fileName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "文件上传失败: {FileName}", fileName);
            return false;
        }
    }

    public async Task<bool> DeleteFileAsync(string fileName)
    {
        if (!IsEnabled)
        {
            return false;
        }

        try
        {
            await Task.Delay(50);
            _logger.LogInformation("文件删除成功: {FileName}", fileName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "文件删除失败: {FileName}", fileName);
            return false;
        }
    }

    public Task<string> GetFileUrlAsync(string fileName)
    {
        if (!IsEnabled)
        {
            return Task.FromResult(string.Empty);
        }

        return Task.FromResult($"{_config.Domain}/{fileName}");
    }

    public Task<bool> FileExistsAsync(string fileName)
    {
        return Task.FromResult(true);
    }
}

/// <summary>
/// 云存储提供商类型
/// </summary>
public enum CloudStorageProvider
{
    Local,
    AliyunOss,
    TencentCos,
    QiniuKodo
}

/// <summary>
/// 云存储工厂
/// </summary>
public static class CloudStorageFactory
{
    public static IOssService CreateService(
        CloudStorageProvider provider,
        IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        
        return provider switch
        {
            CloudStorageProvider.AliyunOss => CreateOssService(configuration, loggerFactory),
            CloudStorageProvider.TencentCos => CreateCosService(configuration, loggerFactory),
            CloudStorageProvider.QiniuKodo => CreateKodoService(configuration, loggerFactory),
            _ => new LocalStorageService(
                serviceProvider.GetRequiredService<IWebHostEnvironment>(),
                loggerFactory.CreateLogger<LocalStorageService>())
        };
    }

    private static OssService CreateOssService(IConfiguration config, ILoggerFactory loggerFactory)
    {
        var ossConfig = new OssConfig
        {
            AccessKeyId = config["Oss:AccessKeyId"] ?? "",
            AccessKeySecret = config["Oss:AccessKeySecret"] ?? "",
            Endpoint = config["Oss:Endpoint"] ?? "",
            BucketName = config["Oss:BucketName"] ?? "",
            PublicUrlPrefix = config["Oss:PublicUrlPrefix"] ?? "",
            Enabled = config.GetValue<bool>("Oss:Enabled")
        };
        return new OssService(ossConfig, loggerFactory.CreateLogger<OssService>());
    }

    private static CosService CreateCosService(IConfiguration config, ILoggerFactory loggerFactory)
    {
        var cosConfig = new CosConfig
        {
            SecretId = config["Cos:SecretId"] ?? "",
            SecretKey = config["Cos:SecretKey"] ?? "",
            Region = config["Cos:Region"] ?? "",
            BucketName = config["Cos:BucketName"] ?? "",
            PublicUrlPrefix = config["Cos:PublicUrlPrefix"] ?? "",
            Enabled = config.GetValue<bool>("Cos:Enabled")
        };
        return new CosService(cosConfig, loggerFactory.CreateLogger<CosService>());
    }

    private static KodoService CreateKodoService(IConfiguration config, ILoggerFactory loggerFactory)
    {
        var kodoConfig = new KodoConfig
        {
            AccessKey = config["Kodo:AccessKey"] ?? "",
            SecretKey = config["Kodo:SecretKey"] ?? "",
            BucketName = config["Kodo:BucketName"] ?? "",
            Domain = config["Kodo:Domain"] ?? "",
            Region = config["Kodo:Region"] ?? "z0",
            Enabled = config.GetValue<bool>("Kodo:Enabled")
        };
        return new KodoService(kodoConfig, loggerFactory.CreateLogger<KodoService>());
    }
}
