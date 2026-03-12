# SmartQB (Intelligent Question Bank)

> **Status**: Development Phase Completed (Core, Infrastructure, UI implemented)
> **Target Framework**: .NET 10
> **UI Framework**: WPF (Windows Presentation Foundation)
> **Architecture**: Clean Architecture (MVVM)

## 📖 Project Overview

SmartQB is an intelligent, local desktop application designed to revolutionize question bank management. It leverages AI (Vision Large Language Models) to transform unstructured PDF exam papers into structured, searchable data assets.

The goal of SmartQB is to solve the traditional problem of "dead" question banks. By extracting the logical characteristics and reasoning paths of each question using AI, SmartQB enables educators to search not just by keywords, but by **problem-solving concepts**.

## ✨ Features

* **Intelligent Ingestion**: Drag and drop PDF files directly into the system. SmartQB automatically analyzes the layout, segments questions, and uses AI vision models to extract the stem, options, answers, and explanations.
* **LaTeX Math Rendering**: All mathematical formulas are strictly converted to LaTeX (`$...$`) for precise rendering via WpfMath and MathJax.
* **Recursive Tagging**: When a new tag is created (e.g., "Derivative Extrema"), SmartQB asynchronously scans existing questions in the background using vector search and AI to automatically back-tag relevant historical questions without blocking the UI.
* **Semantic Search**: Powered by local vector embeddings, users can search for questions using natural language descriptions of the problem's logic, alongside traditional tag combinations.
* **Paper Generation & Export**: Select questions into a "basket" and export them. SmartQB generates an HTML template (rendered seamlessly via WebView2) and exports it as a high-quality PDF. It supports both "Teacher" (with solutions) and "Student" (questions only) editions.

## 🏗️ Technology Stack

* **Runtime**: .NET 10 (C# 12)
* **Frontend**: WPF (Windows Presentation Foundation) styled with `MaterialDesignThemes`.
* **State Management**: `CommunityToolkit.Mvvm` (MVVM Pattern, Source Generators).
* **Database**: Entity Framework Core with SQLite (Metadata).
* **AI Integration**: OpenAI Compatible API (e.g., GPT-4o, DeepSeek) for LLM and Vision tasks.
* **PDF Engine**: `Docnet.Core` (rendering) and `SkiaSharp` (high-performance image processing).
* **Export Engine**: HTML Templates + `Microsoft.Web.WebView2` (`PrintToPdfAsync`).

## 🚀 Getting Started

### Prerequisites
* [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
* A valid OpenAI-compatible API Key (e.g., OpenAI, DeepSeek).

### Configuration (User Secrets)
To keep your API keys secure and prevent them from being committed to source control, SmartQB uses .NET User Secrets.

1. Navigate to the UI project directory:
   ```bash
   cd src/SmartQB.UI
   ```
2. Initialize User Secrets (if not already done):
   ```bash
   dotnet user-secrets init
   ```
3. Set your AI API Key and Model ID:
   ```bash
   dotnet user-secrets set "AI:ApiKey" "your-api-key-here"
   dotnet user-secrets set "AI:ModelId" "gpt-4o"
   ```

*(Alternatively, you can provide these values in `appsettings.json` for non-development environments).*

### Running the Application
1. Build and run the `SmartQB.UI` project:
   ```bash
   dotnet run --project src/SmartQB.UI/SmartQB.UI.csproj
   ```

## 📂 Documentation

* [Product Requirements Document (PRD)](PRD.md) - Detailed business requirements and use cases.


## 🛠️ Architecture Decisions (ADR)
* **Local Deployment Script**: A `build.bat` script and accompanying `docs/Build_Deployment_Guide.md` have been introduced to standardize and automate local Windows desktop deployment, strictly adhering to the constraint prohibiting cloud web deployments and automated CI/CD packaging tasks for this project.
* **Localization & UI Polish**: All English strings in the UI and ViewModels were translated to Chinese. The overall UI was improved using `MaterialDesignThemes` with better layout, active sidebar navigation highlights, and helpful tooltips. `nextdo.md` was removed as requested.
