# Run the CAP Announcement Formatter application
Write-Host "Starting CAP Announcement Formatter..." -ForegroundColor Cyan

# Check if built
$outputDir = "src\AnnouncementFormatter\bin\Debug\net8.0"
if (-not (Test-Path $outputDir)) {
    Write-Host "Application not built. Running build first..." -ForegroundColor Yellow
    .\build.ps1
    if ($LASTEXITCODE -ne 0) {
        exit 1
    }
}

# Run the application
Write-Host "`nLaunching application..." -ForegroundColor Green
dotnet run --project src\AnnouncementFormatter
