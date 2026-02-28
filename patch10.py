with open('src/SmartQB.UI/ViewModels/MainViewModel.cs', 'r') as f:
    content = f.read()

content = content.replace("    private string _version = versionService.GetVersion();\n\n}", "    private string _version = versionService.GetVersion();\n}")

with open('src/SmartQB.UI/ViewModels/MainViewModel.cs', 'w') as f:
    f.write(content)
