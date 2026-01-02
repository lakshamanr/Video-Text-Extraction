using System.Text;
using Tesseract;
using VideoTextExtraction.Models;
using VideoTextExtraction.Utilities;

namespace VideoTextExtraction.Services;

/// <summary>
/// OCR engine for extracting text from video frames
/// Useful for videos with presentations, Q&A sessions, or burned-in text
/// </summary>
public class OcrEngine : IDisposable
{
    private TesseractEngine? _engine;
    private readonly FrameExtractor _frameExtractor;

    public OcrEngine()
    {
        _frameExtractor = new FrameExtractor();
    }

    /// <summary>
    /// Extract text from video using OCR on frames
    /// </summary>
    public async Task<string> ExtractTextFromVideoAsync(string videoPath, ProcessingOptions options)
    {
        List<string> framePaths = new();

        try
        {
            // Initialize Tesseract engine
            InitializeEngine(options);

            // Extract frames from video
            framePaths = await _frameExtractor.ExtractFramesAsync(videoPath, options);

            if (!framePaths.Any())
            {
                Logger.Warning("No frames extracted from video");
                return string.Empty;
            }

            // Process each frame with OCR
            var extractedText = ProcessFrames(framePaths, options);

            return extractedText;
        }
        finally
        {
            // Clean up frames
            if (framePaths.Any())
            {
                _frameExtractor.CleanupFrames(framePaths);
            }
        }
    }

    private void InitializeEngine(ProcessingOptions options)
    {
        try
        {
            Logger.Info($"Initializing Tesseract OCR engine (language: {options.OcrLanguage})...");

            if (!Directory.Exists(options.TessDataPath))
            {
                throw new DirectoryNotFoundException(
                    $"Tesseract data directory not found: {options.TessDataPath}\n" +
                    "Please download tessdata from: https://github.com/tesseract-ocr/tessdata");
            }

            _engine = new TesseractEngine(options.TessDataPath, options.OcrLanguage, EngineMode.Default);
            Logger.Success("OCR engine initialized");
        }
        catch (Exception ex)
        {
            Logger.Error("Failed to initialize OCR engine", ex);
            throw;
        }
    }

    private string ProcessFrames(List<string> framePaths, ProcessingOptions options)
    {
        if (_engine == null)
        {
            throw new InvalidOperationException("OCR engine not initialized");
        }

        Logger.Info($"Processing {framePaths.Count} frames with OCR...");

        var result = new StringBuilder();
        var previousText = string.Empty;
        int processedCount = 0;
        int textFoundCount = 0;

        foreach (var framePath in framePaths)
        {
            try
            {
                processedCount++;

                if (processedCount % 10 == 0)
                {
                    Logger.Info($"Processing frame {processedCount}/{framePaths.Count}...");
                }

                var text = ExtractTextFromImage(framePath);

                if (string.IsNullOrWhiteSpace(text))
                {
                    continue;
                }

                // Remove duplicates from consecutive frames
                if (options.RemoveDuplicates)
                {
                    var cleanText = CleanText(text);

                    if (IsSimilarText(cleanText, previousText))
                    {
                        continue; // Skip duplicate text
                    }

                    previousText = cleanText;
                }

                textFoundCount++;

                if (options.IncludeTimestamps)
                {
                    var frameNumber = ExtractFrameNumber(framePath);
                    var timestamp = CalculateTimestamp(frameNumber, options.FramesPerSecond);
                    result.AppendLine($"[{timestamp}]");
                }

                result.AppendLine(text);
                result.AppendLine(); // Add blank line between segments
            }
            catch (Exception ex)
            {
                Logger.Warning($"Error processing frame {framePath}: {ex.Message}");
            }
        }

        Logger.Success($"OCR complete: Found text in {textFoundCount}/{processedCount} frames");

        return result.ToString();
    }

    private string ExtractTextFromImage(string imagePath)
    {
        if (_engine == null)
        {
            throw new InvalidOperationException("OCR engine not initialized");
        }

        using var img = Pix.LoadFromFile(imagePath);
        using var page = _engine.Process(img);

        var text = page.GetText();
        var confidence = page.GetMeanConfidence();

        // Only return text if confidence is reasonable
        if (confidence < 0.3f)
        {
            return string.Empty;
        }

        return text;
    }

    private string CleanText(string text)
    {
        // Remove extra whitespace and normalize
        return string.Join(" ", text.Split(new[] { ' ', '\t', '\n', '\r' },
            StringSplitOptions.RemoveEmptyEntries));
    }

    private bool IsSimilarText(string text1, string text2)
    {
        if (string.IsNullOrWhiteSpace(text1) || string.IsNullOrWhiteSpace(text2))
        {
            return false;
        }

        // Simple similarity check - can be improved with Levenshtein distance
        var similarity = CalculateSimilarity(text1, text2);
        return similarity > 0.85; // 85% similarity threshold
    }

    private double CalculateSimilarity(string text1, string text2)
    {
        var words1 = new HashSet<string>(text1.ToLower().Split(' '));
        var words2 = new HashSet<string>(text2.ToLower().Split(' '));

        if (!words1.Any() || !words2.Any())
        {
            return 0;
        }

        var intersection = words1.Intersect(words2).Count();
        var union = words1.Union(words2).Count();

        return (double)intersection / union;
    }

    private int ExtractFrameNumber(string framePath)
    {
        var fileName = Path.GetFileNameWithoutExtension(framePath);
        var numberPart = new string(fileName.Where(char.IsDigit).ToArray());

        if (int.TryParse(numberPart, out var frameNumber))
        {
            return frameNumber;
        }

        return 0;
    }

    private string CalculateTimestamp(int frameNumber, int fps)
    {
        var totalSeconds = frameNumber / fps;
        var hours = totalSeconds / 3600;
        var minutes = (totalSeconds % 3600) / 60;
        var seconds = totalSeconds % 60;

        return $"{hours:D2}:{minutes:D2}:{seconds:D2}";
    }

    public void Dispose()
    {
        _engine?.Dispose();
    }
}
