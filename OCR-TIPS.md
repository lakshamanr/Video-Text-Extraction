# OCR Tips and Best Practices

This guide helps you get the best text extraction results from videos.

## Understanding the Improvements

The OCR engine now includes:

### 1. **Image Preprocessing**
- Converts frames to grayscale
- Enhances contrast with unsharp masking
- Applies adaptive thresholding (binarization)
- Results in clearer, more readable text for OCR

### 2. **Better Duplicate Detection**
- Uses Levenshtein distance algorithm (edit distance)
- 90% similarity threshold (stricter than before)
- Normalizes text before comparison
- Much more accurate than previous method

### 3. **Higher Quality Standards**
- Confidence threshold raised from 30% to 60%
- Character whitelist filters out junk text
- Better text trimming and cleaning
- More accurate, fewer false positives

## Recommended Settings for Different Video Types

### Presentation Videos (PowerPoint, Slides)

**Best Settings:**
- **Frames/Second**: 2-3
- **Force OCR**: ✅ Checked
- **Include Timestamps**: ✅ Checked (helpful for navigating)
- **Remove Duplicates**: ✅ Checked

**Why:**
- Higher FPS captures slide transitions better
- Timestamps help you know when slides change
- Duplicate removal prevents repeated text from same slide

**Example:**
```
Video: "Product Launch Presentation.mp4"
FPS: 2
Output: Clean text from each slide with timestamps
```

### Q&A Sessions / Interviews

**Best Settings:**
- **Frames/Second**: 1
- **Force OCR**: ✅ Checked
- **Include Timestamps**: ✅ Checked
- **Remove Duplicates**: ✅ Checked

**Why:**
- Questions often stay on screen for multiple frames
- Lower FPS is sufficient for static text
- Duplicates removal keeps only unique questions

### Educational Videos / Tutorials

**Best Settings:**
- **Frames/Second**: 1-2
- **Force OCR**: Only if no subtitles
- **Include Timestamps**: ✅ Checked
- **Remove Duplicates**: ✅ Checked

**Why:**
- Try subtitle extraction first (much faster)
- Use OCR for on-screen code examples or diagrams

### Fast-Moving Content

**Best Settings:**
- **Frames/Second**: 3-5
- **Force OCR**: ✅ Checked
- **Include Timestamps**: ✅ Checked
- **Remove Duplicates**: ⚠️ Consider unchecking

**Why:**
- Higher FPS captures rapid text changes
- May want to keep near-duplicates if text changes quickly

## Video Quality Requirements

### Optimal Video Quality
- **Resolution**: 720p (1280x720) or higher
- **Text Size**: Minimum 20px font size
- **Contrast**: High contrast between text and background
- **Clarity**: Sharp, not blurry

### Minimum Requirements
- **Resolution**: 480p (640x480)
- **Text Size**: Readable when paused
- **Contrast**: Text clearly visible
- **Clarity**: Minimal compression artifacts

### Poor Quality (Will Have Issues)
- ❌ Resolution below 480p
- ❌ Heavily compressed video
- ❌ Blurry or out-of-focus text
- ❌ Low contrast (gray text on gray background)
- ❌ Very small text (<12px)

## Improving OCR Accuracy

### 1. Increase Frame Rate
If you're missing text:
- Try **2 FPS** instead of 1
- Try **3 FPS** for fast-changing slides
- Higher FPS = more chances to capture text

### 2. Check Video Quality
- Play the video and pause on text
- Can you easily read it? If not, OCR will struggle
- Consider re-recording at higher quality

### 3. Verify Tesseract Language
- Make sure correct language is selected
- Download appropriate language data
- Example: For Spanish video, use `spa` language

### 4. Review Duplicate Settings
- If getting too much repeated text: **Enable** Remove Duplicates
- If missing some text: **Disable** Remove Duplicates temporarily
- The new algorithm is much better at this!

### 5. Use Timestamps
- Always enable timestamps for presentations
- Helps correlate extracted text with video timing
- Easier to verify accuracy

## Understanding the Output Log

After processing, you'll see detailed statistics:

```
[INFO] Processing 120 frames with OCR...
[INFO] Duplicate removal: Enabled
[INFO] Processing frame 1/120 (0%)...
[INFO] Processing frame 10/120 (8%)...
...
[SUCCESS] OCR complete:
[INFO]   - Frames processed: 120
[INFO]   - Text segments found: 25
[INFO]   - Low confidence skipped: 80
[INFO]   - Duplicates skipped: 15
```

**What This Means:**
- **Frames processed**: Total frames analyzed
- **Text segments found**: Unique text blocks extracted
- **Low confidence skipped**: Frames with poor OCR confidence (<60%)
- **Duplicates skipped**: Frames with duplicate text removed

**Interpreting Results:**

**Good Results:**
```
Frames processed: 120
Text segments: 25
Low confidence: 80
Duplicates: 15
```
✅ Found 25 unique text segments
✅ Properly skipped low-quality frames
✅ Removed duplicates effectively

**Poor Results:**
```
Frames processed: 120
Text segments: 2
Low confidence: 118
Duplicates: 0
```
❌ Only found 2 segments (should be more)
❌ Almost all frames low confidence
**Fix:** Check video quality or increase FPS

**Too Many Duplicates:**
```
Frames processed: 120
Text segments: 10
Low confidence: 10
Duplicates: 100
```
⚠️ High duplicate rate (might be OK for static content)
**Consider:** Decreasing FPS for efficiency

## Common Issues and Solutions

### Issue: "No text extracted"

**Possible Causes:**
1. Video has no visible text (only audio)
2. Text is too small or blurry
3. Wrong FPS setting

**Solutions:**
- ✅ Verify video has on-screen text
- ✅ Increase FPS to 2 or 3
- ✅ Check if subtitles exist instead (faster!)
- ✅ Ensure video quality is adequate

### Issue: "Lots of duplicate text"

**Status:** Should be **fixed** with new duplicate detection!

**If still occurring:**
- ✅ Verify "Remove Duplicates" is checked
- ✅ Update to latest version (commit a7e7b7a or later)
- ✅ Try decreasing FPS to 1

### Issue: "Gibberish or incorrect text"

**Possible Causes:**
1. Low video quality
2. Poor contrast
3. Unusual fonts
4. Wrong language setting

**Solutions:**
- ✅ Check video quality (play and pause to verify readability)
- ✅ Verify correct OCR language is selected
- ✅ Try different FPS settings
- ✅ Ensure tessdata for your language is installed

### Issue: "Missing some text"

**Possible Causes:**
1. Text appears very briefly
2. FPS too low
3. Text too small

**Solutions:**
- ✅ Increase FPS to 2-3
- ✅ Temporarily disable "Remove Duplicates"
- ✅ Check if missed text is very small or low contrast

## Performance Tips

### Fast Processing (Lower Quality OK)
```
FPS: 0.5 (one frame every 2 seconds)
Remove Duplicates: Enabled
```
- Processes faster
- Good for static content
- May miss some text

### Balanced (Recommended)
```
FPS: 1
Remove Duplicates: Enabled
```
- Good accuracy
- Reasonable processing time
- Works for most videos

### Maximum Accuracy (Slower)
```
FPS: 3
Remove Duplicates: Enabled
```
- Best accuracy
- Captures fast-changing text
- Takes longer to process

## Processing Time Estimates

Based on 1-hour video:

| FPS | Processing Time | Best For |
|-----|----------------|----------|
| 0.5 | 2-3 minutes | Static presentations |
| 1 | 3-6 minutes | Most videos |
| 2 | 6-12 minutes | Fast-changing slides |
| 3 | 9-18 minutes | Maximum accuracy |

*Times vary based on CPU speed and video complexity*

## What Changed in the Latest Update

### Before (Old Algorithm)
- ❌ Simple word-based similarity (Jaccard index)
- ❌ 85% threshold (too loose)
- ❌ No image preprocessing
- ❌ 30% confidence threshold (too low)
- ❌ Result: Many duplicates, inaccurate text

### After (New Algorithm - Commit a7e7b7a)
- ✅ Levenshtein distance (edit distance)
- ✅ 90% threshold (stricter)
- ✅ Image preprocessing (grayscale, contrast, binarization)
- ✅ 60% confidence threshold (higher quality)
- ✅ Result: Accurate text, proper duplicate removal

## Example Workflows

### Workflow 1: Extract from Business Presentation
```
1. Open GUI
2. Select video: "Q4_Results.mp4"
3. Set FPS: 2
4. Check: Force OCR, Include Timestamps, Remove Duplicates
5. Update Tessdata path if needed
6. Click "Extract Text"
7. Review output file
```

### Workflow 2: Extract from Educational Video
```
1. Open GUI
2. Select video: "Python_Tutorial.mp4"
3. First try without Force OCR (check for subtitles)
4. If no subtitles:
   - Check Force OCR
   - Set FPS: 2
   - Check Remove Duplicates
5. Extract and review
```

### Workflow 3: Fast Processing for Static Content
```
1. Select video with mostly static slides
2. Set FPS: 1 (or even 0.5)
3. Enable Remove Duplicates
4. Process
5. Should be fast with good results
```

## Verifying Results

After extraction:

1. **Check the log output**
   - Look at statistics
   - Verify reasonable number of segments found

2. **Open the output file**
   - Read through extracted text
   - Check if it makes sense

3. **Compare with video**
   - Spot-check a few timestamps
   - Verify text matches what's on screen

4. **Adjust if needed**
   - If missing text: Increase FPS
   - If too many duplicates: Should be fixed now!
   - If gibberish: Check video quality

## Getting Help

If you're still having issues:

1. Check video quality first (play and verify text is readable)
2. Verify FFmpeg and Tesseract are installed correctly
3. Ensure correct tessdata language files are installed
4. Try different FPS settings
5. Check the [SETUP-WINDOWS.md](SETUP-WINDOWS.md) guide

## Summary

**Key Improvements:**
- ✅ Better image preprocessing
- ✅ Smarter duplicate detection
- ✅ Higher quality standards
- ✅ More detailed logging

**Best Practices:**
- Start with FPS = 1 or 2
- Always enable "Remove Duplicates"
- Use timestamps for presentations
- Verify video quality first

**Expected Results:**
- Much more accurate text extraction
- Proper duplicate removal
- Higher quality output
- Better logging and statistics

The new OCR engine should provide significantly better results! 🎉
