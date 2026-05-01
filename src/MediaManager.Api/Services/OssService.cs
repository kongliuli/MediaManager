using Aliyun.OSS;
using Aliyun.OSS.Common;

namespace MediaManager.Api.Services;

/// <summary>
/// 阿里云 OSS 配置
/// </summary>
public class OssConfig
{
    public string AccessKeyId { get; set; } = string.Empty;
    public string AccessKeySecret { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public string BucketName { get; set; } = string.Empty;
    public string PublicUrlPrefix { get; set; } = string.Empty;
    public bool Enabled { get; set; } = false;
}

/// <summary>
/// OSS 服务接口
/// </summary>
public interface IOssService
{
    Task<bool> UploadFileAsync(Stream fileStream, string fileName, string contentType);
    Task<bool> DeleteFileAsync(string fileName);
    Task<string> GetFileUrlAsync(string fileName);
    Task<bool> FileExistsAsync(string fileName);
    bool IsEnabled { get; }
}

/// <summary>
/// 阿里云 OSS 服务实现
/// </summary>
public class OssService : IOssService
{
    private readonly OssConfig _config;
    private readonly ILogger<OssService> _logger;
    private OssClient? _client;

    public OssService(OssConfig config, ILogger<OssService> logger)
    {
        _config = config;
        _logger = logger;

        if (_config.Enabled && !string.IsNullOrEmpty(_config.AccessKeyId))
        {
            try
            {
                _client = new OssClient(_config.Endpoint, _config.AccessKeyId, _config.AccessKeySecret);
                _logger.LogInformation("OSS 客户端初始化成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OSS 客户端初始化失败");
            }
        }
    }

    public bool IsEnabled => _config.Enabled && _client != null;

    public async Task<bool> UploadFileAsync(Stream fileStream, string fileName, string contentType)
    {
        if (!IsEnabled)
        {
            _logger.LogWarning("OSS 未启用或未配置");
            return false;
        }

        try
        {
            var objectKey = $"media/{Guid.NewGuid()}/{fileName}";
            
            var metadata = new ObjectMetadata
            {
                ContentType = contentType
            };

            await Task.Run(() =>
            {
                _client!.PutObject(_config.BucketName, objectKey, fileStream, metadata);
            });

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
            await Task.Run(() =>
            {
                _client!.DeleteObject(_config.BucketName, fileName);
            });

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
            var req = new GeneratePresignedUriRequest(_config.BucketName, fileName, SignHttpMethod.Get)
            {
                Expiration = DateTimeOffset.UtcNow.AddHours(1)
            };

            var uri = _client!.GeneratePresignedUri(req);
            return Task.FromResult(uri.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "生成预签名URL失败: {FileName}", fileName);
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
            return await Task.Run(() =>
            {
                return _client!.DoesObjectExist(_config.BucketName, fileName);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检查文件存在失败: {FileName}", fileName);
            return false;
        }
    }
}
