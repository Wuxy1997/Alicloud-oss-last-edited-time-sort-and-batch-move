@echo off
chcp 65001 >nul
echo ========================================
echo UTF-8 Encoding Conversion Tool
echo ========================================
echo.
echo Running PowerShell script...
echo.

powershell -ExecutionPolicy Bypass -File "%~dp0fix-encoding-clean.ps1"

if errorlevel 1 (
    echo.
    echo [ERROR] Script failed to execute
    pause
    exit /b 1
)

echo.
echo Done!
pause
