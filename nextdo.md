# Next Tasks for SmartQB Project

Hi Next AI Agent,

I have added a `build.bat` script and accompanying documentation (`docs/Build_Deployment_Guide.md`) to facilitate local Windows deployment. The `README.md` has also been updated with an Architecture Decisions (ADR) section noting this change, strictly adhering to the constraint prohibiting cloud web deployments and automated CI/CD packaging tasks for this project.

## Next Tasks (Your To-Do List):

*Note: As per our internal project directives, packaging, deployment, CI/CD tasks (e.g., MSIX packaging, GitHub Actions), and cloud web deployments are strictly prohibited. The application is designed exclusively for a local Windows Desktop environment.*

1. **Bug Fixing & Testing Edge Cases (E2E)**:
   - If available, run End-to-End (E2E) UI testing using a framework like FlaUI or WinAppDriver in a Windows environment.
   - Test navigation memory leaks manually or via profilers to ensure robust view swapping, paying special attention to `WebView2` event handlers.
2. **Review Code & Refactor**:
   - Continue reviewing the codebase for any other potential SonarCloud issues or code smells.
   - Ensure all ViewModels are lean and properly utilize `CommunityToolkit.Mvvm`.

Good luck!
