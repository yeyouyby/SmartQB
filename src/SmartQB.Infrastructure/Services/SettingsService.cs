using SmartQB.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace SmartQB.Infrastructure.Services;

public class SettingsData
{
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public string ModelId { get; set; } = "gpt-4o";
}

public class SettingsService : ISettingsService
{
    private readonly Microsoft.Extensions.Logging.ILogger<SettingsService> _logger;
    private readonly string _settingsFilePath;
    private SettingsData _data;

    public string ApiKey
    {
        get => _data.ApiKey;
        set => _data.ApiKey = value;
    }

    public string BaseUrl
    {
        get => _data.BaseUrl;
        set => _data.BaseUrl = value;
    }

    public string ModelId
    {
        get => _data.ModelId;
        set => _data.ModelId = value;
    }

    public SettingsService(Microsoft.Extensions.Logging.ILogger<SettingsService> logger)
    {
        _logger = logger;
        var appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SmartQB");
        if (!Directory.Exists(appDataFolder))
        {
            Directory.CreateDirectory(appDataFolder);
        }
        _settingsFilePath = Path.Combine(appDataFolder, "settings.json");
        _data = new SettingsData();
    }

    public async Task LoadAsync()
    {
        if (!File.Exists(_settingsFilePath))
        {
            _data = new SettingsData();
            return;
        }

        try
        {
            var json = await File.ReadAllTextAsync(_settingsFilePath).ConfigureAwait(false);
            _data = JsonSerializer.Deserialize<SettingsData>(json) ?? new SettingsData();
        }
        catch (FileNotFoundException)
        {
            _data = new SettingsData();
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize settings from {SettingsFilePath}", _settingsFilePath);
            _data = new SettingsData();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while loading settings from {SettingsFilePath}", _settingsFilePath);
            _data = new SettingsData();
        }
    }
    public async Task SaveAsync()
    {
        try
        {
            var json = JsonSerializer.Serialize(_data, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_settingsFilePath, json).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save settings to {SettingsFilePath}", _settingsFilePath);
            throw; // Let the caller handle it (e.g. ViewModel showing error message)
        }
    }
}
