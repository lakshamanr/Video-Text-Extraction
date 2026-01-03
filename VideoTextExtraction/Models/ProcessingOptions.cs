namespace VideoTextExtraction.Models;

/// <summary>
/// Configuration options for video text extraction
/// </summary>
public class ProcessingOptions
{
    /// <summary>
    /// Input video file path
    /// </summary>
    public string VideoPath { get; set; } = string.Empty;

    /// <summary>
    /// Output file path for extracted text
    /// </summary>
    public string OutputPath { get; set; } = string.Empty;

    /// <summary>
    /// Frames per second to extract for OCR (default: 1)
    /// </summary>
    public int FramesPerSecond { get; set; } = 1;

    /// <summary>
    /// Preferred subtitle language code (e.g., "eng", "spa")
    /// </summary>
    public string? PreferredLanguage { get; set; }

    /// <summary>
    /// Force OCR even if subtitles are available
    /// </summary>
    public bool ForceOcr { get; set; } = false;

    /// <summary>
    /// Include timestamps in output
    /// </summary>
    public bool IncludeTimestamps { get; set; } = false;

    /// <summary>
    /// Output format (txt or srt)
    /// </summary>
    public OutputFormat Format { get; set; } = OutputFormat.Text;

    /// <summary>
    /// Tesseract language data path
    /// </summary>
    public string TessDataPath { get; set; } = "./tessdata";

    /// <summary>
    /// OCR language (default: eng)
    /// </summary>
    public string OcrLanguage { get; set; } = "eng";

    /// <summary>
    /// Remove duplicate OCR text across consecutive frames
    /// </summary>
    public bool RemoveDuplicates { get; set; } = true;

    /// <summary>
    /// Use visual image comparison instead of text comparison for duplicate detection
    /// </summary>
    public bool UseImageComparison { get; set; } = true;

    /// <summary>
    /// Image similarity threshold (0.0 to 1.0, higher = more similar required)
    /// </summary>
    public double ImageSimilarityThreshold { get; set; } = 0.95;

    /// <summary>
    /// Save each unique frame's text to a separate file
    /// </summary>
    public bool SaveSeparateFiles { get; set; } = false;

    /// <summary>
    /// Maximum video duration to process (in seconds, 0 = no limit)
    /// </summary>
    public int MaxDurationSeconds { get; set; } = 0;

    /// <summary>
    /// Stop processing after finding no text for this many consecutive frames
    /// </summary>
    public int StopAfterEmptyFrames { get; set; } = 0;

    /// <summary>
    /// Maximum video length to process from the start (in minutes, 0 = process entire video)
    /// For example, if set to 30, only the first 30 minutes of the video will be processed
    /// </summary>
    public int MaxVideoLengthMinutes { get; set; } = 0;
}

public enum OutputFormat
{
    Text,
    Subtitle
}
