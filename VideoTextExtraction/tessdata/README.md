# Tesseract Language Data

This directory should contain Tesseract OCR language data files.

## Quick Setup

### Option 1: Run the setup script (recommended)

**Linux/macOS:**
```bash
cd ..
chmod +x setup-tessdata.sh
./setup-tessdata.sh
```

**Windows (PowerShell):**
```powershell
cd ..
.\setup-tessdata.ps1
```

### Option 2: Manual download

Download language files from the official Tesseract repository:
https://github.com/tesseract-ocr/tessdata

**For English (required):**
```bash
curl -L -o eng.traineddata https://github.com/tesseract-ocr/tessdata/raw/main/eng.traineddata
```

**For other languages:**
- Spanish: `spa.traineddata`
- French: `fra.traineddata`
- German: `deu.traineddata`
- Chinese Simplified: `chi_sim.traineddata`
- Japanese: `jpn.traineddata`
- See full list: https://github.com/tesseract-ocr/tessdata

## File Structure

After setup, this directory should contain:
```
tessdata/
├── README.md (this file)
├── eng.traineddata
├── spa.traineddata (optional)
├── fra.traineddata (optional)
└── ... (other languages)
```

## Specifying Custom Path

If you want to use a different tessdata location:
```bash
dotnet run -- --video input.mp4 --output output.txt --tessdata /path/to/tessdata
```

## Language Codes

Common language codes for the `--ocr-lang` option:
- English: `eng`
- Spanish: `spa`
- French: `fra`
- German: `deu`
- Italian: `ita`
- Portuguese: `por`
- Russian: `rus`
- Chinese (Simplified): `chi_sim`
- Chinese (Traditional): `chi_tra`
- Japanese: `jpn`
- Korean: `kor`
- Arabic: `ara`

## Troubleshooting

**Error: "Tesseract data directory not found"**
- Make sure this directory contains at least `eng.traineddata`
- Run the setup script or download manually

**Error: "Language data not found"**
- Download the specific language data file you need
- Specify the correct language code with `--ocr-lang`
