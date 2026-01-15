# Build and run the application
Write-Host "Building CAP Announcement Formatter..." -ForegroundColor Cyan

# Restore NuGet packages
Write-Host "`nRestoring NuGet packages..." -ForegroundColor Yellow
dotnet restore CAP-Announcement-Formatter.sln

if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to restore packages" -ForegroundColor Red
    exit 1
}

# Build solution
Write-Host "`nBuilding solution..." -ForegroundColor Yellow
dotnet build CAP-Announcement-Formatter.sln --configuration Debug

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed" -ForegroundColor Red
    exit 1
}

# Copy assets to output directory
Write-Host "`nCopying assets..." -ForegroundColor Yellow
$outputDir = "src\AnnouncementFormatter\bin\Debug\net8.0"
$assetsDir = "assets"

if (Test-Path $outputDir) {
    if (Test-Path "$outputDir\Assets") {
        Remove-Item "$outputDir\Assets" -Recurse -Force
    }
    Copy-Item -Path $assetsDir -Destination "$outputDir\Assets" -Recurse -Force
    Write-Host "Assets copied successfully" -ForegroundColor Green
}

Write-Host "`nBuild completed successfully!" -ForegroundColor Green
Write-Host "`nTo run the application:" -ForegroundColor Cyan
Write-Host "  dotnet run --project src\AnnouncementFormatter" -ForegroundColor White
