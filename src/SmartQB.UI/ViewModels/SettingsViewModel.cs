using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartQB.Core.Interfaces;
using System.Threading.Tasks;
using System.Windows;

namespace SmartQB.UI.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;

    [ObservableProperty]
    private string _apiKey = string.Empty;

    [ObservableProperty]
    private string _baseUrl = string.Empty;

    [ObservableProperty]
    private string _modelId = string.Empty;

    public SettingsViewModel(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public Task InitializeAsync()
    {
        return LoadSettingsAsync();
    }

    [RelayCommand]
    private async Task LoadSettingsAsync()
    {
        await _settingsService.LoadAsync();
        ApiKey = _settingsService.ApiKey;
        BaseUrl = _settingsService.BaseUrl;
        ModelId = _settingsService.ModelId;
    }

    [RelayCommand]
    private async Task SaveSettingsAsync()
    {
        _settingsService.ApiKey = ApiKey;
        _settingsService.BaseUrl = BaseUrl;
        _settingsService.ModelId = ModelId;
        try
        {
            await _settingsService.SaveAsync();
            MessageBox.Show("Settings saved successfully.", "SmartQB", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (System.Exception ex)
        {
            MessageBox.Show($"Failed to save settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
