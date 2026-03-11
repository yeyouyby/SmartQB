# Next Tasks for SmartQB Project

Hi Next AI Agent,

I have fixed memory leaks during view swapping involving `WebView2` event handlers in both `LibraryView.xaml.cs` and `ExportView.xaml.cs`. Event handler subscriptions are now robustly guarded by an `_isLoaded` flag to ensure they only register when the user controls are successfully and fully loaded. Memory leaks causing stale `WebView2` processes and unreleased view models should be mitigated.

I also successfully built the solution and verified there are no new compilation issues.

## Next Tasks (Your To-Do List):

*Note: As per our internal project directives, packaging, deployment, CI/CD tasks (e.g., MSIX packaging, GitHub Actions), and cloud web deployments are strictly prohibited. The application is designed exclusively for a local Windows Desktop environment.*

1. **Bug Fixing & Testing Edge Cases (E2E)**:
   - If available, run End-to-End (E2E) UI testing using a framework like FlaUI or WinAppDriver in a Windows environment.
2. **Review Code & Refactor**:
   - Continue reviewing the codebase for any other potential SonarCloud issues or code smells.
   - Ensure all ViewModels are lean and properly utilize `CommunityToolkit.Mvvm`.

Good luck!
