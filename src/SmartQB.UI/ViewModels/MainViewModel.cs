using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using SmartQB.Core.Interfaces;
using System;
using System.Diagnostics;

namespace SmartQB.UI.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IVersionService _versionService;
    private readonly IServiceProvider _serviceProvider;

    [ObservableProperty]
    private string _version;

    [ObservableProperty]
    private object? _currentPage;

    public MainViewModel(IVersionService versionService, IServiceProvider serviceProvider)
    {
        _versionService = versionService;
        _serviceProvider = serviceProvider;
        _version = _versionService.GetVersion();

        // Default to Library View
        NavigateToLibrary();
    }

    [RelayCommand]
    private void NavigateToLibrary()
    {
        CurrentPage = _serviceProvider.GetRequiredService<LibraryViewModel>();
    }

    [RelayCommand]
    private void NavigateToImport()
    {
        CurrentPage = _serviceProvider.GetRequiredService<ImportViewModel>();
    }
}
