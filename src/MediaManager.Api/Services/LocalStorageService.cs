using Microsoft.AspNetCore.Hosting;

namespace MediaManager.Api.Services;

/// <summary>
/// 本地存储服务（当 OSS 未启用时使用）
/// </summary>
public class LocalStorageService : IOssService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<LocalStorageService> _logger;
    private readonly string _uploadPath;

    public LocalStorageService(IWebHostEnvironment environment, ILogger<LocalStorageService> logger)
    {
        _environment = environment;
        _logger = logger;
        _uploadPath = Path.Combine(_environment.ContentRootPath, "uploads");
        
        if (!Directory.Exists(_uploadPath))
        {
            Directory.CreateDirectory(_uploadPath);
        }
    }

    public bool IsEnabled => true;

    public Task<bool> UploadFileAsync(Stream fileStream, string fileName, string contentType)
    {
        try
        {
            var extension = Path.GetExtension(fileName);
            var newFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(_uploadPath, newFileName);

            using var outputStream = new FileStream(filePath, FileMode.Create);
            fileStream.CopyTo(outputStream);

            _logger.LogInformation("文件上传成功: {FileName} -> {FilePath}", fileName, newFileName);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "文件上传失败: {FileName}", fileName);
            return Task.FromResult(false);
        }
    }

    public Task<bool> DeleteFileAsync(string fileName)
    {
        try
        {
            var filePath = Path.Combine(_uploadPath, fileName);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
                _logger.LogInformation("文件删除成功: {FileName}", fileName);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "文件删除失败: {FileName}", fileName);
            return Task.FromResult(false);
        }
    }

    public Task<string> GetFileUrlAsync(string fileName)
    {
        return Task.FromResult($"/uploads/{fileName}");
    }

    public Task<bool> FileExistsAsync(string fileName)
    {
        var filePath = Path.Combine(_uploadPath, fileName);
        return Task.FromResult(System.IO.File.Exists(filePath));
    }
}
