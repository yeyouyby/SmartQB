using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using SmartQB.Core.Entities;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.Input;
using SmartQB.Core.Interfaces;

namespace SmartQB.UI.ViewModels;

public partial class LibraryViewModel : ObservableObject
{
    private readonly IQuestionService _questionService;
    private readonly IIngestionService _ingestionService;
    private readonly IVectorService _vectorService;

    public LibraryViewModel(IQuestionService questionService, IIngestionService ingestionService, IVectorService vectorService)
    {
        _questionService = questionService;
        _ingestionService = ingestionService;
        _vectorService = vectorService;

        _ingestionService.QuestionIngested += (sender, args) =>
        {
            Application.Current?.Dispatcher.InvokeAsync(LoadQuestionsAsync);
        };
    }

    [ObservableProperty]
    private ObservableCollection<Question> _questions = new();

    [ObservableProperty]
    private Question? _selectedQuestion;

    [ObservableProperty]
    private string _searchQuery = "";

    public async Task LoadQuestionsAsync()
    {
        var list = await _questionService.GetAllQuestionsAsync();
        Questions.Clear();
        foreach (var q in list)
        {
            Questions.Add(q);
        }
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery))
        {
            await LoadQuestionsAsync();
            return;
        }

        var results = await _vectorService.SearchSimilarAsync(SearchQuery);
        Questions.Clear();
        foreach (var q in results)
        {
            Questions.Add(q);
        }
    }

    [RelayCommand]
    private async Task ClearSearchAsync()
    {
        SearchQuery = string.Empty;
        await LoadQuestionsAsync();
    }
}