@echo off
setlocal enabledelayedexpansion

echo ==============================================
echo SmartQB Build and Deployment Script (Windows)
echo Target Framework: .NET 10
echo ==============================================
echo.

:: Check for .NET SDK
where dotnet >nul 2>nul
if %errorlevel% neq 0 (
    echo [ERROR] .NET CLI is not installed or not in PATH.
    echo Please install the .NET 10 SDK from: https://dotnet.microsoft.com/download/dotnet/10.0
    pause
    exit /b 1
)

:: Set variables
set "PROJECT_PATH=src\SmartQB.UI\SmartQB.UI.csproj"
set "OUTPUT_DIR=publish"
set "CONFIGURATION=Release"

echo [INFO] Restoring dependencies...
dotnet restore "%PROJECT_PATH%"
if %errorlevel% neq 0 (
    echo [ERROR] Failed to restore dependencies.
    pause
    exit /b 1
)

echo.
echo [INFO] Building the solution (%CONFIGURATION%)...
dotnet build "%PROJECT_PATH%" --configuration %CONFIGURATION% --no-restore
if %errorlevel% neq 0 (
    echo [ERROR] Build failed.
    pause
    exit /b 1
)

echo.
echo [INFO] Publishing to '%OUTPUT_DIR%'...
dotnet publish "%PROJECT_PATH%" -c %CONFIGURATION% -o "%OUTPUT_DIR%" --no-build
if %errorlevel% neq 0 (
    echo [ERROR] Publish failed.
    pause
    exit /b 1
)

echo.
echo ==============================================
echo [SUCCESS] Build and Deployment completed successfully!
echo Executable is located in the '%OUTPUT_DIR%' directory.
echo ==============================================

pause
