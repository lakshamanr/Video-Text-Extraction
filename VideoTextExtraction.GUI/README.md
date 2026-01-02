# Video Text Extraction - GUI Application

A user-friendly Windows Forms GUI for the Video Text Extraction Tool.

## Features

- **Easy file selection** with browse dialogs
- **Visual configuration** of all extraction options
- **Real-time progress** feedback
- **Console log display** to monitor processing
- **Result summary** with success/error notifications
- **Quick file opening** after extraction completes

## Requirements

- Windows OS (Windows 10 or later recommended)
- .NET 6.0 Runtime or later
- FFmpeg installed and in PATH
- Tesseract OCR installed (for OCR functionality)
- Tesseract language data files

## Installation

### Option 1: Build from Source

```bash
cd VideoTextExtraction.GUI
dotnet restore
dotnet build
dotnet run
```

### Option 2: Publish as Standalone Executable

```bash
cd VideoTextExtraction.GUI

# Publish self-contained executable
dotnet publish -c Release -r win-x64 --self-contained

# The executable will be in:
# bin/Release/net6.0-windows/win-x64/publish/VideoTextExtraction.GUI.exe
```

## Usage

1. **Launch the application**
   - Double-click `VideoTextExtraction.GUI.exe` or run via `dotnet run`

2. **Select video file**
   - Click "Browse..." next to "Video File"
   - Choose your video file (.mp4, .mkv, .avi, etc.)

3. **Choose output location**
   - Click "Browse..." next to "Output File"
   - Select where to save the extracted text
   - (Auto-suggested based on video filename)

4. **Configure options**
   - **Force OCR**: Check to use OCR even if subtitles exist
   - **Include timestamps**: Add timestamps to extracted text
   - **Remove duplicates**: Remove duplicate text from consecutive frames (OCR mode)
   - **Frames/Second**: How many frames per second to extract for OCR (1-10)
   - **Subtitle Language**: Preferred subtitle language (e.g., "eng", "spa")
   - **OCR Language**: Language for Tesseract OCR (e.g., "eng", "spa")
   - **Tessdata Path**: Path to Tesseract language data files

5. **Extract text**
   - Click "Extract Text" button
   - Monitor progress in the log window
   - Wait for completion notification

6. **View results**
   - Click "Yes" in the completion dialog to open the output file
   - Or manually open the output file from the specified location

## Screenshots

### Main Window
The GUI provides:
- File selection dialogs for easy video and output file selection
- Grouped extraction options for clear configuration
- Progress bar for visual feedback
- Console log window showing real-time processing status
- Status label showing current state

### Options Explained

**Extraction Options:**
- **Force OCR**: Useful for videos with presentations or on-screen text where you want to extract visual content rather than subtitles
- **Include timestamps**: Adds timestamp markers to the output text
- **Remove duplicates**: Prevents repeated text when the same content appears in consecutive frames
- **Frames/Second**: Higher values = more accurate but slower (recommended: 1-2)

**Language Settings:**
- **Subtitle Language**: Use language codes like "eng", "spa", "fra", "deu"
- **OCR Language**: Must match installed Tesseract language data files

## Common Use Cases

### Extract from Video with Subtitles
1. Select video
2. Keep default settings
3. Click "Extract Text"
4. Done!

### Extract from PowerPoint Presentation
1. Select presentation video
2. Check "Force OCR"
3. Set "Frames/Second" to 2
4. Check "Include timestamps"
5. Click "Extract Text"

### Extract from Multi-Language Video
1. Select video
2. Enter desired language in "Subtitle Language" (e.g., "spa")
3. If using OCR, set "OCR Language" to match (e.g., "spa")
4. Click "Extract Text"

## Troubleshooting

### "FFmpeg not found" error
- Install FFmpeg: `choco install ffmpeg` (Windows)
- Make sure FFmpeg is in your PATH
- Restart the application after installation

### "Tesseract data directory not found" error
- Download Tesseract language data files
- Run `setup-tessdata.sh` or `setup-tessdata.ps1` from the project root
- Or manually download from: https://github.com/tesseract-ocr/tessdata
- Update "Tessdata Path" in the GUI to point to your tessdata folder

### "No subtitles found" message
- Check "Force OCR" to use OCR extraction instead
- Ensure video actually contains subtitles (check in media player)

### Poor OCR accuracy
- Increase "Frames/Second" value (try 2 or 3)
- Ensure video has good resolution and clear text
- Verify correct OCR language is selected
- Make sure appropriate Tesseract language data is installed

## Building for Distribution

To create a distributable executable:

```bash
# Build self-contained with all dependencies
dotnet publish -c Release -r win-x64 --self-contained /p:PublishSingleFile=true

# This creates a single .exe file that includes:
# - The application
# - .NET runtime
# - All dependencies
```

Note: Users still need FFmpeg and Tesseract installed separately.

## Technical Details

- **Framework**: .NET 6.0 Windows Forms
- **UI Threading**: Async/await pattern for non-blocking UI
- **Console Redirection**: Console output redirected to log window for visibility
- **Project Reference**: References the main VideoTextExtraction console project

## Comparison: GUI vs CLI

| Feature | GUI | CLI |
|---------|-----|-----|
| Ease of use | ⭐⭐⭐⭐⭐ Easy | ⭐⭐⭐ Moderate |
| Batch processing | ❌ No | ✅ Yes |
| Automation | ❌ No | ✅ Yes |
| Visual feedback | ✅ Yes | ⭐⭐⭐ Limited |
| Scripting | ❌ No | ✅ Yes |
| Platform | Windows only | Cross-platform |

**Use the GUI when:**
- Processing individual videos
- You prefer visual interfaces
- You want guided configuration

**Use the CLI when:**
- Batch processing multiple videos
- Automating workflows
- Scripting or integration
- Running on Linux/macOS

## See Also

- [Main README](../README.md) - Full documentation
- [Quick Start Guide](../QUICKSTART.md) - Setup instructions
- [Examples](../EXAMPLES.md) - CLI usage examples

## License

MIT License
