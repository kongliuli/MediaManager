using Microsoft.AspNetCore.Mvc;
using MediaManager.Api.Services;

namespace MediaManager.Api.Controllers;

/// <summary>
/// 文件上传 API 控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UploadController : ControllerBase
{
    private readonly IOssService _ossService;
    private readonly ILogger<UploadController> _logger;
    private readonly IWebHostEnvironment _environment;
    private readonly string[] _allowedExtensions = { ".mp3", ".mp4", ".wav", ".flac", ".aac", ".ogg", ".m4a", ".avi", ".mov", ".wmv", ".mkv", ".webm", ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
    private readonly long _maxFileSize = 500 * 1024 * 1024; // 500MB

    public UploadController(
        IOssService ossService,
        ILogger<UploadController> logger,
        IWebHostEnvironment environment)
    {
        _ossService = ossService;
        _logger = logger;
        _environment = environment;
    }

    /// <summary>
    /// 上传单个文件
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { Error = "未选择文件" });
        }

        if (file.Length > _maxFileSize)
        {
            return BadRequest(new { Error = $"文件大小超过限制 ({_maxFileSize / 1024 / 1024}MB)" });
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(extension))
        {
            return BadRequest(new { Error = $"不支持的文件类型: {extension}" });
        }

        try
        {
            string fileUrl;

            if (_ossService.IsEnabled)
            {
                using var stream = file.OpenReadStream();
                var success = await _ossService.UploadFileAsync(stream, file.FileName, file.ContentType);
                
                if (!success)
                {
                    return StatusCode(500, new { Error = "上传到云存储失败" });
                }

                fileUrl = await _ossService.GetFileUrlAsync($"media/{file.FileName}");
            }
            else
            {
                var uploadPath = Path.Combine(_environment.ContentRootPath, "uploads");
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                fileUrl = $"/uploads/{fileName}";
            }

            _logger.LogInformation("文件上传成功: {FileName}, URL: {Url}", file.FileName, fileUrl);

            return Ok(new
            {
                Success = true,
                FileName = file.FileName,
                FileSize = file.Length,
                ContentType = file.ContentType,
                Url = fileUrl,
                Storage = _ossService.IsEnabled ? "OSS" : "Local"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "文件上传失败: {FileName}", file.FileName);
            return StatusCode(500, new { Error = $"上传失败: {ex.Message}" });
        }
    }

    /// <summary>
    /// 批量上传文件
    /// </summary>
    [HttpPost("batch")]
    public async Task<IActionResult> UploadMultipleFiles(List<IFormFile> files)
    {
        if (files == null || files.Count == 0)
        {
            return BadRequest(new { Error = "未选择文件" });
        }

        var results = new List<object>();
        var errors = new List<string>();

        foreach (var file in files)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
            {
                errors.Add($"{file.FileName}: 不支持的文件类型");
                continue;
            }

            if (file.Length > _maxFileSize)
            {
                errors.Add($"{file.FileName}: 文件大小超过限制");
                continue;
            }

            try
            {
                string fileUrl;

                if (_ossService.IsEnabled)
                {
                    using var stream = file.OpenReadStream();
                    var success = await _ossService.UploadFileAsync(stream, file.FileName, file.ContentType);
                    
                    if (success)
                    {
                        fileUrl = await _ossService.GetFileUrlAsync($"media/{file.FileName}");
                        results.Add(new
                        {
                            Success = true,
                            FileName = file.FileName,
                            FileSize = file.Length,
                            Url = fileUrl
                        });
                    }
                    else
                    {
                        errors.Add($"{file.FileName}: 上传到云存储失败");
                    }
                }
                else
                {
                    var uploadPath = Path.Combine(_environment.ContentRootPath, "uploads");
                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }

                    var newFileName = $"{Guid.NewGuid()}{extension}";
                    var filePath = Path.Combine(uploadPath, newFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    fileUrl = $"/uploads/{newFileName}";
                    results.Add(new
                    {
                        Success = true,
                        FileName = file.FileName,
                        FileSize = file.Length,
                        Url = fileUrl
                    });
                }
            }
            catch (Exception ex)
            {
                errors.Add($"{file.FileName}: {ex.Message}");
            }
        }

        return Ok(new
        {
            TotalFiles = files.Count,
            SuccessCount = results.Count,
            FailedCount = errors.Count,
            Results = results,
            Errors = errors
        });
    }

    /// <summary>
    /// 删除上传的文件
    /// </summary>
    [HttpDelete("{filePath}")]
    public async Task<IActionResult> DeleteFile(string filePath)
    {
        try
        {
            if (_ossService.IsEnabled)
            {
                var success = await _ossService.DeleteFileAsync(filePath);
                if (!success)
                {
                    return NotFound(new { Error = "文件不存在或删除失败" });
                }
            }
            else
            {
                var fullPath = Path.Combine(_environment.ContentRootPath, filePath.TrimStart('/'));
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }
                else
                {
                    return NotFound(new { Error = "文件不存在" });
                }
            }

            return Ok(new { Success = true, Message = "文件已删除" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除文件失败: {FilePath}", filePath);
            return StatusCode(500, new { Error = $"删除失败: {ex.Message}" });
        }
    }

    /// <summary>
    /// 获取上传状态
    /// </summary>
    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        return Ok(new
        {
            OssEnabled = _ossService.IsEnabled,
            MaxFileSize = _maxFileSize,
            MaxFileSizeFormatted = $"{_maxFileSize / 1024 / 1024}MB",
            AllowedExtensions = _allowedExtensions
        });
    }
}
