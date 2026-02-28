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
}
