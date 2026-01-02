using System.Diagnostics;
using VideoTextExtraction.Models;
using VideoTextExtraction.Utilities;

namespace VideoTextExtraction.Services;

/// <summary>
/// Main orchestrator for video text extraction
/// Coordinates subtitle extraction and OCR fallback
/// </summary>
public class VideoTextExtractor
{
    private readonly SubtitleDetector _subtitleDetector;
    private readonly SubtitleExtractor _subtitleExtractor;

    public VideoTextExtractor()
    {
        _subtitleDetector = new SubtitleDetector();
        _subtitleExtractor = new SubtitleExtractor();
    }

    /// <summary>
    /// Extract text from video using best available method
    /// </summary>
    public async Task<ExtractionResult> ExtractTextAsync(ProcessingOptions options)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = new ExtractionResult
        {
            OutputPath = options.OutputPath
        };

        try
        {
            // Validate input
            if (!File.Exists(options.VideoPath))
            {
                result.Success = false;
                result.ErrorMessage = $"Video file not found: {options.VideoPath}";
                Logger.Error(result.ErrorMessage);
                return result;
            }

            Logger.Info($"Processing video: {Path.GetFileName(options.VideoPath)}");
            Logger.Info($"Output: {options.OutputPath}");

            // Check for subtitles first
            var subtitleInfo = await _subtitleDetector.DetectSubtitlesAsync(
                options.VideoPath,
                options.PreferredLanguage);

            string extractedText = string.Empty;
            ExtractionMethod method = ExtractionMethod.None;

            // Try subtitle extraction (unless forced to use OCR)
            if (!options.ForceOcr && subtitleInfo.HasAnySubtitles)
            {
                Logger.Info("Using subtitle extraction method");

                extractedText = await _subtitleExtractor.ExtractSubtitleTextAsync(
                    options.VideoPath,
                    options);

                method = subtitleInfo.HasEmbeddedSubtitles
                    ? ExtractionMethod.EmbeddedSubtitles
                    : ExtractionMethod.ExternalSubtitles;
            }
            // Fall back to OCR
            else
            {
                if (options.ForceOcr)
                {
                    Logger.Info("Force OCR mode enabled");
                }
                else
                {
                    Logger.Info("No subtitles found - using OCR method");
                }

                using var ocrEngine = new OcrEngine();
                extractedText = await ocrEngine.ExtractTextFromVideoAsync(
                    options.VideoPath,
                    options);

                method = ExtractionMethod.Ocr;
            }

            // Save output
            if (!string.IsNullOrWhiteSpace(extractedText))
            {
                await SaveOutputAsync(extractedText, options);

                result.Success = true;
                result.Method = method;
                result.ExtractedText = extractedText;
                result.SegmentCount = CountSegments(extractedText);

                Logger.Success($"Text extraction complete using {method}");
                Logger.Success($"Extracted {result.SegmentCount} text segments");
                Logger.Success($"Output saved to: {options.OutputPath}");
            }
            else
            {
                result.Success = false;
                result.ErrorMessage = "No text extracted from video";
                Logger.Warning(result.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            Logger.Error("Extraction failed", ex);
        }
        finally
        {
            stopwatch.Stop();
            result.ProcessingTimeSeconds = stopwatch.Elapsed.TotalSeconds;
            Logger.Info($"Total processing time: {stopwatch.Elapsed:mm\\:ss}");
        }

        return result;
    }

    private async Task SaveOutputAsync(string text, ProcessingOptions options)
    {
        try
        {
            var outputDir = Path.GetDirectoryName(options.OutputPath);
            if (!string.IsNullOrEmpty(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            await File.WriteAllTextAsync(options.OutputPath, text);
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to save output to {options.OutputPath}", ex);
            throw;
        }
    }

    private int CountSegments(string text)
    {
        return text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
            .Count(line => !string.IsNullOrWhiteSpace(line));
    }
}
