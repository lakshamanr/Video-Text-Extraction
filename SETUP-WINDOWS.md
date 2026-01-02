# Windows Setup Guide

Complete setup instructions for Windows users.

## Step 1: Install .NET SDK

1. Download .NET 6.0 SDK or later from: https://dotnet.microsoft.com/download
2. Run the installer
3. Verify installation:
   ```powershell
   dotnet --version
   ```

## Step 2: Install FFmpeg

FFmpeg is required for video processing.

### Option A: Using Chocolatey (Recommended)

```powershell
# Install Chocolatey if you don't have it
# Run PowerShell as Administrator
Set-ExecutionPolicy Bypass -Scope Process -Force; [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072; iex ((New-Object System.Net.WebClient).DownloadString('https://community.chocolatey.org/install.ps1'))

# Install FFmpeg
choco install ffmpeg
```

### Option B: Manual Installation

1. Download FFmpeg from: https://github.com/BtbN/FFmpeg-Builds/releases
   - Download: `ffmpeg-master-latest-win64-gpl.zip`

2. Extract the ZIP file to `C:\ffmpeg`

3. Add to PATH:
   - Open **System Properties** → **Environment Variables**
   - Under **System variables**, find and select **Path**
   - Click **Edit** → **New**
   - Add: `C:\ffmpeg\bin`
   - Click **OK** on all dialogs

4. **Restart your computer** (important!)

5. Verify installation:
   ```powershell
   ffmpeg -version
   ```

## Step 3: Install Tesseract OCR

Tesseract is required for OCR (extracting text from video frames).

### Option A: Using Chocolatey (Recommended)

```powershell
# Run PowerShell as Administrator
choco install tesseract
```

### Option B: Manual Installation

1. Download Tesseract from: https://github.com/UB-Mannheim/tesseract/wiki
   - Download: `tesseract-ocr-w64-setup-5.3.x.exe` (latest version)

2. Run the installer
   - **Important**: During installation, note the installation path (usually `C:\Program Files\Tesseract-OCR`)
   - Make sure to install language data files (English is required)

3. Add to PATH:
   - Open **System Properties** → **Environment Variables**
   - Under **System variables**, find and select **Path**
   - Click **Edit** → **New**
   - Add: `C:\Program Files\Tesseract-OCR`
   - Click **OK** on all dialogs

4. Verify installation:
   ```powershell
   tesseract --version
   ```

## Step 4: Download Tesseract Language Data

### If Tesseract was installed via installer:
Language data should already be included. Check:
```powershell
dir "C:\Program Files\Tesseract-OCR\tessdata"
```

### If you need to download manually:

1. Create tessdata folder in your project:
   ```powershell
   mkdir VideoTextExtraction\tessdata
   cd VideoTextExtraction\tessdata
   ```

2. Download English language data:
   ```powershell
   Invoke-WebRequest -Uri "https://github.com/tesseract-ocr/tessdata/raw/main/eng.traineddata" -OutFile "eng.traineddata"
   ```

3. For additional languages:
   ```powershell
   # Spanish
   Invoke-WebRequest -Uri "https://github.com/tesseract-ocr/tessdata/raw/main/spa.traineddata" -OutFile "spa.traineddata"

   # French
   Invoke-WebRequest -Uri "https://github.com/tesseract-ocr/tessdata/raw/main/fra.traineddata" -OutFile "fra.traineddata"
   ```

## Step 5: Configure Tessdata Path in GUI

When using the GUI application:

1. If Tesseract was installed via installer:
   - Change **Tessdata Path** to: `C:\Program Files\Tesseract-OCR\tessdata`

2. If you downloaded manually to project folder:
   - Use the full path like: `C:\Users\YourName\source\repos\Video-Text-Extraction\VideoTextExtraction\tessdata`

## Step 6: Run the GUI Application

```powershell
# Navigate to project directory
cd C:\Users\YourName\source\repos\Video-Text-Extraction

# Run the GUI
.\run-gui.ps1
```

Or open the solution in Visual Studio:
```powershell
# Open in Visual Studio
.\VideoTextExtraction.sln
```

Then set `VideoTextExtraction.GUI` as startup project and press F5.

## Troubleshooting

### Error: "Cannot find FFmpeg in PATH"

**Solution:**
1. Verify FFmpeg is installed:
   ```powershell
   ffmpeg -version
   ```
2. If command not found, FFmpeg is not in PATH
3. Add FFmpeg to PATH following Step 2 above
4. **Restart your computer**
5. **Close and reopen** the GUI application

### Error: "Tesseract data directory not found"

**Solution:**

#### Option 1: Point to installed Tesseract data
In the GUI, change **Tessdata Path** field to:
```
C:\Program Files\Tesseract-OCR\tessdata
```

#### Option 2: Download to project folder
1. Run the setup script:
   ```powershell
   .\setup-tessdata.ps1
   ```
2. In the GUI, change **Tessdata Path** to the full path:
   ```
   C:\Users\YourName\source\repos\Video-Text-Extraction\VideoTextExtraction\tessdata
   ```

### Error: "No subtitles found"

**Solution:**
- If your video has on-screen text (like presentations), check **Force OCR** in the GUI
- Increase **Frames/Second** to 2 for better OCR accuracy

### GUI doesn't start

**Solution:**
1. Make sure .NET 6.0 SDK is installed:
   ```powershell
   dotnet --version
   ```
2. Rebuild the project:
   ```powershell
   cd VideoTextExtraction.GUI
   dotnet clean
   dotnet build
   dotnet run
   ```

## Quick Test

Once everything is set up, test with a sample video:

1. Open the GUI application
2. Click **Browse** next to Video File
3. Select a video with on-screen text
4. Check **Force OCR**
5. Update **Tessdata Path** to: `C:\Program Files\Tesseract-OCR\tessdata`
6. Click **Extract Text**

## Common Paths

| Component | Default Path |
|-----------|--------------|
| FFmpeg (Chocolatey) | `C:\ProgramData\chocolatey\bin\ffmpeg.exe` |
| FFmpeg (Manual) | `C:\ffmpeg\bin\ffmpeg.exe` |
| Tesseract | `C:\Program Files\Tesseract-OCR\tesseract.exe` |
| Tessdata | `C:\Program Files\Tesseract-OCR\tessdata` |

## System Requirements

- **OS**: Windows 10 or later (64-bit)
- **.NET**: 6.0 SDK or later
- **RAM**: 4 GB minimum (8 GB recommended for OCR)
- **Disk**: 500 MB for dependencies + space for videos

## Getting Help

If you encounter issues:
1. Check all prerequisites are installed
2. Verify PATH variables are set correctly
3. Restart your computer after PATH changes
4. Check the [README.md](README.md) for more information
5. Report issues on GitHub

---

**Next Steps:**
- Once setup is complete, see [GUI README](VideoTextExtraction.GUI/README.md) for usage instructions
- For CLI usage, see [EXAMPLES.md](EXAMPLES.md)
