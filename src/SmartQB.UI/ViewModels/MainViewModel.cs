using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartQB.Core.Interfaces;
using System.Threading.Tasks;

namespace SmartQB.UI.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IVersionService _versionService;

    [ObservableProperty]
    private string _version;

    [ObservableProperty]
    private ObservableObject _currentViewModel;

    public IngestionViewModel IngestionVM { get; }
    public QuestionBankViewModel QuestionBankVM { get; }
    public PaperCompositionViewModel PaperCompositionVM { get; }

    public MainViewModel(
        IVersionService versionService,
        IngestionViewModel ingestionVM,
        QuestionBankViewModel questionBankVM,
        PaperCompositionViewModel paperCompositionVM)
    {
        _versionService = versionService;
        IngestionVM = ingestionVM;
        QuestionBankVM = questionBankVM;
        PaperCompositionVM = paperCompositionVM;

        _version = _versionService.GetVersion();

        // Default View
        CurrentViewModel = IngestionVM;
    }

    [RelayCommand]
    private async Task NavigateAsync(string viewName)
    {
        switch (viewName)
        {
            case "Ingestion":
                CurrentViewModel = IngestionVM;
                break;
            case "QuestionBank":
                CurrentViewModel = QuestionBankVM;
                await QuestionBankVM.InitializeAsync();
                break;
            case "Composition":
                CurrentViewModel = PaperCompositionVM;
                break;
        }
    }
}
