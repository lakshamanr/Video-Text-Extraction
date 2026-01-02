# Video Text Extraction Tool

A powerful C# application that extracts text from videos using **subtitles** (primary method) or **OCR** (fallback method). Perfect for extracting text from presentations, Q&A sessions, educational videos, or any video with on-screen text.

## Features

- **Dual Extraction Methods**:
  - **Subtitle Extraction** (Primary): Automatically detects and extracts embedded or external subtitles
  - **OCR Extraction** (Fallback): Extracts text from video frames when subtitles are unavailable

- **Format Support**:
  - Video formats: `.mp4`, `.mkv`, `.avi`, and more
  - Subtitle formats: `.srt`, `.vtt`, `.ass`, `.ssa`

- **Advanced Capabilities**:
  - Embedded subtitle detection
  - External subtitle file detection
  - Multi-language support
  - Duplicate text removal (OCR mode)
  - Timestamp preservation
  - Configurable frame extraction rate

- **Use Cases**:
  - Extract text from presentation videos (PowerPoint, slides)
  - Process Q&A sessions with on-screen text
  - Convert lecture videos to text
  - Extract burned-in subtitles

## Architecture

```
Video File
   │
   ├── Subtitle Detector
   │      ├── Embedded Subtitles (.mp4, .mkv)
   │      └── External Subtitles (.srt, .vtt, .ass)
   │
   ├── Subtitle Extractor ──► Text Output
   │
   └── OCR Pipeline (fallback)
          ├── Frame Extractor (FFmpeg)
          ├── OCR Engine (Tesseract)
          └── Text Aggregator
```

## Prerequisites

### 1. .NET SDK

Install .NET 6.0 or later:
- Download from: https://dotnet.microsoft.com/download

### 2. FFmpeg

FFmpeg is required for video processing:

**Windows:**
```bash
# Using Chocolatey
choco install ffmpeg

# Or download from: https://ffmpeg.org/download.html
```

**macOS:**
```bash
brew install ffmpeg
```

**Linux (Ubuntu/Debian):**
```bash
sudo apt-get update
sudo apt-get install ffmpeg
```

### 3. Tesseract OCR (for OCR functionality)

**Windows:**
```bash
# Using Chocolatey
choco install tesseract

# Or download from: https://github.com/UB-Mannheim/tesseract/wiki
```

**macOS:**
```bash
brew install tesseract
```

**Linux (Ubuntu/Debian):**
```bash
sudo apt-get install tesseract-ocr
```

### 4. Tesseract Language Data

Download language data files from:
https://github.com/tesseract-ocr/tessdata

Extract to `./tessdata` directory in your project or specify custom path with `--tessdata`.

For English:
```bash
mkdir tessdata
cd tessdata
wget https://github.com/tesseract-ocr/tessdata/raw/main/eng.traineddata
```

## Installation

### Option 1: Build from Source

```bash
# Clone the repository
git clone <repository-url>
cd Video-Text-Extraction/VideoTextExtraction

# Restore packages
dotnet restore

# Build the project
dotnet build

# Run the application
dotnet run -- --help
```

### Option 2: Publish as Executable

```bash
# Publish for your platform
dotnet publish -c Release -r win-x64 --self-contained
# Or for Linux: -r linux-x64
# Or for macOS: -r osx-x64

# Run the executable
./bin/Release/net6.0/win-x64/publish/VideoTextExtraction.exe
```

## Usage

### Basic Usage

Extract text using subtitles (if available):
```bash
dotnet run -- --video input.mp4 --output output.txt
```

### Force OCR Mode

Force OCR extraction even if subtitles exist:
```bash
dotnet run -- --video presentation.mp4 --output output.txt --force-ocr
```

### With Timestamps

Include timestamps in output:
```bash
dotnet run -- --video video.mp4 --output output.txt --timestamps
```

### Specify Language

Prefer specific subtitle language:
```bash
dotnet run -- --video video.mp4 --output output.txt --language eng
```

### Adjust Frame Rate (OCR)

Extract more/fewer frames per second:
```bash
# 2 frames per second (more accurate but slower)
dotnet run -- --video video.mp4 --output output.txt --force-ocr --fps 2

# 0.5 frames per second (faster but may miss text)
dotnet run -- --video video.mp4 --output output.txt --force-ocr --fps 0.5
```

### OCR with Different Language

```bash
dotnet run -- --video video_spanish.mp4 --output output.txt --force-ocr --ocr-lang spa
```

## Command-Line Options

| Option | Alias | Description | Default |
|--------|-------|-------------|---------|
| `--video` | `-v` | Path to input video file | (required) |
| `--output` | `-o` | Path to output text file | (required) |
| `--fps` | `-f` | Frames per second for OCR | 1 |
| `--force-ocr` | - | Force OCR even if subtitles exist | false |
| `--timestamps` | `-t` | Include timestamps in output | false |
| `--language` | `-l` | Preferred subtitle language | auto |
| `--ocr-lang` | - | OCR language for Tesseract | eng |
| `--tessdata` | - | Path to Tesseract data directory | ./tessdata |
| `--remove-duplicates` | - | Remove duplicate OCR text | true |

## Examples

### Example 1: Extract from Educational Video

```bash
dotnet run -- \
  --video lecture.mp4 \
  --output lecture_transcript.txt \
  --timestamps
```

### Example 2: Extract from Presentation (No Subtitles)

```bash
dotnet run -- \
  --video presentation.mp4 \
  --output presentation_text.txt \
  --force-ocr \
  --fps 2
```

### Example 3: Multi-language Video

```bash
dotnet run -- \
  --video spanish_video.mp4 \
  --output output.txt \
  --language spa \
  --ocr-lang spa
```

## Project Structure

```
VideoTextExtraction/
├── Models/
│   ├── ProcessingOptions.cs    # Configuration options
│   └── ExtractionResult.cs     # Result model
├── Services/
│   ├── SubtitleDetector.cs     # Detects subtitles
│   ├── SubtitleExtractor.cs    # Extracts subtitle text
│   ├── FrameExtractor.cs       # Extracts video frames
│   ├── OcrEngine.cs            # OCR processing
│   └── VideoTextExtractor.cs   # Main orchestrator
├── Utilities/
│   └── Logger.cs               # Console logging
├── Program.cs                  # CLI entry point
└── VideoTextExtraction.csproj  # Project file
```

## How It Works

### Subtitle Path (Fast & Accurate)

1. **Detection**: Scans video for embedded subtitles and checks for external subtitle files
2. **Extraction**: Extracts subtitle stream or reads external file
3. **Parsing**: Parses subtitle format (SRT, VTT, ASS) to plain text
4. **Output**: Saves extracted text to file

### OCR Path (Slower, Works Without Subtitles)

1. **Frame Extraction**: Extracts frames at specified FPS using FFmpeg
2. **OCR Processing**: Runs Tesseract OCR on each frame
3. **Duplicate Removal**: Removes duplicate text from consecutive frames
4. **Aggregation**: Combines text in chronological order
5. **Output**: Saves extracted text to file

## Performance

| Video Duration | Method | Processing Time (Approx) |
|----------------|--------|--------------------------|
| 1 hour | Subtitles | < 10 seconds |
| 1 hour | OCR (1 FPS) | 3-8 minutes |
| 1 hour | OCR (2 FPS) | 6-15 minutes |

**Note**: OCR performance depends on CPU speed and text complexity.

## Troubleshooting

### "FFmpeg not found"

Make sure FFmpeg is installed and in your PATH. Test with:
```bash
ffmpeg -version
```

### "Tesseract data directory not found"

Download `tessdata` and place in project directory or specify path:
```bash
dotnet run -- --video input.mp4 --output output.txt --tessdata /path/to/tessdata
```

### Poor OCR Accuracy

- Increase FPS: `--fps 2` or `--fps 3`
- Ensure video has clear, readable text
- Try different language: `--ocr-lang <lang>`
- Check video quality (low resolution affects OCR)

### "No subtitles found"

Use `--force-ocr` to extract text from video frames instead.

## Future Enhancements

- [ ] Batch processing (folder of videos)
- [ ] GPU acceleration for OCR
- [ ] Image preprocessing (contrast, threshold)
- [ ] Advanced duplicate detection (Levenshtein distance)
- [ ] JSON output format
- [ ] GUI interface

## Dependencies

- **Xabe.FFmpeg** - FFmpeg wrapper for .NET
- **Tesseract** - OCR engine
- **System.CommandLine** - Modern CLI framework

## License

MIT License - Feel free to use and modify.

## Contributing

Contributions welcome! Please submit issues and pull requests.

## Credits

Built with:
- [FFmpeg](https://ffmpeg.org/) - Video processing
- [Tesseract OCR](https://github.com/tesseract-ocr/tesseract) - Text recognition
- [Xabe.FFmpeg](https://github.com/tomaszzmuda/Xabe.FFmpeg) - .NET FFmpeg wrapper
