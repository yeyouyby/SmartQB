using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using SmartQB.Core.Entities;
using System.Threading.Tasks;
using System;

namespace SmartQB.UI.ViewModels;

public partial class LibraryViewModel(Core.Interfaces.IQuestionService questionService, Core.Interfaces.IVectorService vectorService, Core.Interfaces.ITaggingService taggingService) : ObservableObject
{
    private readonly Core.Interfaces.IQuestionService _questionService = questionService;
    private readonly Core.Interfaces.IVectorService _vectorService = vectorService;
    private readonly Core.Interfaces.ITaggingService _taggingService = taggingService;

    // Use a parameterless init method called from View, or subscribe right here in a static context if not possible, but we can just subscribe when someone needs it or use a trick to run initialization:
    private bool _isInitialized;

    public void Initialize()
    {
        if (_isInitialized) return;
        _taggingService.QuestionProcessed += OnQuestionProcessed;
        _isInitialized = true;
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
        var list = await _questionService.GetAllQuestionsAsync();
        Questions.Clear();
        foreach (var q in list)
        {
            if (SelectedTag != null)
            {
                bool hasTag = false;
                foreach (var t in q.Tags)
                {
                    if (t.Id == SelectedTag.Id)
                    {
                        hasTag = true;
                        break;
                    }
                }
                if (!hasTag) continue;
            }
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
            var results = await _vectorService.SearchSimilarAsync(SearchQuery, 10);
            Questions.Clear();
            if (results != null)
            {
                foreach (var q in results)
                {
                    if (SelectedTag != null)
                    {
                        bool hasTag = false;
                        foreach (var t in q.Tags)
                        {
                            if (t.Id == SelectedTag.Id)
                            {
                                hasTag = true;
                                break;
                            }
                        }
                        if (!hasTag) continue;
                    }
                    Questions.Add(q);
                }
            }
        }
        catch (Exception)
        {
            Questions.Clear();
        }
    }
}
