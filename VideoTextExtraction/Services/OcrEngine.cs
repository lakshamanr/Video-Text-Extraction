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
        Logger.Info($"Comparison method: {(options.UseImageComparison ? "Visual (Image)" : "Text-based")}");
        Logger.Info($"Output mode: {(options.SaveSeparateFiles ? "Separate files" : "Combined file")}");

        var result = new StringBuilder();
        var previousText = string.Empty;
        string? previousImagePath = null;
        int processedCount = 0;
        int textFoundCount = 0;
        int duplicatesSkipped = 0;
        int lowConfidenceSkipped = 0;
        int uniqueFrameNumber = 1;

        // Create output directory for separate files if needed
        string? separateFilesDir = null;
        if (options.SaveSeparateFiles)
        {
            var baseDir = Path.GetDirectoryName(options.OutputPath) ?? ".";
            var baseName = Path.GetFileNameWithoutExtension(options.OutputPath);
            separateFilesDir = Path.Combine(baseDir, $"{baseName}_frames");
            Directory.CreateDirectory(separateFilesDir);
            Logger.Info($"Saving separate files to: {separateFilesDir}");
        }

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

                // Check for duplicate images BEFORE OCR (more efficient)
                if (options.RemoveDuplicates && options.UseImageComparison && previousImagePath != null)
                {
                    if (AreImagesSimilar(previousImagePath, framePath, options.ImageSimilarityThreshold))
                    {
                        duplicatesSkipped++;
                        continue; // Skip duplicate image
                    }
                }

                var text = ExtractTextFromImage(framePath);

                if (string.IsNullOrWhiteSpace(text))
                {
                    lowConfidenceSkipped++;
                    continue;
                }

                // Text-based duplicate check (if not using image comparison)
                if (options.RemoveDuplicates && !options.UseImageComparison)
                {
                    var cleanText = CleanText(text);

                    if (IsSimilarText(cleanText, previousText))
                    {
                        duplicatesSkipped++;
                        continue; // Skip duplicate text
                    }

                    previousText = cleanText;
                }
                else if (!options.RemoveDuplicates)
                {
                    previousText = CleanText(text);
                }

                // This is a unique frame - save it
                textFoundCount++;
                previousImagePath = framePath;
                previousText = CleanText(text);

                // Build output
                var frameOutput = new StringBuilder();

                if (options.IncludeTimestamps)
                {
                    var frameNum = ExtractFrameNumber(framePath);
                    var timestamp = CalculateTimestamp(frameNum, options.FramesPerSecond);
                    frameOutput.AppendLine($"[{timestamp}]");
                }

                frameOutput.AppendLine(text.Trim());

                // Save to separate file or combined file
                if (options.SaveSeparateFiles && separateFilesDir != null)
                {
                    var fileName = $"frame_{uniqueFrameNumber:D4}.txt";
                    var filePath = Path.Combine(separateFilesDir, fileName);
                    File.WriteAllText(filePath, frameOutput.ToString());
                }
                else
                {
                    result.AppendLine(frameOutput.ToString());
                    result.AppendLine(); // Add blank line between segments
                }

                uniqueFrameNumber++;
            }
            catch (Exception ex)
            {
                Logger.Warning($"Error processing frame {Path.GetFileName(framePath)}: {ex.Message}");
            }
        }

        Logger.Success($"OCR complete:");
        Logger.Info($"  - Frames processed: {processedCount}");
        Logger.Info($"  - Unique frames found: {textFoundCount}");
        Logger.Info($"  - Low confidence skipped: {lowConfidenceSkipped}");
        Logger.Info($"  - Duplicates skipped: {duplicatesSkipped}");

        if (options.SaveSeparateFiles && separateFilesDir != null)
        {
            Logger.Success($"Saved {textFoundCount} separate files to: {separateFilesDir}");
        }

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

    private bool AreImagesSimilar(string imagePath1, string imagePath2, double threshold)
    {
        try
        {
            using var img1 = Pix.LoadFromFile(imagePath1);
            using var img2 = Pix.LoadFromFile(imagePath2);

            // Quick check: if dimensions are different, images are different
            if (img1.Width != img2.Width || img1.Height != img2.Height)
            {
                return false;
            }

            // Convert both images to grayscale for comparison
            using var gray1 = img1.Depth == 8 ? img1.Clone() : img1.ConvertRGBToGray();
            using var gray2 = img2.Depth == 8 ? img2.Clone() : img2.ConvertRGBToGray();

            if (gray1 == null || gray2 == null)
            {
                return false;
            }

            // Calculate structural similarity using pixel-by-pixel comparison
            // For performance, sample every Nth pixel
            var sampleRate = 10; // Sample every 10th pixel
            var width = gray1.Width;
            var height = gray1.Height;
            var totalSamples = 0;
            var matchingSamples = 0;

            for (var y = 0; y < height; y += sampleRate)
            {
                for (var x = 0; x < width; x += sampleRate)
                {
                    totalSamples++;

                    var pixel1 = gray1.GetPixel(x, y);
                    var pixel2 = gray2.GetPixel(x, y);

                    // Compare grayscale values (allow small difference for tolerance)
                    var diff = Math.Abs(pixel1 - pixel2);
                    if (diff < 15) // Tolerance of 15 grayscale levels (out of 256)
                    {
                        matchingSamples++;
                    }
                }
            }

            var similarity = (double)matchingSamples / totalSamples;
            return similarity >= threshold;
        }
        catch (Exception ex)
        {
            Logger.Warning($"Error comparing images: {ex.Message}");
            return false; // If comparison fails, treat as different
        }
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
