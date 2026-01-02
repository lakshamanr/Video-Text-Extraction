# Usage Examples

This document provides practical examples for common use cases.

## Table of Contents
- [Basic Examples](#basic-examples)
- [Presentation Videos](#presentation-videos)
- [Educational Content](#educational-content)
- [Multi-Language Videos](#multi-language-videos)
- [Batch Processing](#batch-processing)
- [Integration Examples](#integration-examples)

---

## Basic Examples

### Example 1: Extract from Video with Subtitles

```bash
dotnet run -- \
  --video lecture.mp4 \
  --output lecture_transcript.txt
```

**Output:**
```
Welcome to this tutorial on machine learning.
Today we'll cover basic concepts.
Let's start with supervised learning.
...
```

### Example 2: Extract with Timestamps

```bash
dotnet run -- \
  --video interview.mp4 \
  --output interview_text.txt \
  --timestamps
```

**Output:**
```
[00:00:05] Welcome to today's interview.
[00:00:12] Our guest is an expert in AI.
[00:00:25] Let's discuss recent developments.
...
```

---

## Presentation Videos

### Example 3: PowerPoint Presentation (No Audio/Subtitles)

For videos showing slides or presentations:

```bash
dotnet run -- \
  --video presentation.mp4 \
  --output slides_content.txt \
  --force-ocr \
  --fps 2 \
  --timestamps
```

**Use Case:** Extract text from:
- PowerPoint presentations
- Google Slides recordings
- Keynote presentations
- PDF slideshows recorded as video

**Output:**
```
[00:00:00]
Introduction to Neural Networks
Deep Learning Fundamentals

[00:00:15]
What is a Neural Network?
- Artificial neurons
- Layers and connections
- Activation functions

[00:00:30]
Types of Neural Networks
1. Feedforward
2. Convolutional (CNN)
3. Recurrent (RNN)
...
```

### Example 4: Q&A Session with On-Screen Questions

```bash
dotnet run -- \
  --video qa_session.mp4 \
  --output qa_text.txt \
  --force-ocr \
  --fps 1 \
  --remove-duplicates
```

**Use Case:** Extract questions displayed on screen during:
- Live Q&A sessions
- Panel discussions
- Town halls
- AMAs (Ask Me Anything)

---

## Educational Content

### Example 5: Online Course with Subtitles

```bash
dotnet run -- \
  --video course_module1.mp4 \
  --output module1_notes.txt \
  --language eng \
  --timestamps
```

### Example 6: Tutorial with Code Examples on Screen

```bash
dotnet run -- \
  --video coding_tutorial.mp4 \
  --output code_snippets.txt \
  --force-ocr \
  --fps 3
```

**Note:** Higher FPS (3) captures more code changes on screen.

### Example 7: Webinar Recording

```bash
dotnet run -- \
  --video webinar_2024.mp4 \
  --output webinar_transcript.txt \
  --timestamps
```

---

## Multi-Language Videos

### Example 8: Spanish Video

```bash
# Download Spanish language data first
cd VideoTextExtraction/tessdata
curl -L -o spa.traineddata https://github.com/tesseract-ocr/tessdata/raw/main/spa.traineddata
cd ../..

# Process video
dotnet run -- \
  --video video_espanol.mp4 \
  --output transcript_spanish.txt \
  --language spa \
  --ocr-lang spa
```

### Example 9: Video with Multiple Subtitle Tracks

```bash
# Prefer English subtitles
dotnet run -- \
  --video multilang_video.mp4 \
  --output english_transcript.txt \
  --language eng
```

---

## Batch Processing

### Example 10: Process All MP4 Files (Bash)

```bash
#!/bin/bash

# Process all MP4 files in current directory
for video in *.mp4; do
  echo "Processing: $video"
  output="${video%.mp4}_transcript.txt"

  dotnet run -- \
    --video "$video" \
    --output "transcripts/$output" \
    --timestamps

  echo "Completed: $output"
  echo ""
done
```

### Example 11: Process All MP4 Files (PowerShell)

```powershell
# Create output directory
New-Item -ItemType Directory -Force -Path "transcripts"

# Process all MP4 files
Get-ChildItem *.mp4 | ForEach-Object {
  Write-Host "Processing: $($_.Name)"
  $output = "transcripts\$($_.BaseName)_transcript.txt"

  dotnet run -- `
    --video $_.Name `
    --output $output `
    --timestamps

  Write-Host "Completed: $output"
  Write-Host ""
}
```

### Example 12: Process with Different Methods Based on Content

```bash
#!/bin/bash

# Process videos with appropriate method
for video in *.mp4; do
  output="${video%.mp4}_text.txt"

  # Check if video name contains "presentation"
  if [[ "$video" == *"presentation"* ]]; then
    echo "OCR mode for: $video"
    dotnet run -- --video "$video" --output "$output" --force-ocr --fps 2
  else
    echo "Subtitle mode for: $video"
    dotnet run -- --video "$video" --output "$output"
  fi
done
```

---

## Integration Examples

### Example 13: YouTube Video Processing

```bash
# 1. Download video with subtitles using yt-dlp
yt-dlp --write-auto-sub --sub-lang en \
  https://www.youtube.com/watch?v=VIDEO_ID

# 2. Extract text
dotnet run -- \
  --video "Video Title [VIDEO_ID].mp4" \
  --output youtube_transcript.txt
```

### Example 14: Extract and Analyze

```bash
#!/bin/bash

VIDEO="lecture.mp4"
OUTPUT="lecture_text.txt"

# Extract text
dotnet run -- --video "$VIDEO" --output "$OUTPUT"

# Count words
echo "Word count: $(wc -w < "$OUTPUT")"

# Search for keywords
echo "Mentions of 'machine learning':"
grep -i "machine learning" "$OUTPUT" | wc -l

# Create summary (first 20 lines)
head -20 "$OUTPUT" > "${OUTPUT%.txt}_summary.txt"
```

### Example 15: Convert to Different Formats

```bash
#!/bin/bash

VIDEO="input.mp4"
BASE="output"

# Extract text
dotnet run -- --video "$VIDEO" --output "${BASE}.txt" --timestamps

# Convert to JSON (simple example)
python3 << EOF
import json

with open("${BASE}.txt", "r") as f:
    lines = f.readlines()

data = {
    "video": "$VIDEO",
    "lines": [line.strip() for line in lines if line.strip()]
}

with open("${BASE}.json", "w") as f:
    json.dump(data, f, indent=2)
EOF

echo "Created ${BASE}.json"
```

### Example 16: Process and Upload to Cloud

```bash
#!/bin/bash

VIDEO="meeting.mp4"
OUTPUT="meeting_transcript.txt"

# Extract text
dotnet run -- --video "$VIDEO" --output "$OUTPUT" --timestamps

# Upload to S3 (example)
aws s3 cp "$OUTPUT" s3://my-bucket/transcripts/

# Or upload to Google Drive (using gdrive CLI)
gdrive upload "$OUTPUT"

echo "Transcript uploaded successfully"
```

---

## Advanced Use Cases

### Example 17: Extract Code from Coding Tutorial

```bash
# High FPS to capture code changes
dotnet run -- \
  --video python_tutorial.mp4 \
  --output code_extracted.txt \
  --force-ocr \
  --fps 5 \
  --remove-duplicates
```

### Example 18: Extract Subtitles in SRT Format

While the tool outputs plain text, you can preserve SRT format:

```bash
# Keep subtitle format (basic example)
dotnet run -- \
  --video movie.mp4 \
  --output subtitles.txt \
  --timestamps

# Note: For proper SRT format, you might want to extract the subtitle stream directly
# and not parse it. Future feature could support this.
```

### Example 19: Process Low Quality Video

```bash
# Use lower FPS for faster processing
dotnet run -- \
  --video low_quality.mp4 \
  --output text.txt \
  --force-ocr \
  --fps 0.5  # One frame every 2 seconds
```

### Example 20: Debugging OCR Issues

```bash
# Test with small sample and inspect frame extraction
dotnet run -- \
  --video test.mp4 \
  --output test_output.txt \
  --force-ocr \
  --fps 1

# Check tessdata path
ls -la VideoTextExtraction/tessdata/

# Verify Tesseract works
tesseract --list-langs
```

---

## Performance Comparison

### Same Video, Different Methods

```bash
# Method 1: Subtitle extraction (FAST)
time dotnet run -- --video lecture.mp4 --output method1.txt
# Real time: ~5 seconds

# Method 2: OCR at 1 FPS (MEDIUM)
time dotnet run -- --video lecture.mp4 --output method2.txt --force-ocr --fps 1
# Real time: ~3-5 minutes for 1 hour video

# Method 3: OCR at 2 FPS (SLOW but more accurate)
time dotnet run -- --video lecture.mp4 --output method3.txt --force-ocr --fps 2
# Real time: ~6-10 minutes for 1 hour video
```

---

## Tips for Best Results

### For Presentations
- Use `--fps 2` or higher
- Enable `--timestamps` to match slides with time
- Use `--remove-duplicates` to avoid repeated text

### For Subtitled Videos
- Always try subtitle extraction first (much faster)
- Specify `--language` if video has multiple subtitle tracks

### For OCR
- Higher FPS = more accurate but slower
- Ensure good video quality (720p or higher recommended)
- Use appropriate language data (`--ocr-lang`)

### For Batch Processing
- Create organized output directory structure
- Use meaningful filenames
- Log processing status for debugging

---

## Need Help?

- Check [README.md](README.md) for full documentation
- See [QUICKSTART.md](QUICKSTART.md) for setup guide
- Report issues at GitHub repository

Happy extracting!
