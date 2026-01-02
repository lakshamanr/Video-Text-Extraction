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

            // Configure Tesseract for better accuracy
            _engine.SetVariable("tessedit_char_whitelist", "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789 .,?!:;-()[]{}\"'/@#$%&*+=");
            _engine.SetVariable("preserve_interword_spaces", "1");

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
        Logger.Info($"Duplicate removal: {(options.RemoveDuplicates ? "Enabled" : "Disabled")}");

        var result = new StringBuilder();
        var previousText = string.Empty;
        int processedCount = 0;
        int textFoundCount = 0;
        int duplicatesSkipped = 0;
        int lowConfidenceSkipped = 0;

        foreach (var framePath in framePaths)
        {
            try
            {
                processedCount++;

                if (processedCount % 10 == 0 || processedCount == 1)
                {
                    var percent = (int)((double)processedCount / framePaths.Count * 100);
                    Logger.Info($"Processing frame {processedCount}/{framePaths.Count} ({percent}%)...");
                }

                var text = ExtractTextFromImage(framePath);

                if (string.IsNullOrWhiteSpace(text))
                {
                    lowConfidenceSkipped++;
                    continue;
                }

                // Remove duplicates from consecutive frames
                if (options.RemoveDuplicates)
                {
                    var cleanText = CleanText(text);

                    if (IsSimilarText(cleanText, previousText))
                    {
                        duplicatesSkipped++;
                        continue; // Skip duplicate text
                    }

                    previousText = cleanText;
                }
                else
                {
                    previousText = CleanText(text);
                }

                textFoundCount++;

                if (options.IncludeTimestamps)
                {
                    var frameNumber = ExtractFrameNumber(framePath);
                    var timestamp = CalculateTimestamp(frameNumber, options.FramesPerSecond);
                    result.AppendLine($"[{timestamp}]");
                }

                result.AppendLine(text.Trim());
                result.AppendLine(); // Add blank line between segments
            }
            catch (Exception ex)
            {
                Logger.Warning($"Error processing frame {Path.GetFileName(framePath)}: {ex.Message}");
            }
        }

        Logger.Success($"OCR complete:");
        Logger.Info($"  - Frames processed: {processedCount}");
        Logger.Info($"  - Text segments found: {textFoundCount}");
        Logger.Info($"  - Low confidence skipped: {lowConfidenceSkipped}");
        Logger.Info($"  - Duplicates skipped: {duplicatesSkipped}");

        return result.ToString();
    }

    private string ExtractTextFromImage(string imagePath)
    {
        if (_engine == null)
        {
            throw new InvalidOperationException("OCR engine not initialized");
        }

        using var img = Pix.LoadFromFile(imagePath);

        // Preprocess image for better OCR
        using var processed = PreprocessImage(img);

        // Use PSM_AUTO_OSD for automatic page segmentation with orientation detection
        using var page = _engine.Process(processed, PageSegMode.Auto);

        var text = page.GetText();
        var confidence = page.GetMeanConfidence();

        // Only return text if confidence is reasonable (raised threshold for better quality)
        if (confidence < 0.6f)
        {
            return string.Empty;
        }

        // Clean up the text
        text = text?.Trim() ?? string.Empty;

        return text;
    }

    private Pix PreprocessImage(Pix original)
    {
        // Create a copy to work with
        var processed = original;

        try
        {
            // Convert to grayscale if not already
            if (processed.Depth != 8)
            {
                var gray = processed.ConvertRGBToGray();
                if (gray != null)
                {
                    processed = gray;
                }
            }

            // Increase contrast and brightness
            var enhanced = processed.UnsharpMaskingGray(5, 2.5f);
            if (enhanced != null)
            {
                processed = enhanced;
            }

            // Binarize (convert to black and white) for better OCR
            var binarized = processed.BinarizeOtsuAdaptiveThreshold(
                2000, 2000, 0, 0, 0.1f);
            if (binarized != null)
            {
                processed = binarized;
            }
        }
        catch
        {
            // If preprocessing fails, return original
            return original;
        }

        return processed;
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

        // Normalize texts
        var norm1 = NormalizeForComparison(text1);
        var norm2 = NormalizeForComparison(text2);

        // Exact match after normalization
        if (norm1 == norm2)
        {
            return true;
        }

        // Use Levenshtein distance for similarity
        var similarity = CalculateSimilarity(norm1, norm2);
        return similarity > 0.90; // 90% similarity threshold (stricter)
    }

    private string NormalizeForComparison(string text)
    {
        // Remove extra whitespace, lowercase, remove special chars
        var normalized = text.ToLower().Trim();
        normalized = System.Text.RegularExpressions.Regex.Replace(normalized, @"\s+", " ");
        normalized = System.Text.RegularExpressions.Regex.Replace(normalized, @"[^\w\s]", "");
        return normalized;
    }

    private double CalculateSimilarity(string text1, string text2)
    {
        // Use Levenshtein distance for better accuracy
        var distance = LevenshteinDistance(text1, text2);
        var maxLength = Math.Max(text1.Length, text2.Length);

        if (maxLength == 0)
        {
            return 1.0;
        }

        return 1.0 - ((double)distance / maxLength);
    }

    private int LevenshteinDistance(string s1, string s2)
    {
        if (string.IsNullOrEmpty(s1))
        {
            return s2?.Length ?? 0;
        }

        if (string.IsNullOrEmpty(s2))
        {
            return s1.Length;
        }

        var d = new int[s1.Length + 1, s2.Length + 1];

        for (var i = 0; i <= s1.Length; i++)
        {
            d[i, 0] = i;
        }

        for (var j = 0; j <= s2.Length; j++)
        {
            d[0, j] = j;
        }

        for (var i = 1; i <= s1.Length; i++)
        {
            for (var j = 1; j <= s2.Length; j++)
            {
                var cost = (s2[j - 1] == s1[i - 1]) ? 0 : 1;

                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
            }
        }

        return d[s1.Length, s2.Length];
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
