# PowerShell script to build GUI application for distribution

param(
    [switch]$SingleFile = $false
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Video Text Extraction - GUI Builder" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if .NET SDK is installed
try {
    $dotnetVersion = dotnet --version
    Write-Host "✓ .NET SDK detected: $dotnetVersion" -ForegroundColor Green
}
catch {
    Write-Host "✗ .NET SDK not found!" -ForegroundColor Red
    Write-Host "Please install .NET 6.0 or later from:" -ForegroundColor Yellow
    Write-Host "https://dotnet.microsoft.com/download" -ForegroundColor Yellow
    exit 1
}

Write-Host ""
Write-Host "Building GUI application for distribution..." -ForegroundColor Yellow
Write-Host ""

# Navigate to GUI project
Set-Location VideoTextExtraction.GUI

# Clean previous builds
if (Test-Path "bin") {
    Remove-Item -Path "bin" -Recurse -Force
}
if (Test-Path "obj") {
    Remove-Item -Path "obj" -Recurse -Force
}

# Build based on options
if ($SingleFile) {
    Write-Host "Building self-contained single-file executable..." -ForegroundColor Yellow
    dotnet publish -c Release -r win-x64 --self-contained /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true
}
else {
    Write-Host "Building self-contained executable..." -ForegroundColor Yellow
    dotnet publish -c Release -r win-x64 --self-contained
}

if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Build failed" -ForegroundColor Red
    Set-Location ..
    exit 1
}

Write-Host ""
Write-Host "✓ Build successful!" -ForegroundColor Green
Write-Host ""

$publishPath = "bin\Release\net6.0-windows\win-x64\publish"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Build Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Published files location:" -ForegroundColor Yellow
Write-Host "  $publishPath" -ForegroundColor White
Write-Host ""
Write-Host "Main executable:" -ForegroundColor Yellow
Write-Host "  $publishPath\VideoTextExtraction.GUI.exe" -ForegroundColor White
Write-Host ""

if ($SingleFile) {
    Write-Host "Single file mode: All dependencies are bundled" -ForegroundColor Green
}
else {
    Write-Host "Multiple files mode: Distribute entire publish folder" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "IMPORTANT NOTES:" -ForegroundColor Red
Write-Host "  1. Users must install FFmpeg separately" -ForegroundColor Yellow
Write-Host "  2. Users must install Tesseract OCR for OCR functionality" -ForegroundColor Yellow
Write-Host "  3. Tesseract language data files are NOT included" -ForegroundColor Yellow
Write-Host ""
Write-Host "To test the build:" -ForegroundColor Cyan
Write-Host "  & '$publishPath\VideoTextExtraction.GUI.exe'" -ForegroundColor White
Write-Host ""

Set-Location ..
