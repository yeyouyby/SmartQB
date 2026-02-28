using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartQB.Core.Interfaces;

namespace SmartQB.UI.ViewModels;

public partial class MainViewModel(ImportViewModel importVM, LibraryViewModel libraryVM, ExportViewModel exportVM, IVersionService versionService) : ObservableObject
{
    private readonly IVersionService _versionService = versionService;

    [ObservableProperty]
    private string _version = versionService.GetVersion();

    [ObservableProperty]
    private object _currentViewModel = importVM;

    public ImportViewModel ImportVM { get; } = importVM;
    public LibraryViewModel LibraryVM { get; } = libraryVM;
    public ExportViewModel ExportVM { get; } = exportVM;

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