#!/bin/bash
cat src/SmartQB.Infrastructure/Services/SettingsService.cs | awk '
/public class SettingsService : ISettingsService/ {
    print $0
    print "    private readonly Microsoft.Extensions.Logging.ILogger<SettingsService> _logger;"
    next
}
/public SettingsService\(\)/ {
    print "    public SettingsService(Microsoft.Extensions.Logging.ILogger<SettingsService> logger)"
    print "    {"
    print "        _logger = logger;"
    skip_ctor = 1
    next
}
skip_ctor && /var appDataFolder/ { skip_ctor = 0; print $0; next }
skip_ctor { next }

/public async Task LoadAsync\(\)/ {
    print "    public async Task LoadAsync()"
    print "    {"
    print "        if (!File.Exists(_settingsFilePath))"
    print "        {"
    print "            _data = new SettingsData();"
    print "            return;"
    print "        }"
    print ""
    print "        try"
    print "        {"
    print "            var json = await File.ReadAllTextAsync(_settingsFilePath).ConfigureAwait(false);"
    print "            _data = JsonSerializer.Deserialize<SettingsData>(json) ?? new SettingsData();"
    print "        }"
    print "        catch (FileNotFoundException)"
    print "        {"
    print "            _data = new SettingsData();"
    print "        }"
    print "        catch (JsonException ex)"
    print "        {"
    print "            _logger.LogError(ex, \"Failed to deserialize settings from {SettingsFilePath}\", _settingsFilePath);"
    print "            _data = new SettingsData();"
    print "        }"
    print "        catch (Exception ex)"
    print "        {"
    print "            _logger.LogError(ex, \"Unexpected error while loading settings from {SettingsFilePath}\", _settingsFilePath);"
    print "            _data = new SettingsData();"
    print "        }"
    print "    }"
    skip_load = 1
    next
}
skip_load && /public async Task SaveAsync\(\)/ { skip_load = 0; print $0; next }
skip_load { next }

/public async Task SaveAsync\(\)/ {
    print "    public async Task SaveAsync()"
    print "    {"
    print "        try"
    print "        {"
    print "            var json = JsonSerializer.Serialize(_data, new JsonSerializerOptions { WriteIndented = true });"
    print "            await File.WriteAllTextAsync(_settingsFilePath, json).ConfigureAwait(false);"
    print "        }"
    print "        catch (Exception ex)"
    print "        {"
    print "            _logger.LogError(ex, \"Failed to save settings to {SettingsFilePath}\", _settingsFilePath);"
    print "            throw; // Let the caller handle it (e.g. ViewModel showing error message)"
    print "        }"
    print "    }"
    skip_save = 1
    next
}
skip_save && /^}/ { skip_save = 0; print $0; next }
skip_save { next }
{ print }
' > src/SmartQB.Infrastructure/Services/SettingsService.cs.new
mv src/SmartQB.Infrastructure/Services/SettingsService.cs.new src/SmartQB.Infrastructure/Services/SettingsService.cs
