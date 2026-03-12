#!/bin/bash
cat src/SmartQB.Infrastructure/Services/SettingsService.cs | awk '
/public class SettingsService : ISettingsService/ {
    print $0
    print "{"
    print "    private readonly Microsoft.Extensions.Logging.ILogger<SettingsService> _logger;"
    skip_brace = 1
    next
}
skip_brace && /private readonly Microsoft.Extensions.Logging.ILogger/ { next }
skip_brace && /^{/ { skip_brace = 0; next }
/public async Task SaveAsync()/ {
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
