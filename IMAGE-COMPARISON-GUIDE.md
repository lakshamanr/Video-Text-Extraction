# Image Comparison & Separate Files - User Guide

## New Features

Two powerful new features have been added to improve OCR processing:

### 1. ✅ **Visual Image Comparison** (Enabled by default)
Compares actual images instead of just text to detect duplicates.

### 2. 📁 **Separate File Output** (Optional)
Saves each unique frame's text to its own file.

---

## Feature 1: Visual Image Comparison

### What It Does
- Compares **images pixel-by-pixel** BEFORE running OCR
- Skips OCR entirely for duplicate frames
- Much more efficient than text-based comparison

### How It Works
```
Frame 1 → Load image → Compare with previous → Different → Run OCR → Save
Frame 2 → Load image → Compare with previous → SAME → Skip OCR ✓
Frame 3 → Load image → Compare with previous → Different → Run OCR → Save
```

### Benefits
- **Faster**: Skips OCR for duplicate frames (huge time savings)
- **More Accurate**: Detects visual duplicates even if OCR varies slightly
- **Efficient**: Only processes unique frames

### Perfect For
- **Presentation videos** where slides stay static for multiple frames
- **Static content** with same image repeating
- **Long videos** with lots of duplicate frames

### GUI Usage
```
☑ Remove duplicate frames
☑ Compare images visually (more accurate)  ← This enables image comparison
```

### Technical Details
- **Algorithm**: Pixel-by-pixel grayscale comparison
- **Sampling**: Every 10th pixel (for performance)
- **Tolerance**: 15 grayscale levels (out of 256)
- **Default Threshold**: 95% similarity
- **Speed**: ~100x faster than OCR

### Example Results

**Before (Text Comparison):**
```
Frames processed: 120
OCR operations: 120
Processing time: 6 minutes
```

**After (Image Comparison):**
```
Frames processed: 120
Unique frames: 25
OCR operations: 25 (80% saved!)
Duplicates skipped: 95
Processing time: 1.5 minutes
```

### When to Use
- ✅ **Enable** for presentations and static content (DEFAULT)
- ✅ **Enable** when frames repeat frequently
- ⚠️ **Disable** if video has rapidly changing content with similar text

---

## Feature 2: Separate File Output

### What It Does
- Saves each **unique frame's text** to its own file
- Creates a subfolder with numbered files

### Output Structure
```
Az204 - Exam Questions_extracted.txt           ← Main combined file
Az204 - Exam Questions_extracted_frames/       ← New folder created
  ├── frame_0001.txt  ← First unique slide
  ├── frame_0002.txt  ← Second unique slide
  ├── frame_0003.txt  ← Third unique slide
  ├── frame_0004.txt  ← Fourth unique slide
  └── ...
```

### File Contents
Each file contains:
```
[00:00:15]                           ← Timestamp (if enabled)
Question 12: What is Azure App Service?

A) A cloud storage solution
B) A platform for building web apps
C) A database service
D) A virtual machine service
```

### GUI Usage
```
☐ Save each frame to separate file  ← Check this box
```

### Benefits
- **Easy Review**: Open and review each slide individually
- **Quality Control**: Verify extraction accuracy per frame
- **Selective Use**: Process only specific frames you need
- **Organization**: Each slide in its own file

### Perfect For
- **Q&A Sessions**: Each question in separate file
- **Presentations**: Each slide isolated
- **Studying**: Review slides one at a time
- **Data Processing**: Process frames independently

### Example Use Cases

#### Use Case 1: Exam Preparation
```
Video: "Az204 Practice Questions.mp4"
Settings:
  ☑ Force OCR
  ☑ Compare images visually
  ☑ Save each frame to separate file
  ☑ Include timestamps

Result:
  practice_questions_frames/
    frame_0001.txt  ← Question 1
    frame_0002.txt  ← Question 2
    frame_0003.txt  ← Question 3
    ...
```

#### Use Case 2: Presentation Review
```
Video: "Product Launch Presentation.mp4"
Settings:
  ☑ Force OCR
  ☑ Compare images visually
  ☑ Save each frame to separate file
  ☑ Include timestamps
  FPS: 2

Result:
  presentation_frames/
    frame_0001.txt  ← Title slide
    frame_0002.txt  ← Agenda
    frame_0003.txt  ← Product overview
    ...
```

---

## Recommended Settings

### For Presentations (Slides with Text)
```
✅ Force OCR: Checked
✅ Compare images visually: Checked (DEFAULT)
✅ Save each frame to separate file: Checked (if you want individual files)
✅ Remove duplicate frames: Checked
✅ Include timestamps: Checked
📊 Frames/Second: 2
```

### For Q&A Videos (Questions on Screen)
```
✅ Force OCR: Checked
✅ Compare images visually: Checked (DEFAULT)
✅ Save each frame to separate file: Checked (each question separate)
✅ Remove duplicate frames: Checked
✅ Include timestamps: Checked
📊 Frames/Second: 1
```

### For Fast-Changing Videos
```
✅ Force OCR: Checked
⚠️ Compare images visually: Maybe uncheck (if content changes rapidly)
❌ Save each frame to separate file: Unchecked (too many files)
⚠️ Remove duplicate frames: Maybe uncheck
✅ Include timestamps: Checked
📊 Frames/Second: 3-5
```

---

## Understanding the Log Output

With new features enabled, you'll see:

```
[INFO] Processing 120 frames with OCR...
[INFO] Duplicate removal: Enabled
[INFO] Comparison method: Visual (Image)          ← Image comparison active
[INFO] Output mode: Separate files                ← Saving separate files
[INFO] Saving separate files to: C:\...\output_frames
[INFO] Processing frame 1/120 (0%)...
[INFO] Processing frame 10/120 (8%)...
...
[SUCCESS] OCR complete:
[INFO]   - Frames processed: 120
[INFO]   - Unique frames found: 25                 ← Only 25 unique slides
[INFO]   - Low confidence skipped: 10
[INFO]   - Duplicates skipped: 85                  ← 85 frames were duplicates!
[SUCCESS] Saved 25 separate files to: C:\...\output_frames
```

### What This Means:
- **120 frames** extracted from video
- **Visual comparison** detected **85 duplicates** (skipped OCR)
- **Only 25 frames** were unique and processed with OCR
- **25 separate files** created (one per unique frame)
- **~80% time saved** by skipping duplicate OCR

---

## Performance Comparison

### Scenario: 1-hour presentation video (3600 frames at 1 FPS)

| Feature Combination | Unique Frames | OCR Operations | Time | Files Created |
|---------------------|---------------|----------------|------|---------------|
| Text comparison only | ~50 | 3600 | ~18 min | 1 combined |
| Image comparison | ~50 | 50 | ~3 min | 1 combined |
| Image + Separate files | ~50 | 50 | ~3 min | 50 separate |

**Result**: Image comparison saves ~15 minutes by avoiding 3550 unnecessary OCR operations!

---

## Troubleshooting

### Issue: "Too many duplicate frames skipped"

**Possible Cause**: Threshold too high, treating different frames as same

**Solution**:
- This is usually GOOD (means frames are actually duplicates)
- If concerned, check output files to verify
- Threshold is set to 95% (very strict)

### Issue: "Not enough duplicates detected"

**Possible Cause**: Video content changes frequently

**Solution**:
- This is expected for videos with rapidly changing content
- Image comparison working correctly
- Consider if you really need duplicate removal

### Issue: "Too many separate files created"

**Possible Cause**: Video has many unique frames

**Solutions**:
1. **Decrease FPS** to 0.5 or 1 (fewer frames extracted)
2. **Uncheck "Save separate files"** for combined output
3. **Review actual content** - each file represents unique frame

### Issue: "Separate files folder not created"

**Possible Cause**: "Save separate files" not checked

**Solution**:
- ✅ Check "Save each frame to separate file" in GUI
- Folder will be created automatically when processing starts

---

## CLI Usage (for advanced users)

```bash
# Image comparison + combined file (DEFAULT)
dotnet run -- --video input.mp4 --output output.txt --force-ocr

# Image comparison + separate files
dotnet run -- --video input.mp4 --output output.txt --force-ocr --save-separate-files

# Text comparison (legacy mode)
dotnet run -- --video input.mp4 --output output.txt --force-ocr --no-image-comparison
```

**Note**: CLI support will be added in future update. Currently use GUI for these features.

---

## Best Practices

### ✅ DO:
- Enable image comparison for presentations (default)
- Use separate files for Q&A or study materials
- Include timestamps for easy navigation
- Set FPS to 1-2 for static content

### ❌ DON'T:
- Disable image comparison unless you have a specific reason
- Use separate files for long videos with many unique frames (creates too many files)
- Use very high FPS with separate files (creates excessive files)

---

## Summary

### Image Comparison
- **What**: Compares actual images instead of text
- **When**: Always (enabled by default)
- **Benefit**: 50-90% faster processing for static content

### Separate Files
- **What**: Saves each unique frame to its own file
- **When**: When you want individual slide/frame files
- **Benefit**: Easy to review and process frames independently

### Perfect Combination
```
☑ Force OCR
☑ Compare images visually (DEFAULT)
☑ Save each frame to separate file (OPTIONAL)
☑ Remove duplicate frames
☑ Include timestamps
```

This gives you:
- **Fast processing** (image comparison)
- **Organized output** (separate files)
- **No duplicates** (duplicate removal)
- **Easy navigation** (timestamps)

Try it now with your presentation or Q&A video! 🎉
