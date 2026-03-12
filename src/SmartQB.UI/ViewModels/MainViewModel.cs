using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartQB.Core.Interfaces;

namespace SmartQB.UI.ViewModels;

/// <summary>
/// Root ViewModel that handles global navigation and basic application states.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly IVersionService _versionService;

    [ObservableProperty]
    private string _version;

    [ObservableProperty]
    private object _currentViewModel;

    [ObservableProperty]
    private string _currentRoute = "Import";

    public ImportViewModel ImportVM { get; }
    public LibraryViewModel LibraryVM { get; }
    public ExportViewModel ExportVM { get; }
    public SettingsViewModel SettingsVM { get; }

    public MainViewModel(ImportViewModel importVM, LibraryViewModel libraryVM, ExportViewModel exportVM, SettingsViewModel settingsVM, IVersionService versionService)
    {
        ImportVM = importVM;
        LibraryVM = libraryVM;
        ExportVM = exportVM;
        SettingsVM = settingsVM;
        _versionService = versionService;
        _version = versionService.GetVersion();
        _currentViewModel = importVM;

        ImportVM.ImportCompleted += (s, e) => Navigate("Library");
    }

    /// <summary>
    /// Switches the active view model based on navigation requests from the sidebar.
    /// </summary>
    /// <param name="destination">The string literal of the destination view ("Import", "Library", "Export").</param>
    [RelayCommand]
    private void Navigate(string destination)
    {
        switch (destination)
        {
            case "Import":
                CurrentViewModel = ImportVM;
                CurrentRoute = destination;
                break;
            case "Library":
                CurrentViewModel = LibraryVM;
                CurrentRoute = destination;
                break;
            case "Export":
                CurrentViewModel = ExportVM;
                CurrentRoute = destination;
                break;
            case "Settings":
                CurrentViewModel = SettingsVM;
                CurrentRoute = destination;
                // Trigger async initialization when navigating to Settings
                _ = SettingsVM.InitializeAsync();
                break;
        }
    }
}
