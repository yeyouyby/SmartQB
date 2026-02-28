using CommunityToolkit.Mvvm.ComponentModel;
using SmartQB.Core.Interfaces;
using System.Diagnostics;

namespace SmartQB.UI.ViewModels;

public partial class MainViewModel(ImportViewModel importVM, IVersionService versionService, IPdfService pdfService, ILLMService llmService) : ObservableObject
{
    private readonly IVersionService _versionService = versionService;
    private readonly IPdfService _pdfService = pdfService;
    private readonly ILLMService _llmService = llmService;

    public ImportViewModel ImportVM { get; } = importVM;

    [ObservableProperty]
    private string _version = versionService.GetVersion();

    // The constructor body logic from before can be placed in a method or property setter.
    // For MVVM, initializing in a property or command is preferred if side-effects exist.
    // Here, Debug.WriteLine was just for a sanity check, we can safely remove or re-implement differently.
    // Since it requires a method body, we can keep it simple: we know it injected correctly if it runs.
}
