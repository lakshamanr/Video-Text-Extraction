# PowerShell script to build and run the GUI application

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Video Text Extraction - GUI Launcher" -ForegroundColor Cyan
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
Write-Host "Building GUI application..." -ForegroundColor Yellow

# Navigate to GUI project
Set-Location VideoTextExtraction.GUI

# Restore and build
dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Failed to restore packages" -ForegroundColor Red
    exit 1
}

dotnet build -c Release
if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Build failed" -ForegroundColor Red
    exit 1
}

Write-Host "✓ Build successful!" -ForegroundColor Green
Write-Host ""
Write-Host "Launching GUI application..." -ForegroundColor Yellow
Write-Host ""

# Run the application
dotnet run -c Release

Set-Location ..
