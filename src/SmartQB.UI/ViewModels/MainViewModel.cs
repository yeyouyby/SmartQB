using CommunityToolkit.Mvvm.ComponentModel;
using SmartQB.Core.Interfaces;
using System.Diagnostics;

namespace SmartQB.UI.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IVersionService _versionService;
    private readonly IPdfService _pdfService;
    private readonly ILLMService _llmService;

    [ObservableProperty]
    private string _version;

    public MainViewModel(IVersionService versionService, IPdfService pdfService, ILLMService llmService)
    {
        _versionService = versionService;
        _pdfService = pdfService;
        _llmService = llmService;
        _version = _versionService.GetVersion();

        // Verification check
        Debug.WriteLine($"Services Injected: PDF={_pdfService != null}, LLM={_llmService != null}");
    }
}
