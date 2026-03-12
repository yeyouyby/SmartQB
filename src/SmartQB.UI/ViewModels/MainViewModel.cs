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

    public ImportViewModel ImportVM { get; }
    public LibraryViewModel LibraryVM { get; }
    public ExportViewModel ExportVM { get; }

    public MainViewModel(ImportViewModel importVM, LibraryViewModel libraryVM, ExportViewModel exportVM, IVersionService versionService)
    {
        ImportVM = importVM;
        LibraryVM = libraryVM;
        ExportVM = exportVM;
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
                break;
            case "Library":
                CurrentViewModel = LibraryVM;
                break;
            case "Export":
                CurrentViewModel = ExportVM;
                break;
        }
    }
}
