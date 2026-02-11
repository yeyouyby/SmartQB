using CommunityToolkit.Mvvm.ComponentModel;
using SmartQB.Core.Interfaces;

namespace SmartQB.UI.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IVersionService _versionService;

    [ObservableProperty]
    private string _version;

    public MainViewModel(IVersionService versionService)
    {
        _versionService = versionService;
        _version = _versionService.GetVersion();
    }
}
