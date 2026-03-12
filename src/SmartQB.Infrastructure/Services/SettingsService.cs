using SmartQB.Core.Interfaces;
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

    public SettingsService()
    {
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
        if (File.Exists(_settingsFilePath))
        {
            try
            {
                var json = await File.ReadAllTextAsync(_settingsFilePath).ConfigureAwait(false);
                _data = JsonSerializer.Deserialize<SettingsData>(json) ?? new SettingsData();
            }
            catch
            {
                _data = new SettingsData(); // Fallback on error
            }
        }
    }

    public async Task SaveAsync()
    {
        try
        {
            var json = JsonSerializer.Serialize(_data, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_settingsFilePath, json);
        }
        catch
        {
            // Logging can be added here
        }
    }
}
