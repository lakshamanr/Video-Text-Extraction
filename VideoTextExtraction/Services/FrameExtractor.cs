using Xabe.FFmpeg;
using VideoTextExtraction.Models;
using VideoTextExtraction.Utilities;

namespace VideoTextExtraction.Services;

/// <summary>
/// Extracts video frames for OCR processing
/// </summary>
public class FrameExtractor
{
    /// <summary>
    /// Extract frames from video at specified FPS
    /// </summary>
    public async Task<List<string>> ExtractFramesAsync(string videoPath, ProcessingOptions options)
    {
        try
        {
            Logger.Info($"Extracting frames at {options.FramesPerSecond} FPS...");

            // Create output directory for frames
            var outputDir = Path.Combine(Path.GetTempPath(), "VideoTextExtraction", $"frames_{Guid.NewGuid()}");
            Directory.CreateDirectory(outputDir);

            var outputPattern = Path.Combine(outputDir, "frame_%04d.png");

            // Get video info
            var mediaInfo = await FFmpeg.GetMediaInfo(videoPath);
            var duration = mediaInfo.Duration;

            Logger.Info($"Video duration: {duration:hh\\:mm\\:ss}");

            // Calculate effective duration to process
            var processingDuration = duration;
            if (options.MaxVideoLengthMinutes > 0)
            {
                var maxDuration = TimeSpan.FromMinutes(options.MaxVideoLengthMinutes);
                if (maxDuration < duration)
                {
                    processingDuration = maxDuration;
                    Logger.Info($"Video length limit: Processing only first {options.MaxVideoLengthMinutes} minutes ({maxDuration:hh\\:mm\\:ss})");
                }
            }

            // Build FFmpeg command to extract frames
            var conversion = FFmpeg.Conversions.New()
                .AddParameter($"-i \"{videoPath}\"");

            // Add duration limit if specified
            if (options.MaxVideoLengthMinutes > 0 && processingDuration < duration)
            {
                conversion.AddParameter($"-t {(int)processingDuration.TotalSeconds}");
            }

            conversion
                .AddParameter($"-vf fps={options.FramesPerSecond}")
                .AddParameter($"\"{outputPattern}\"");

            conversion.OnProgress += (sender, args) =>
            {
                var percent = (int)(args.Duration.TotalSeconds / processingDuration.TotalSeconds * 100);
                Logger.Info($"Extracting frames... {percent}%");
            };

            await conversion.Start();

            // Get all extracted frame paths
            var framePaths = Directory.GetFiles(outputDir, "frame_*.png")
                .OrderBy(f => f)
                .ToList();

            Logger.Success($"Extracted {framePaths.Count} frames to {outputDir}");

            return framePaths;
        }
        catch (Exception ex)
        {
            Logger.Error("Error extracting frames", ex);
            throw;
        }
    }

    /// <summary>
    /// Clean up extracted frames
    /// </summary>
    public void CleanupFrames(List<string> framePaths)
    {
        if (!framePaths.Any())
        {
            return;
        }

        try
        {
            var directory = Path.GetDirectoryName(framePaths.First());
            if (directory != null && Directory.Exists(directory))
            {
                Logger.Info($"Cleaning up {framePaths.Count} frames...");
                Directory.Delete(directory, recursive: true);
                Logger.Success("Cleanup complete");
            }
        }
        catch (Exception ex)
        {
            Logger.Warning($"Failed to clean up frames: {ex.Message}");
        }
    }
}
