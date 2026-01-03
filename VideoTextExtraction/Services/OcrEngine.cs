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

        if (options.MaxDurationSeconds > 0)
        {
            var maxTime = TimeSpan.FromSeconds(options.MaxDurationSeconds);
            Logger.Info($"Time limit: {maxTime:hh\\:mm\\:ss}");
        }

        if (options.StopAfterEmptyFrames > 0)
        {
            Logger.Info($"Auto-stop after {options.StopAfterEmptyFrames} consecutive empty frames");
        }

        var result = new StringBuilder();
        var previousText = string.Empty;
        string? previousImagePath = null;
        int processedCount = 0;
        int textFoundCount = 0;
        int duplicatesSkipped = 0;
        int lowConfidenceSkipped = 0;
        int uniqueFrameNumber = 1;
        int consecutiveEmptyFrames = 0;
        string lastTextFoundTimestamp = "00:00:00";
        string firstTextFoundTimestamp = "";

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

                // Get current frame timestamp
                var currentFrameNum = ExtractFrameNumber(framePath);
                var currentTimestamp = CalculateTimestamp(currentFrameNum, options.FramesPerSecond);
                var currentSeconds = currentFrameNum / options.FramesPerSecond;

                // Check time limit
                if (options.MaxDurationSeconds > 0 && currentSeconds > options.MaxDurationSeconds)
                {
                    Logger.Warning($"Reached time limit at {currentTimestamp}, stopping processing");
                    break;
                }

                if (processedCount % 10 == 0 || processedCount == 1)
                {
                    var percent = (int)((double)processedCount / framePaths.Count * 100);
                    Logger.Info($"Processing frame {processedCount}/{framePaths.Count} ({percent}%) at {currentTimestamp}");
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
                    consecutiveEmptyFrames++;

                    // Check if we should stop due to too many empty frames
                    if (options.StopAfterEmptyFrames > 0 && consecutiveEmptyFrames >= options.StopAfterEmptyFrames)
                    {
                        Logger.Warning($"No text found in {consecutiveEmptyFrames} consecutive frames at {currentTimestamp}, stopping processing");
                        break;
                    }

                    continue;
                }

                // Found text - reset consecutive empty counter
                consecutiveEmptyFrames = 0;

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

                // Track timestamps
                lastTextFoundTimestamp = currentTimestamp;
                if (string.IsNullOrEmpty(firstTextFoundTimestamp))
                {
                    firstTextFoundTimestamp = currentTimestamp;
                }

                // Build output
                var frameOutput = new StringBuilder();

                if (options.IncludeTimestamps)
                {
                    frameOutput.AppendLine($"[{currentTimestamp}]");
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

        if (textFoundCount > 0)
        {
            Logger.Info($"  - First text found at: {firstTextFoundTimestamp}");
            Logger.Info($"  - Last text found at: {lastTextFoundTimestamp}");

            var timeSpan = ParseTimestamp(lastTextFoundTimestamp) - ParseTimestamp(firstTextFoundTimestamp);
            Logger.Info($"  - Text found across: {FormatTimeSpan(timeSpan)}");
        }
        else
        {
            Logger.Warning("  - No text found in video");
        }

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
        try
        {
            // Convert to grayscale if not already
            Pix? processed = original;
            if (original.Depth != 8)
            {
                processed = original.ConvertRGBToGray();
                if (processed == null) return original;
            }

            // Apply binarization for better OCR
            var binarized = processed.BinarizeOtsuAdaptiveThreshold(2000, 2000, 0, 0, 0.1f);
            if (binarized != null)
            {
                return binarized;
            }

            return processed;
        }
        catch
        {
            // If preprocessing fails, return original
            return original;
        }
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

            // Use file-based comparison as fallback since Pix doesn't support pixel access
            // Compare file sizes first (very fast)
            var file1 = new FileInfo(imagePath1);
            var file2 = new FileInfo(imagePath2);

            // If file sizes differ significantly, images are different
            var sizeDiff = Math.Abs(file1.Length - file2.Length);
            var avgSize = (file1.Length + file2.Length) / 2.0;
            var sizeSimilarity = 1.0 - (sizeDiff / avgSize);

            // If files are very different sizes, skip hash comparison
            if (sizeSimilarity < 0.90)
            {
                return false;
            }

            // Compare file hashes for exact match detection
            var hash1 = ComputeFileHash(imagePath1);
            var hash2 = ComputeFileHash(imagePath2);

            // Exact match
            if (hash1 == hash2)
            {
                return true;
            }

            // For near-duplicates, use size similarity with threshold adjustment
            // Since we can't do pixel comparison, rely on file similarity
            return sizeSimilarity >= threshold;
        }
        catch (Exception ex)
        {
            Logger.Warning($"Error comparing images: {ex.Message}");
            return false; // If comparison fails, treat as different
        }
    }

    private string ComputeFileHash(string filePath)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        using var stream = File.OpenRead(filePath);
        var hash = md5.ComputeHash(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
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

    private TimeSpan ParseTimestamp(string timestamp)
    {
        var parts = timestamp.Split(':');
        if (parts.Length == 3 &&
            int.TryParse(parts[0], out var hours) &&
            int.TryParse(parts[1], out var minutes) &&
            int.TryParse(parts[2], out var seconds))
        {
            return new TimeSpan(hours, minutes, seconds);
        }
        return TimeSpan.Zero;
    }

    private string FormatTimeSpan(TimeSpan timeSpan)
    {
        if (timeSpan.TotalHours >= 1)
        {
            return $"{(int)timeSpan.TotalHours}h {timeSpan.Minutes}m {timeSpan.Seconds}s";
        }
        else if (timeSpan.TotalMinutes >= 1)
        {
            return $"{timeSpan.Minutes}m {timeSpan.Seconds}s";
        }
        else
        {
            return $"{timeSpan.Seconds}s";
        }
    }

    public void Dispose()
    {
        _engine?.Dispose();
    }
}
