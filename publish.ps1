# Performance Project Publisher
# .NET 8 WinForms Application

param(
    [ValidateSet('SelfContained', 'FrameworkDependent', 'Optimized')]
    [string]$DeploymentMode = 'SelfContained'
)

Write-Host "========================================" -ForegroundColor Green
Write-Host "  Performance - .exe Publisher" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""

$projectPath = "src\Performance.UI\Performance.csproj"
$outputDir = "publish"

# Clean previous build
if (Test-Path $outputDir) {
    Write-Host "Cleaning previous build..." -ForegroundColor Yellow
    Remove-Item -Path $outputDir -Recurse -Force
}

Write-Host "Deployment Mode: $DeploymentMode" -ForegroundColor Cyan
Write-Host ""

switch ($DeploymentMode) {
    'SelfContained' {
        Write-Host "Publishing Self-Contained (Single .exe with all dependencies)..." -ForegroundColor Cyan
        dotnet publish $projectPath `
            --configuration Release `
            --runtime win-x64 `
            --self-contained true `
            --output $outputDir `
            -p:PublishSingleFile=true `
            -p:IncludeNativeLibrariesForSelfExtract=true `
            -p:EnableCompressionInSingleFile=true `
            -p:DebugType=None `
            -p:DebugSymbols=false
    }
    
    'FrameworkDependent' {
        Write-Host "Publishing Framework-Dependent (Requires .NET 8 Runtime)..." -ForegroundColor Cyan
        dotnet publish $projectPath `
            --configuration Release `
            --runtime win-x64 `
            --self-contained false `
            --output $outputDir `
            -p:PublishSingleFile=true `
            -p:DebugType=None `
            -p:DebugSymbols=false
    }
    
    'Optimized' {
        Write-Host "Publishing Optimized (Trimmed + ReadyToRun)..." -ForegroundColor Cyan
        dotnet publish $projectPath `
            --configuration Release `
            --runtime win-x64 `
            --self-contained true `
            --output $outputDir `
            -p:PublishSingleFile=true `
            -p:PublishReadyToRun=true `
            -p:PublishTrimmed=true `
            -p:TrimMode=link `
            -p:DebugType=None `
            -p:DebugSymbols=false
    }
}

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "  BUILD SUCCESSFUL!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Output directory: $outputDir" -ForegroundColor Cyan
    Write-Host ""
    
    # List files
    Get-ChildItem -Path $outputDir | Format-Table Name, Length -AutoSize
    
    Write-Host ""
    Write-Host "Main executable: $outputDir\Performance.exe" -ForegroundColor Green
    Write-Host ""
    Write-Host "IMPORTANT FILES TO DISTRIBUTE:" -ForegroundColor Yellow
    Write-Host "  - Performance.exe (main application)" -ForegroundColor White
    Write-Host "  - appsettings.json (configuration - EDIT API KEY!)" -ForegroundColor White
    Write-Host ""
    
    # Check appsettings.json
    $appsettingsPath = Join-Path $outputDir "appsettings.json"
    if (Test-Path $appsettingsPath) {
        Write-Host "? appsettings.json found - Remember to add Gemini API Key!" -ForegroundColor Yellow
        Write-Host ""
    }
    
    Write-Host "To run the application:" -ForegroundColor Cyan
    Write-Host "  cd $outputDir" -ForegroundColor White
    Write-Host "  .\Performance.exe" -ForegroundColor White
    Write-Host ""
    
} else {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Red
    Write-Host "  BUILD FAILED!" -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor Red
    Write-Host ""
    Write-Host "Check errors above and try again." -ForegroundColor Yellow
    exit 1
}
