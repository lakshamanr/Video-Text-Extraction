# Quick Start Guide

Get up and running with Video Text Extraction in 5 minutes!

## Step 1: Install Prerequisites

### Install .NET 6.0+
```bash
# Check if already installed
dotnet --version

# If not installed, download from:
# https://dotnet.microsoft.com/download
```

### Install FFmpeg
```bash
# Windows (using Chocolatey)
choco install ffmpeg

# macOS
brew install ffmpeg

# Linux (Ubuntu/Debian)
sudo apt-get install ffmpeg

# Verify installation
ffmpeg -version
```

### Install Tesseract (for OCR)
```bash
# Windows (using Chocolatey)
choco install tesseract

# macOS
brew install tesseract

# Linux (Ubuntu/Debian)
sudo apt-get install tesseract-ocr

# Verify installation
tesseract --version
```

## Step 2: Download Tesseract Language Data

```bash
# Create tessdata directory
mkdir tessdata
cd tessdata

# Download English language data
curl -L -o eng.traineddata https://github.com/tesseract-ocr/tessdata/raw/main/eng.traineddata

# Optional: Download additional languages
# Spanish
curl -L -o spa.traineddata https://github.com/tesseract-ocr/tessdata/raw/main/spa.traineddata

# French
curl -L -o fra.traineddata https://github.com/tesseract-ocr/tessdata/raw/main/fra.traineddata

cd ..
```

## Step 3: Build the Project

```bash
# Navigate to project directory
cd VideoTextExtraction

# Restore NuGet packages
dotnet restore

# Build the project
dotnet build

# Verify it works
dotnet run -- --help
```

## Step 4: Test with a Sample Video

### Test 1: With Subtitles
```bash
# If your video has subtitles
dotnet run -- --video myvideo.mp4 --output result.txt

# View the result
cat result.txt
```

### Test 2: With OCR (for presentations/slides)
```bash
# For videos with on-screen text but no subtitles
dotnet run -- --video presentation.mp4 --output presentation_text.txt --force-ocr --fps 1

# View the result
cat presentation_text.txt
```

## Step 5: Common Use Cases

### Extract from YouTube Video (with subtitles)
```bash
# Download video with yt-dlp (preserving subtitles)
yt-dlp --write-auto-sub https://youtube.com/watch?v=VIDEO_ID

# Extract text
dotnet run -- --video "Video Title.mp4" --output transcript.txt
```

### Extract from Presentation Recording
```bash
# Use OCR mode with higher frame rate for better accuracy
dotnet run -- \
  --video presentation.mp4 \
  --output slides_text.txt \
  --force-ocr \
  --fps 2 \
  --timestamps
```

### Process Multiple Videos (Bash)
```bash
# Process all MP4 files in current directory
for video in *.mp4; do
  output="${video%.mp4}_transcript.txt"
  dotnet run -- --video "$video" --output "$output"
done
```

### Process Multiple Videos (PowerShell)
```powershell
# Process all MP4 files in current directory
Get-ChildItem *.mp4 | ForEach-Object {
  $output = $_.BaseName + "_transcript.txt"
  dotnet run -- --video $_.Name --output $output
}
```

## Troubleshooting

### Issue: "FFmpeg not found"
**Solution**: Make sure FFmpeg is in your system PATH
```bash
# Test FFmpeg
ffmpeg -version

# If not found, add to PATH or reinstall
```

### Issue: "Tesseract data not found"
**Solution**: Download tessdata to project directory
```bash
mkdir tessdata
cd tessdata
curl -L -o eng.traineddata https://github.com/tesseract-ocr/tessdata/raw/main/eng.traineddata
```

### Issue: Poor OCR accuracy
**Solution**: Try these improvements:
```bash
# 1. Increase frame rate
dotnet run -- --video input.mp4 --output output.txt --force-ocr --fps 2

# 2. Ensure correct language
dotnet run -- --video input.mp4 --output output.txt --force-ocr --ocr-lang eng

# 3. Check video quality (low resolution = poor OCR)
```

## Next Steps

- Read the full [README.md](README.md) for detailed documentation
- Customize processing options for your use case
- Integrate into your workflow or automation scripts

## Tips

1. **Subtitle extraction is much faster** than OCR - prefer videos with subtitles
2. **Higher FPS = better accuracy but slower** - balance based on your needs
3. **Check video quality** - low resolution videos produce poor OCR results
4. **Use appropriate language** - specify `--ocr-lang` for non-English videos
5. **Enable timestamps** with `--timestamps` for easier navigation

Happy extracting!
