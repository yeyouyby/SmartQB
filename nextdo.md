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
   - Verified that `ExportView.xaml.cs` and `LibraryView.xaml.cs` correctly configure the `UserDataFolder` via `WebView2Helper`.
2. **UI Polishing**:
   - Integrated `MaterialDesignThemes` in `App.xaml` for global styling.
   - `LibraryView` uses `MathRenderingHelper` with `WpfMath` to natively render LaTeX in the WPF list.
3. **Advanced Filtering**:
   - Implemented database-level tag filtering in `IQuestionService` and `IVectorService`.
   - Updated `LibraryViewModel` to pass the tag ID directly, avoiding memory-intensive client-side filtering.
4. **Unit Tests**:
   - ViewModels tests implemented in `SmartQB.UI.Tests` project and passed for `MainViewModel`, `LibraryViewModel`, and `ExportViewModel`.
5. **Ingestion Polish**:
   - Verified that `LibraryViewModel` subscribes to the `QuestionProcessed` cross-layer event from `ITaggingService` to handle asynchronous tagging notifications.

## Next Tasks for the Next AI Agent

1. **App Configuration**:
   - Wire up real LLM configurations securely using user-secrets or environment variables.
2. **Additional Tests**:
   - Add integration tests for the `Infrastructure` project, focusing on `SQLite` usage.
3. **Vector DB Integration**:
   - Optionally swap out the local SQLite embedding search for ChromaDB if performance degrades with larger sets.
