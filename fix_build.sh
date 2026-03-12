#!/bin/bash
cat src/SmartQB.Infrastructure/Services/SettingsService.cs | awk '
/^using System;/ {
    print "using Microsoft.Extensions.Logging;"
    print $0
    next
}
{ print }
' > src/SmartQB.Infrastructure/Services/SettingsService.cs.new
mv src/SmartQB.Infrastructure/Services/SettingsService.cs.new src/SmartQB.Infrastructure/Services/SettingsService.cs

cat src/SmartQB.Infrastructure/Services/LLMService.cs | awk '
/private string _currentApiKey;/ {
    print "    private string? _currentApiKey;"
    print "    private string? _currentBaseUrl;"
    print "    private string? _currentModelId;"
    skip = 1
    next
}
skip && /private string _currentBaseUrl;/ { next }
skip && /private string _currentModelId;/ { skip = 0; next }
skip { next }
{ print }
' > src/SmartQB.Infrastructure/Services/LLMService.cs.new
mv src/SmartQB.Infrastructure/Services/LLMService.cs.new src/SmartQB.Infrastructure/Services/LLMService.cs
