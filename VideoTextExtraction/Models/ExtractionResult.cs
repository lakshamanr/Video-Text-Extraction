namespace VideoTextExtraction.Models;

/// <summary>
/// Result of text extraction from video
/// </summary>
public class ExtractionResult
{
    /// <summary>
    /// Whether extraction was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Extraction method used
    /// </summary>
    public ExtractionMethod Method { get; set; }

    /// <summary>
    /// Extracted text content
    /// </summary>
    public string ExtractedText { get; set; } = string.Empty;

    /// <summary>
    /// Output file path
    /// </summary>
    public string OutputPath { get; set; } = string.Empty;

    /// <summary>
    /// Error message if extraction failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Processing time in seconds
    /// </summary>
    public double ProcessingTimeSeconds { get; set; }

    /// <summary>
    /// Number of text segments extracted
    /// </summary>
    public int SegmentCount { get; set; }
}

public enum ExtractionMethod
{
    EmbeddedSubtitles,
    ExternalSubtitles,
    Ocr,
    None
}
