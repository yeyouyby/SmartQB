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
            MessageBox.Show("设置保存成功。", "SmartQB 提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to save settings: {ex}");
            MessageBox.Show("保存设置失败，请检查输入或查看日志。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
