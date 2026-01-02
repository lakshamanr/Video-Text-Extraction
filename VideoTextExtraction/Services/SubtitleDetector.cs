using Xabe.FFmpeg;
using VideoTextExtraction.Utilities;

namespace VideoTextExtraction.Services;

/// <summary>
/// Detects embedded and external subtitles for video files
/// </summary>
public class SubtitleDetector
{
    /// <summary>
    /// Detect all available subtitles (embedded and external)
    /// </summary>
    public async Task<SubtitleInfo> DetectSubtitlesAsync(string videoPath, string? preferredLanguage = null)
    {
        var info = new SubtitleInfo();

        if (!File.Exists(videoPath))
        {
            Logger.Error($"Video file not found: {videoPath}");
            return info;
        }

        // Check for embedded subtitles
        await DetectEmbeddedSubtitlesAsync(videoPath, info, preferredLanguage);

        // Check for external subtitle files
        DetectExternalSubtitles(videoPath, info);

        return info;
    }

    private async Task DetectEmbeddedSubtitlesAsync(string videoPath, SubtitleInfo info, string? preferredLanguage)
    {
        try
        {
            Logger.Info("Checking for embedded subtitles...");
            var mediaInfo = await FFmpeg.GetMediaInfo(videoPath);

            var subtitleStreams = mediaInfo.SubtitleStreams.ToList();

            if (subtitleStreams.Any())
            {
                Logger.Success($"Found {subtitleStreams.Count} embedded subtitle stream(s)");

                foreach (var stream in subtitleStreams)
                {
                    var language = stream.Language ?? "unknown";
                    Logger.Info($"  - Stream {stream.Index}: {language} ({stream.Codec})");
                }

                // Select preferred stream
                if (!string.IsNullOrEmpty(preferredLanguage))
                {
                    info.EmbeddedStream = subtitleStreams
                        .FirstOrDefault(s => s.Language?.Equals(preferredLanguage, StringComparison.OrdinalIgnoreCase) == true)
                        ?? subtitleStreams.First();
                }
                else
                {
                    info.EmbeddedStream = subtitleStreams.First();
                }

                info.HasEmbeddedSubtitles = true;
                Logger.Success($"Selected embedded subtitle: {info.EmbeddedStream.Language ?? "unknown"}");
            }
            else
            {
                Logger.Info("No embedded subtitles found");
            }
        }
        catch (Exception ex)
        {
            Logger.Error("Error detecting embedded subtitles", ex);
        }
    }

    private void DetectExternalSubtitles(string videoPath, SubtitleInfo info)
    {
        try
        {
            Logger.Info("Checking for external subtitle files...");

            var directory = Path.GetDirectoryName(videoPath) ?? ".";
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(videoPath);

            var subtitleExtensions = new[] { ".srt", ".vtt", ".ass", ".ssa" };
            var externalSubtitles = new List<string>();

            foreach (var ext in subtitleExtensions)
            {
                var subtitlePath = Path.Combine(directory, fileNameWithoutExt + ext);
                if (File.Exists(subtitlePath))
                {
                    externalSubtitles.Add(subtitlePath);
                    Logger.Success($"Found external subtitle: {Path.GetFileName(subtitlePath)}");
                }
            }

            if (externalSubtitles.Any())
            {
                info.HasExternalSubtitles = true;
                info.ExternalSubtitlePaths = externalSubtitles;
            }
            else
            {
                Logger.Info("No external subtitle files found");
            }
        }
        catch (Exception ex)
        {
            Logger.Error("Error detecting external subtitles", ex);
        }
    }
}

/// <summary>
/// Information about detected subtitles
/// </summary>
public class SubtitleInfo
{
    public bool HasEmbeddedSubtitles { get; set; }
    public bool HasExternalSubtitles { get; set; }
    public ISubtitleStream? EmbeddedStream { get; set; }
    public List<string> ExternalSubtitlePaths { get; set; } = new();

    public bool HasAnySubtitles => HasEmbeddedSubtitles || HasExternalSubtitles;
}
