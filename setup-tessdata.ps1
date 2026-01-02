# PowerShell script for downloading Tesseract language data

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Tesseract Language Data Setup" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

# Create tessdata directory
Write-Host "Creating tessdata directory..." -ForegroundColor Yellow
New-Item -ItemType Directory -Force -Path "tessdata" | Out-Null
Set-Location tessdata

# Function to download language data
function Download-Language {
    param (
        [string]$Lang,
        [string]$LangName
    )

    Write-Host "Downloading $LangName language data..." -ForegroundColor Yellow
    $url = "https://github.com/tesseract-ocr/tessdata/raw/main/$Lang.traineddata"
    $output = "$Lang.traineddata"

    try {
        Invoke-WebRequest -Uri $url -OutFile $output
        Write-Host "[OK] $LangName downloaded successfully" -ForegroundColor Green
    }
    catch {
        Write-Host "[ERROR] Failed to download $LangName" -ForegroundColor Red
    }
}

# Download English (required)
Download-Language -Lang "eng" -LangName "English"

# Ask user for additional languages
Write-Host ""
$response = Read-Host "Do you want to download additional languages? (y/n)"

if ($response -match "^[Yy]") {
    Write-Host ""
    Write-Host "Available languages:"
    Write-Host "1. Spanish (spa)"
    Write-Host "2. French (fra)"
    Write-Host "3. German (deu)"
    Write-Host "4. Chinese Simplified (chi_sim)"
    Write-Host "5. Japanese (jpn)"
    Write-Host "6. Arabic (ara)"
    Write-Host "7. Russian (rus)"
    Write-Host "8. Portuguese (por)"
    Write-Host ""
    $languages = Read-Host "Enter language codes separated by spaces (e.g. spa fra deu)"

    foreach ($lang in $languages.Split(" ")) {
        switch ($lang) {
            "spa" { Download-Language -Lang "spa" -LangName "Spanish" }
            "fra" { Download-Language -Lang "fra" -LangName "French" }
            "deu" { Download-Language -Lang "deu" -LangName "German" }
            "chi_sim" { Download-Language -Lang "chi_sim" -LangName "Chinese Simplified" }
            "jpn" { Download-Language -Lang "jpn" -LangName "Japanese" }
            "ara" { Download-Language -Lang "ara" -LangName "Arabic" }
            "rus" { Download-Language -Lang "rus" -LangName "Russian" }
            "por" { Download-Language -Lang "por" -LangName "Portuguese" }
            default { Write-Host "Unknown language: $lang" -ForegroundColor Red }
        }
    }
}

Set-Location ..

Write-Host ""
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "Setup complete!" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Installed language data files:"
Get-ChildItem tessdata\*.traineddata -ErrorAction SilentlyContinue | Format-Table Name, Length
Write-Host ""
Write-Host "You can now run the application:" -ForegroundColor Yellow
Write-Host "  dotnet run -- --video input.mp4 --output output.txt" -ForegroundColor White
Write-Host ""
