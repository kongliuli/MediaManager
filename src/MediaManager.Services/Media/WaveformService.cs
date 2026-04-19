using MediaManager.Core.Interfaces.Services;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace MediaManager.Services.Media;

/// <summary>
/// 音频波形图生成服务。
/// 使用 NAudio 读取音频采样数据，然后将其渲染为波形图 PNG。
/// </summary>
public class WaveformService : IWaveformService
{
    public async Task GenerateAsync(string audioPath, string outputPath, int width = 800, int height = 100, CancellationToken ct = default)
    {
        // 确保输出目录存在
        var outputDirectory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(outputDirectory) && !Directory.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        // 使用 NAudio 读取音频文件
        using var audioFileReader = new AudioFileReader(audioPath);
        var samples = new List<float>();
        var buffer = new float[8192]; // AudioFileReader 的 Read 方法使用 float[]
        int read;
        while ((read = audioFileReader.Read(buffer, 0, buffer.Length)) > 0)
        {
            for (int i = 0; i < read; i++)
            {
                samples.Add(buffer[i]);
            }
        }
        var sampleCount = samples.Count;
        var sampleArray = samples.ToArray();

        // 生成波形图
        using var bitmap = new Bitmap(width, height);
        using var graphics = Graphics.FromImage(bitmap);

        // 填充背景
        graphics.FillRectangle(Brushes.Black, 0, 0, width, height);

        // 计算每列的样本数
        var samplesPerColumn = sampleCount / width;
        if (samplesPerColumn < 1)
        {
            samplesPerColumn = 1;
        }

        // 绘制波形
        for (int x = 0; x < width; x++)
        {
            ct.ThrowIfCancellationRequested();

            // 计算当前列的样本范围
            var startIndex = x * samplesPerColumn;
            var endIndex = Math.Min(startIndex + samplesPerColumn, sampleCount);

            // 找到当前列的最大振幅
            float maxAmplitude = 0;
            for (int i = startIndex; i < endIndex; i++)
            {
                var amplitude = Math.Abs(sampleArray[i]);
                if (amplitude > maxAmplitude)
                {
                    maxAmplitude = amplitude;
                }
            }

            // 计算波形高度
            var waveformHeight = (int)(maxAmplitude * height / 2);
            var centerY = height / 2;

            // 绘制波形线
            using var pen = new Pen(Color.Green, 1);
            graphics.DrawLine(pen, x, centerY - waveformHeight, x, centerY + waveformHeight);
        }

        // 保存波形图
        bitmap.Save(outputPath, ImageFormat.Png);
    }
}