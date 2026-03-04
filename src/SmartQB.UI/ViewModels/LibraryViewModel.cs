using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using SmartQB.Core.Entities;
using System.Threading.Tasks;

using System;

namespace SmartQB.UI.ViewModels;

public partial class LibraryViewModel : ObservableObject
{
    private readonly Core.Interfaces.IQuestionService _questionService;
    private readonly Core.Interfaces.IVectorService _vectorService;
    private readonly Core.Interfaces.ITaggingService _taggingService;

    public LibraryViewModel(Core.Interfaces.IQuestionService questionService, Core.Interfaces.IVectorService vectorService, Core.Interfaces.ITaggingService taggingService)
    {
        _questionService = questionService;
        _vectorService = vectorService;
        _taggingService = taggingService;

        _taggingService.QuestionProcessed += OnQuestionProcessed;
    }

    private void OnQuestionProcessed(object? sender, EventArgs e)
    {
        if (System.Windows.Application.Current != null)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                _ = LoadQuestionsAsync();
            });
        }
    }

    [ObservableProperty]
    private ObservableCollection<Question> _questions = new();

    [ObservableProperty]
    private Question? _selectedQuestion;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

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
}