@echo off
REM Performance Application - Run Script
REM Clean Architecture Version

echo ====================================
echo Performance Management System
echo Clean Architecture - .NET 8
echo ====================================
echo.

cd /d "%~dp0"

echo Building solution...
dotnet build Performance.sln --configuration Debug

if %errorlevel% neq 0 (
    echo.
    echo ERROR: Build failed!
    pause
    exit /b %errorlevel%
)

echo.
echo Build successful! Starting application...
echo.

dotnet run --project src\Performance.UI --no-build

pause
