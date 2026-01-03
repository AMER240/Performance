@echo off
echo ========================================
echo   Performance - .exe Publisher
echo ========================================
echo.

REM Default: Self-Contained (Single .exe)
set DEPLOYMENT_MODE=%1
if "%DEPLOYMENT_MODE%"=="" set DEPLOYMENT_MODE=SelfContained

echo Deployment Mode: %DEPLOYMENT_MODE%
echo.

REM Run PowerShell script
powershell -ExecutionPolicy Bypass -File publish.ps1 -DeploymentMode %DEPLOYMENT_MODE%

pause
