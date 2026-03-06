# Next Tasks for SmartQB Project

Hi Next AI Agent,

I have completed the core implementation of the WPF UI layout and the infrastructure logic. Here's what has been done:
1. Updated `README.md` to reflect the completed `Recursive Tagging Service` and `UI` phases.
2. Code Audit: Replaced manual dependency injection fields with C# 12 Primary Constructors across ViewModels and Services. Implemented missing `TagQuestionAsync` logic in `TaggingService`.
3. Added the `MainLayout` with a Sidebar and a Content Area that supports ViewModel switching.
4. Implemented `LibraryView` displaying a list of extracted questions (along with tags/logic properties) on the left and full details on the right.
5. Implemented `ExportView` which uses an HTML template injected into `Microsoft.Web.WebView2` (rendering LaTeX via MathJax) and exporting it as a PDF using `PrintToPdfAsync`.

## Completed Tasks

1. **WebView2 Environment Initialization Enhancements**:
   - Done. Uses `WebView2Helper.GetEnvironmentAsync()` in `ExportView.xaml.cs` and `LibraryView.xaml.cs` to properly configure `UserDataFolder`.
2. **UI Polishing**:
   - Done. Integrated `MaterialDesignThemes` in `LibraryView.xaml` and `ExportView.xaml`.
   - Math rendering is working in the left-panel list items via `MathRenderingHelper` using `WpfMath`.
3. **Advanced Filtering**:
   - Done. `LibraryViewModel` effectively provides search and tag-based filtering functionality connected to `VectorService` methods.
4. **Unit Tests**:
   - Done. Addressed for `MainViewModel`, `LibraryViewModel`, and `ExportViewModel`.
5. **Ingestion Polish**:
   - Done. Proper event handlers are set up to capture tagging service completions (`QuestionProcessed` event) directly reloading question lists in the UI thread.

## Next Tasks (Your To-Do List):

1. **Bug Fixing & Testing Edge Cases**:
   - Verify robust error handling.
   - Run End-to-End (E2E) testing with an actual UI runner and fix potential memory leaks on Navigation.
2. **Setup Packaging and CI/CD**:
   - Setup an MSIX or an installer project for standard deployment.
   - Implement GitHub Actions CI for building the .NET 10 project and verifying cross-platform test coverage.

Good luck!
