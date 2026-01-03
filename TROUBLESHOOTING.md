# Troubleshooting Guide

## Issue: No Text Being Extracted

If the application processes frames but extracts no text, follow these diagnostic steps:

### Step 1: Check the Processing Logs

Look for these key messages in the output:

```
Processing mode: SEQUENTIAL (or PARALLEL)
Processing N frames with OCR...
OCR confidence threshold: 0.60
```

### Step 2: Look for Low Confidence Warnings

If you see messages like:
```
Frame frame_0001.png: Low confidence 0.45 (threshold 0.60), text discarded
```

**This means** the OCR is finding text but rejecting it due to low confidence.

**Solution**: Lower the confidence threshold

#### Option 1: Programmatically (Recommended)
```csharp
var options = new ProcessingOptions
{
    VideoPath = "input.mp4",
    OutputPath = "output.txt",
    OcrConfidenceThreshold = 0.3f  // Lower threshold (0.3 instead of 0.6)
};
```

#### Option 2: Temporarily modify ProcessingOptions.cs
Change line 103:
```csharp
// From:
public float OcrConfidenceThreshold { get; set; } = 0.6f;

// To:
public float OcrConfidenceThreshold { get; set; } = 0.3f;
```

**Note**: Lower thresholds (0.2-0.4) extract more text but may include errors. Higher thresholds (0.6-0.8) are more accurate but may miss text.

### Step 3: Check Frame Extraction

Look for:
```
Extracted N frames to [directory]
```

If N is 0:
- Check video path is correct
- Ensure FFmpeg is installed and in PATH
- Try increasing FPS (e.g., from 1 to 2)

### Step 4: Check Tesseract Installation

If you see:
```
Failed to initialize OCR engine
Tesseract data directory not found
```

**Solution**:
1. Download tessdata: https://github.com/tesseract-ocr/tessdata
2. Extract to `./tessdata` or update TessDataPath in options
3. Ensure `eng.traineddata` exists in the tessdata folder

### Step 5: Verify Video Has Text

Some videos might not have readable text:
- Handwritten text (OCR works better with typed text)
- Very small text (increase video resolution or FPS)
- Text in different language (change OcrLanguage option)
- No text at all in the frames being processed

### Step 6: Test with Known Good Video

Create a simple test:
1. Create a PowerPoint slide with large, clear text
2. Export as video (MP4)
3. Try extracting text from this video
4. If this works, the issue is with your original video

### Common Issues and Solutions

#### Issue: "Processing mode: PARALLEL" but expecting sequential
**Solution**: Uncheck "Enable Parallel Processing" in GUI or remove `--parallel` flag from CLI

#### Issue: Compilation errors about Interlocked64Counter
**Cause**: Older version had namespace bug
**Solution**: Pull latest code (commit 6e1a786 or later)

#### Issue: Text extracted previously, but now nothing
**Possible causes**:
1. Parallel processing enabled accidentally
2. Confidence threshold too high
3. Different video format

**Solution**:
- Check log for "OCR confidence threshold" value
- Try with `OcrConfidenceThreshold = 0.3f`
- Disable parallel processing temporarily

#### Issue: Getting errors in logs
**Check for**:
```
Errors encountered: N
```

Look above this message for specific error details. Common errors:
- "Thread-local engine not initialized" → Tesseract installation issue
- "Error extracting text from frame" → Corrupted frame or unsupported format
- "Failed to create thread-local OCR engine" → Missing tessdata files

### Detailed Logging Output

A successful run should show:

```
Initializing Tesseract OCR engine (language: eng)...
OCR engine initialized
Extracting frames at 1 FPS...
Video duration: 00:05:30
Extracted 330 frames to [path]
Processing mode: SEQUENTIAL
Processing 330 frames with OCR...
Duplicate removal: Enabled
Comparison method: Visual (Image)
Output mode: Combined file
OCR confidence threshold: 0.60
Processing frame 1/330 (0%) at 00:00:00
Processing frame 10/330 (3%) at 00:00:09
[...]
OCR complete:
  - Frames processed: 330
  - Unique frames found: 45
  - Low confidence skipped: 285
  - Duplicates skipped: 0
  - First text found at: 00:00:05
  - Last text found at: 00:05:25
  - Text found across: 00:05:20
Extraction complete. Text length: 2547 characters
```

### Key Metrics to Check

1. **Frames processed**: Should equal number of extracted frames
2. **Unique frames found**: Should be > 0 if video has text
3. **Low confidence skipped**: If this equals "Frames processed", lower the threshold
4. **Text length**: Final output length in characters

### Getting Help

If none of these solutions work:

1. **Capture full log output**: Save console output to file
2. **Check versions**:
   - .NET version: `dotnet --version`
   - FFmpeg version: `ffmpeg -version`
   - Tesseract version: Check installation
3. **Test video details**:
   - Format: MP4, AVI, MKV, etc.
   - Resolution: 1920x1080, etc.
   - Duration: How long
   - Text type: Slides, subtitles, overlays

4. **Create minimal test case**:
   ```bash
   # Test with simple video
   VideoTextExtraction.exe \
     --video test.mp4 \
     --output test.txt \
     --fps 1 \
     --force-ocr
   ```

5. **Open GitHub issue** with:
   - Full log output
   - Video details (without sharing the actual video if confidential)
   - Environment details (OS, .NET version, etc.)

## Performance Issues

### Issue: Very Slow Processing

**Solutions**:
1. Enable parallel processing (3-8x faster)
2. Reduce FPS (e.g., 1 instead of 2)
3. Limit video length (first N minutes)
4. Disable image comparison (use text-based duplicate detection)

### Issue: High Memory Usage

**Solutions**:
1. Reduce parallel thread count
2. Process video in segments
3. Disable separate file output
4. Reduce FPS

## Best Practices

1. **Start with defaults**: Use default confidence (0.6) first
2. **Check logs**: Always review logs to understand what's happening
3. **Test incrementally**: Start with short video segments
4. **Adjust based on content**: Lower threshold for low-quality videos, higher for high-quality
5. **Use parallel processing**: Enable for videos > 1 minute for significant speedup

## Quick Reference: Confidence Thresholds

| Threshold | Use Case | Quality | Speed |
|-----------|----------|---------|-------|
| 0.2-0.3   | Low quality videos, handwritten text | Lower accuracy, more text | Same |
| 0.4-0.5   | Medium quality videos | Good balance | Same |
| 0.6 (default) | High quality slides, clear text | High accuracy | Same |
| 0.7-0.8   | Very high quality, typed text only | Highest accuracy, may miss some text | Same |

## Debug Mode

To get maximum diagnostic information, check these log messages:

```
Processing mode: SEQUENTIAL
Initializing Tesseract OCR engine (language: eng)...
OCR engine initialized
Video duration: XX:XX:XX
Processing N frames with OCR...
OCR confidence threshold: 0.60
Processing frame X/Y (Z%) at HH:MM:SS
Frame frame_XXXX.png: Low confidence 0.XX (threshold 0.60), text discarded
Post-processing N frame results...
OCR complete:
  - Frames processed: X
  - Frames with text extracted: Y
  - Unique frames found: Z
Extraction complete. Text length: XXXX characters
```

If any of these messages are missing or show unexpected values, that's where the issue lies.
