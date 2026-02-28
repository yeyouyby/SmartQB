# Next Tasks for SmartQB Project

Hi Next AI Agent,

I have completed the core implementation of the WPF UI layout and the infrastructure logic. Here's what has been done:
1. Updated `README.md` to reflect the completed `Recursive Tagging Service` and `UI` phases.
2. Code Audit: Replaced manual dependency injection fields with C# 12 Primary Constructors across ViewModels and Services. Implemented missing `TagQuestionAsync` logic in `TaggingService`.
3. Added the `MainLayout` with a Sidebar and a Content Area that supports ViewModel switching.
4. Implemented `LibraryView` displaying a list of extracted questions (along with tags/logic properties) on the left and full details on the right.
5. Implemented `ExportView` which uses an HTML template injected into `Microsoft.Web.WebView2` (rendering LaTeX via MathJax) and exporting it as a PDF using `PrintToPdfAsync`.

## Pending Tasks (Your To-Do List):

1. **WebView2 Environment Initialization Enhancements**:
   - `ExportView.xaml.cs` currently has a simple `EnsureCoreWebView2Async(null)` call. It should properly configure the UserDataFolder to a temporary or application-specific path (e.g., `AppData/Local/SmartQB/WebView2`) to avoid permissions issues when the app is installed globally.
2. **UI Polishing**:
   - Improve the design and styles using WPF control templates or a library like `WPF UI` or `MaterialDesignThemes`.
   - The `LibraryView` list items currently display unrendered LaTeX. It would be awesome if the left-panel previews also render math formulas nicely (either via WebView2 blocks or a dedicated WPF Math rendering library).
3. **Advanced Filtering**:
   - `LibraryView` needs search and filtering functionalities based on Tags and Vector Embeddings (Semantic Search). Implement UI components and connect them to the `VectorService` search methods.
4. **Unit Tests**:
   - Write tests for `MainViewModel`, `LibraryViewModel`, and `ExportViewModel`.
5. **Ingestion Polish**:
   - The `IngestionService` uses `TagQuestionAsync` asynchronously. Add UI notification handling via an EventAggregator or Messenger (`CommunityToolkit.Mvvm.Messaging`) to notify the `LibraryView` when new questions have been completely tagged and processed.

Good luck!
