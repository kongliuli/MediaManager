using MediaManager.Core.DTOs;
using MediaManager.Core.Enums;
using MediaManager.Core.Interfaces.Services;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace MediaManager.Services.Metadata;

/// <summary>
/// FFprobe 元数据提取服务。
/// 通过执行 ffprobe 命令解析媒体文件信息。
/// FFmpeg 运行时（ffmpeg.exe / ffprobe.exe）需放置在应用目录或系统 PATH 中。
/// </summary>
public class FfprobeMetadataService : IMetadataService
{
    public async Task<MetadataResult> ExtractAsync(string filePath, CancellationToken ct = default)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "ffprobe",
            Arguments = $"-v quiet -print_format json -show_streams -show_format \"{filePath}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(processStartInfo);
        if (process == null)
        {
            throw new InvalidOperationException("无法启动 ffprobe 进程");
        }

        using var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cancellationTokenSource.CancelAfter(TimeSpan.FromMinutes(2)); // 2分钟超时

        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();
        var exitTask = Task.Run(() => process.WaitForExit(), cancellationTokenSource.Token);

        var completedTask = await Task.WhenAny(outputTask, errorTask, exitTask);

        if (cancellationTokenSource.Token.IsCancellationRequested)
        {
            process.Kill();
            throw new OperationCanceledException(ct);
        }

        if (process.ExitCode != 0)
        {
            var error = await errorTask;
            throw new Exception($"ffprobe 执行失败: {error}");
        }

        var output = await outputTask;
        return ParseMetadata(output, filePath);
    }

    private MetadataResult ParseMetadata(string jsonOutput, string filePath)
    {
        var document = JsonDocument.Parse(jsonOutput);
        var root = document.RootElement;

        var result = new MetadataResult
        {
            FileSize = new FileInfo(filePath).Length
        };

        // 解析 format 节点
        if (root.TryGetProperty("format", out var format))
        {
            if (format.TryGetProperty("duration", out var durationElement) && double.TryParse(durationElement.GetString(), out var duration))
            {
                result.DurationSeconds = duration;
            }

            if (format.TryGetProperty("tags", out var tags))
            {
                result.Title = GetPropertyOrDefault(tags, "title");
                result.Artist = GetPropertyOrDefault(tags, "artist");
                result.Album = GetPropertyOrDefault(tags, "album");
                if (tags.TryGetProperty("date", out var dateElement))
                {
                    var date = dateElement.GetString();
                    if (!string.IsNullOrEmpty(date) && int.TryParse(date.Substring(0, Math.Min(4, date.Length)), out var year))
                    {
                        result.Year = year;
                    }
                }
                result.Genre = GetPropertyOrDefault(tags, "genre");
            }
        }

        // 解析 streams 节点
        if (root.TryGetProperty("streams", out var streams))
        {
            foreach (var stream in streams.EnumerateArray())
            {
                if (stream.TryGetProperty("codec_type", out var codecTypeElement))
                {
                    var codecType = codecTypeElement.GetString();

                    if (codecType == "audio")
                    {
                        result.MediaType = MediaType.Audio;
                        result.AudioCodec = GetPropertyOrDefault(stream, "codec_name");
                        if (stream.TryGetProperty("bit_rate", out var bitRateElement) && int.TryParse(bitRateElement.GetString(), out var bitRate))
                        {
                            result.BitRate = bitRate / 1000; // 转换为 kbps
                        }
                        if (stream.TryGetProperty("sample_rate", out var sampleRateElement) && int.TryParse(sampleRateElement.GetString(), out var sampleRate))
                        {
                            result.SampleRate = sampleRate;
                        }
                        if (stream.TryGetProperty("channels", out var channelsElement) && int.TryParse(channelsElement.GetString(), out var channels))
                        {
                            result.Channels = channels;
                        }
                    }
                    else if (codecType == "video")
                    {
                        result.MediaType = MediaType.Video;
                        result.VideoCodec = GetPropertyOrDefault(stream, "codec_name");
                        if (stream.TryGetProperty("width", out var widthElement) && int.TryParse(widthElement.GetString(), out var width))
                        {
                            result.Width = width;
                        }
                        if (stream.TryGetProperty("height", out var heightElement) && int.TryParse(heightElement.GetString(), out var height))
                        {
                            result.Height = height;
                        }
                        if (stream.TryGetProperty("r_frame_rate", out var frameRateElement))
                        {
                            var frameRateStr = frameRateElement.GetString();
                            if (!string.IsNullOrEmpty(frameRateStr) && frameRateStr.Contains('/'))
                            {
                                var parts = frameRateStr.Split('/');
                                if (int.TryParse(parts[0], out var numerator) && int.TryParse(parts[1], out var denominator) && denominator > 0)
                                {
                                    result.FrameRate = (double)numerator / denominator;
                                }
                            }
                        }
                        if (stream.TryGetProperty("bit_rate", out var videoBitRateElement) && int.TryParse(videoBitRateElement.GetString(), out var videoBitRate))
                        {
                            result.VideoBitRate = videoBitRate / 1000; // 转换为 kbps
                        }
                    }
                }
            }
        }

        // 如果没有检测到媒体类型，根据文件扩展名判断
        if (result.MediaType == 0) // 默认值 0 表示未知媒体类型
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            if (new[] { ".mp3", ".wav", ".flac", ".aac", ".ogg", ".wma", ".m4a", ".opus" }.Contains(extension))
            {
                result.MediaType = MediaType.Audio;
            }
            else if (new[] { ".mp4", ".avi", ".mov", ".wmv", ".flv", ".mkv", ".webm", ".m4v" }.Contains(extension))
            {
                result.MediaType = MediaType.Video;
            }
        }

        return result;
    }

    private string? GetPropertyOrDefault(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) ? property.GetString() : null;
    }
}
