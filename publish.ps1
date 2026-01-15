# Publish the application for distribution
param(
    [Parameter()]
    [ValidateSet('win-x64', 'win-x86', 'win-arm64', 'osx-x64', 'osx-arm64', 'linux-x64')]
    [string]$Runtime = 'win-x64',
    
    [Parameter()]
    [switch]$SingleFile
)

Write-Host "Publishing CAP Announcement Formatter for $Runtime..." -ForegroundColor Cyan

# Create publish directory
$publishDir = "publish\$Runtime"
if (Test-Path $publishDir) {
    Remove-Item $publishDir -Recurse -Force
}

# Build publish command
$publishArgs = @(
    'publish',
    'src\AnnouncementFormatter\AnnouncementFormatter.csproj',
    '-c', 'Release',
    '-r', $Runtime,
    '--self-contained', 'true',
    '-o', $publishDir,
    '-p:PublishReadyToRun=true',
    '-p:PublishTrimmed=false'
)

if ($SingleFile) {
    $publishArgs += '-p:PublishSingleFile=true'
    $publishArgs += '-p:IncludeNativeLibrariesForSelfExtract=true'
}

# Publish
Write-Host "`nPublishing..." -ForegroundColor Yellow
& dotnet @publishArgs

if ($LASTEXITCODE -ne 0) {
    Write-Host "Publish failed" -ForegroundColor Red
    exit 1
}

# Copy assets
Write-Host "`nCopying assets..." -ForegroundColor Yellow
Copy-Item -Path "assets" -Destination "$publishDir\Assets" -Recurse -Force

# Copy documentation
Write-Host "Copying documentation..." -ForegroundColor Yellow
Copy-Item -Path "docs" -Destination "$publishDir\docs" -Recurse -Force
Copy-Item -Path "README.md" -Destination "$publishDir\" -Force

# Create distribution package
Write-Host "`nCreating distribution package..." -ForegroundColor Yellow
$version = "1.0.0"
$packageName = "CAP-Announcement-Formatter-$version-$Runtime"
$packagePath = "publish\$packageName.zip"

if (Test-Path $packagePath) {
    Remove-Item $packagePath -Force
}

Compress-Archive -Path "$publishDir\*" -DestinationPath $packagePath -CompressionLevel Optimal

Write-Host "`nPublish completed successfully!" -ForegroundColor Green
Write-Host "`nOutput directory: $publishDir" -ForegroundColor Cyan
Write-Host "Distribution package: $packagePath" -ForegroundColor Cyan
Write-Host "`nTo run the application:" -ForegroundColor Yellow
Write-Host "  cd $publishDir" -ForegroundColor White
Write-Host "  .\AnnouncementFormatter.exe" -ForegroundColor White
