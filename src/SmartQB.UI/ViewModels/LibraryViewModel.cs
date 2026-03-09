using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using SmartQB.Core.Entities;
using System.Threading.Tasks;
using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace SmartQB.UI.ViewModels;

public partial class LibraryViewModel(Core.Interfaces.IQuestionService questionService, Core.Interfaces.IVectorService vectorService, Core.Interfaces.ITaggingService taggingService, ILogger<LibraryViewModel> logger) : ObservableObject
{
    private readonly Core.Interfaces.IQuestionService _questionService = questionService;
    private readonly Core.Interfaces.IVectorService _vectorService = vectorService;
    private readonly Core.Interfaces.ITaggingService _taggingService = taggingService;
    private readonly ILogger<LibraryViewModel> _logger = logger;

    private CancellationTokenSource? _searchCts;
    private bool _isInitialized;

    public void Activate()
    {
        if (_isInitialized) return;
        _taggingService.QuestionProcessed += OnQuestionProcessed;
        _isInitialized = true;
    }

    public void Deactivate()
    {
        if (!_isInitialized) return;
        _taggingService.QuestionProcessed -= OnQuestionProcessed;
        _isInitialized = false;
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
    private bool _isSearching;

    partial void OnSearchQueryChanged(string value)
    {
        _searchCts?.Cancel();
        _searchCts = new CancellationTokenSource();

        _ = DebouncedSearchAsync(value, _searchCts.Token);
    }

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
        try
        {
            var tags = await _questionService.GetAllTagsAsync();
            Tags = new ObservableCollection<Tag>(tags);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load tags");
        }
    }

    public async Task LoadQuestionsAsync()
    {
        try
        {
            var list = await _questionService.GetAllQuestionsAsync(SelectedTag?.Id);
            Questions = new ObservableCollection<Question>(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load questions");
        }
    }

    [RelayCommand]
    private void ClearFilter()
    {
        SelectedTag = null;
    }

    private async Task DebouncedSearchAsync(string query, CancellationToken token)
    {
        try
        {
            await Task.Delay(500, token); // 500ms debounce
            if (!token.IsCancellationRequested)
            {
                await SearchAsync();
            }
        }
        catch (TaskCanceledException)
        {
            // Ignore cancellation
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

        IsSearching = true;
        try
        {
            var results = await _vectorService.SearchSimilarAsync(SearchQuery, 10, SelectedTag?.Id);
            if (results != null)
            {
                Questions = new ObservableCollection<Question>(results);
            }
            else
            {
                Questions = new ObservableCollection<Question>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Search failed");
            Questions = new ObservableCollection<Question>();
        }
        finally
        {
            IsSearching = false;
        }
    }
}
