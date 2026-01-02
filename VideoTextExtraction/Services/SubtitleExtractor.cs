using System.Text;
using System.Text.RegularExpressions;
using Xabe.FFmpeg;
using VideoTextExtraction.Models;
using VideoTextExtraction.Utilities;

namespace VideoTextExtraction.Services;

/// <summary>
/// Extracts and processes subtitle text from videos
/// </summary>
public class SubtitleExtractor
{
    private readonly SubtitleDetector _detector;

    public SubtitleExtractor()
    {
        _detector = new SubtitleDetector();
    }

    /// <summary>
    /// Extract text from subtitles (embedded or external)
    /// </summary>
    public async Task<string> ExtractSubtitleTextAsync(string videoPath, ProcessingOptions options)
    {
        var subtitleInfo = await _detector.DetectSubtitlesAsync(videoPath, options.PreferredLanguage);

        if (!subtitleInfo.HasAnySubtitles)
        {
            Logger.Warning("No subtitles found in video");
            return string.Empty;
        }

        string subtitleText = string.Empty;

        // Try embedded subtitles first
        if (subtitleInfo.HasEmbeddedSubtitles && subtitleInfo.EmbeddedStream != null)
        {
            Logger.Info("Extracting embedded subtitles...");
            subtitleText = await ExtractEmbeddedSubtitlesAsync(videoPath, subtitleInfo.EmbeddedStream, options);
        }
        // Fall back to external subtitles
        else if (subtitleInfo.HasExternalSubtitles)
        {
            Logger.Info("Reading external subtitle file...");
            var externalPath = subtitleInfo.ExternalSubtitlePaths.First();
            subtitleText = await ParseSubtitleFileAsync(externalPath, options);
        }

        return subtitleText;
    }

    private async Task<string> ExtractEmbeddedSubtitlesAsync(
        string videoPath,
        ISubtitleStream subtitleStream,
        ProcessingOptions options)
    {
        try
        {
            // Create temporary file for extracted subtitles
            var tempDir = Path.Combine(Path.GetTempPath(), "VideoTextExtraction");
            Directory.CreateDirectory(tempDir);

            var tempSubtitlePath = Path.Combine(tempDir, $"temp_{Guid.NewGuid()}.srt");

            Logger.Info($"Extracting subtitle stream {subtitleStream.Index} to {tempSubtitlePath}");

            // Extract subtitle stream to SRT format
            var conversion = FFmpeg.Conversions.New()
                .AddStream(subtitleStream)
                .SetOutput(tempSubtitlePath);

            await conversion.Start();

            Logger.Success("Subtitle extraction complete");

            // Parse the extracted subtitle file
            var text = await ParseSubtitleFileAsync(tempSubtitlePath, options);

            // Clean up temp file
            try
            {
                File.Delete(tempSubtitlePath);
            }
            catch
            {
                // Ignore cleanup errors
            }

            return text;
        }
        catch (Exception ex)
        {
            Logger.Error("Error extracting embedded subtitles", ex);
            throw;
        }
    }

    private async Task<string> ParseSubtitleFileAsync(string subtitlePath, ProcessingOptions options)
    {
        try
        {
            var extension = Path.GetExtension(subtitlePath).ToLowerInvariant();

            return extension switch
            {
                ".srt" => await ParseSrtFileAsync(subtitlePath, options),
                ".vtt" => await ParseVttFileAsync(subtitlePath, options),
                ".ass" or ".ssa" => await ParseAssFileAsync(subtitlePath, options),
                _ => throw new NotSupportedException($"Subtitle format {extension} not supported")
            };
        }
        catch (Exception ex)
        {
            Logger.Error($"Error parsing subtitle file: {subtitlePath}", ex);
            throw;
        }
    }

    private async Task<string> ParseSrtFileAsync(string srtPath, ProcessingOptions options)
    {
        var lines = await File.ReadAllLinesAsync(srtPath);
        var result = new StringBuilder();
        var timeCodePattern = new Regex(@"\d{2}:\d{2}:\d{2},\d{3} --> \d{2}:\d{2}:\d{2},\d{3}");

        string? currentTimestamp = null;

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();

            // Skip empty lines and sequence numbers
            if (string.IsNullOrWhiteSpace(trimmedLine) || int.TryParse(trimmedLine, out _))
            {
                continue;
            }

            // Check for timestamp
            if (timeCodePattern.IsMatch(trimmedLine))
            {
                if (options.IncludeTimestamps)
                {
                    currentTimestamp = trimmedLine.Split("-->")[0].Trim();
                }
                continue;
            }

            // This is subtitle text
            if (options.IncludeTimestamps && currentTimestamp != null)
            {
                result.AppendLine($"[{currentTimestamp}] {trimmedLine}");
                currentTimestamp = null;
            }
            else
            {
                result.AppendLine(trimmedLine);
            }
        }

        return result.ToString();
    }

    private async Task<string> ParseVttFileAsync(string vttPath, ProcessingOptions options)
    {
        var lines = await File.ReadAllLinesAsync(vttPath);
        var result = new StringBuilder();
        var timeCodePattern = new Regex(@"\d{2}:\d{2}:\d{2}\.\d{3} --> \d{2}:\d{2}:\d{2}\.\d{3}");

        bool skipHeader = true;

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();

            // Skip WEBVTT header
            if (skipHeader)
            {
                if (trimmedLine.StartsWith("WEBVTT"))
                {
                    continue;
                }
                if (string.IsNullOrWhiteSpace(trimmedLine))
                {
                    skipHeader = false;
                    continue;
                }
            }

            // Skip empty lines and cue identifiers
            if (string.IsNullOrWhiteSpace(trimmedLine))
            {
                continue;
            }

            // Skip timestamps
            if (timeCodePattern.IsMatch(trimmedLine))
            {
                continue;
            }

            // This is subtitle text
            result.AppendLine(trimmedLine);
        }

        return result.ToString();
    }

    private async Task<string> ParseAssFileAsync(string assPath, ProcessingOptions options)
    {
        var lines = await File.ReadAllLinesAsync(assPath);
        var result = new StringBuilder();
        bool inEventsSection = false;

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();

            if (trimmedLine.Equals("[Events]", StringComparison.OrdinalIgnoreCase))
            {
                inEventsSection = true;
                continue;
            }

            if (trimmedLine.StartsWith("[") && inEventsSection)
            {
                break; // End of Events section
            }

            if (inEventsSection && trimmedLine.StartsWith("Dialogue:", StringComparison.OrdinalIgnoreCase))
            {
                // ASS format: Dialogue: Layer,Start,End,Style,Name,MarginL,MarginR,MarginV,Effect,Text
                var parts = trimmedLine.Split(',', 10);
                if (parts.Length >= 10)
                {
                    var text = parts[9].Trim();
                    // Remove ASS formatting tags
                    text = Regex.Replace(text, @"\{[^}]*\}", "");
                    result.AppendLine(text);
                }
            }
        }

        return result.ToString();
    }
}
