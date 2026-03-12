# SmartQB Build & Deployment Guide (Windows)

This document explains how to build and deploy the SmartQB application locally on a Windows machine. As per project constraints, SmartQB is exclusively designed for a local Windows Desktop environment. Cloud web deployments or automated CI/CD packaging (like MSIX/GitHub Actions) are not utilized.

## Prerequisites

Before building the application, ensure the following are installed on your Windows machine:

1.  **Operating System**: Windows 10 or Windows 11.
2.  **.NET 10 SDK**: Download and install the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).

## One-Click Deployment Script (`build.bat`)

A convenient batch script, `build.bat`, is provided in the repository root to automate the build and publish process.

### What the script does:
1.  **Checks Prerequisites**: Verifies that the .NET CLI is installed and available in your system's PATH.
2.  **Restores Dependencies**: Runs `dotnet restore` on the `SmartQB.UI` project to download required NuGet packages.
3.  **Builds the Solution**: Runs `dotnet build` in `Release` configuration.
4.  **Publishes the Application**: Runs `dotnet publish` to output the final compiled assets into the `publish/` directory.

### How to use it:

1.  Open an explorer window in the root directory of the SmartQB repository.
2.  Double-click the `build.bat` file.
    *   *Alternatively*, open a Command Prompt or PowerShell window in the root directory and run `.\build.bat`.
3.  The script will output its progress to the console. If any step fails, it will display an error message and wait for you to press a key before closing.
4.  Upon successful completion, you will see a success message.

## Running the Deployed Application

Once the `build.bat` script completes successfully:

1.  Navigate to the newly created `publish/` folder in the root directory.
2.  Locate the executable file: `SmartQB.UI.exe`.
3.  Double-click `SmartQB.UI.exe` to launch the application.

## Configuration (User Secrets / App Settings)

By default, sensitive information like API keys is expected to be managed securely.
*   **During Development**: It is recommended to use .NET User Secrets (`dotnet user-secrets`).
*   **Published Application**: Ensure you configure your `appsettings.json` (located in the `publish/` folder) with the necessary environment settings, such as the `AI:ApiKey` and `AI:ModelId`, if they are not being loaded via User Secrets or environment variables on the target machine. *Do not commit API keys to version control.*
