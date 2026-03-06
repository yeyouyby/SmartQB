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
            System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
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

    [ObservableProperty]
    private ObservableCollection<Tag> _tags = new();

    private Tag? _selectedTag;
    public Tag? SelectedTag
    {
        get => _selectedTag;
        set
        {
            if (SetProperty(ref _selectedTag, value))
            {
                _ = LoadQuestionsAsync();
            }
        }
    }

    public async Task LoadTagsAsync()
    {
        var tags = await _questionService.GetAllTagsAsync();
        Tags.Clear();
        foreach (var t in tags)
        {
            Tags.Add(t);
        }
    }

    public async Task LoadQuestionsAsync()
    {
        var list = await _questionService.GetQuestionsAsync(SelectedTag?.Id);
        Questions.Clear();
        foreach (var q in list)
        {
            Questions.Add(q);
        }
    }

    [RelayCommand]
    private void ClearFilter()
    {
        SelectedTag = null;
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery))
        {
            await LoadQuestionsAsync();
            return;
        }

        try
        {
            var results = await _vectorService.SearchSimilarAsync(SearchQuery, 10, SelectedTag?.Id);
            Questions.Clear();
            if (results != null)
            {
                foreach (var q in results)
                {
                    Questions.Add(q);
                }
            }
        }
        catch (Exception)
        {
            // Search failed. Log error or notify UI in a real app.
            // For now, clear the list or keep old list (we chose to clear it and wait for retry).
            Questions.Clear();
        }
    }
}