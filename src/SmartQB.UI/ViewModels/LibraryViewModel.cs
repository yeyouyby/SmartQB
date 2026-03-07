using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using SmartQB.Core.Entities;
using System.Threading.Tasks;
using System;
using System.Diagnostics;

namespace SmartQB.UI.ViewModels;

/// <summary>
/// ViewModel for the Library view, handling question display, filtering, and semantic search.
/// </summary>
public partial class LibraryViewModel(Core.Interfaces.IQuestionService questionService, Core.Interfaces.IVectorService vectorService, Core.Interfaces.ITaggingService taggingService) : ObservableObject
{
    private readonly Core.Interfaces.IQuestionService _questionService = questionService;
    private readonly Core.Interfaces.IVectorService _vectorService = vectorService;
    private readonly Core.Interfaces.ITaggingService _taggingService = taggingService;

    private bool _isInitialized;

    /// <summary>
    /// Activates the ViewModel, subscribing to necessary background events.
    /// Should be called when the view is loaded.
    /// </summary>
    public void Activate()
    {
        if (_isInitialized) return;
        _taggingService.QuestionProcessed += OnQuestionProcessed;
        _isInitialized = true;
    }

    /// <summary>
    /// Deactivates the ViewModel, unsubscribing from background events to prevent memory leaks.
    /// Should be called when the view is unloaded.
    /// </summary>
    public void Deactivate()
    {
        if (!_isInitialized) return;
        _taggingService.QuestionProcessed -= OnQuestionProcessed;
        _isInitialized = false;
    }

    /// <summary>
    /// Event handler for when a question has been processed by the tagging service.
    /// Refreshes the question list on the UI thread.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the selected tag for filtering the question list.
    /// Automatically reloads questions when changed.
    /// </summary>
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

    /// <summary>
    /// Loads all available tags from the database into the observable collection.
    /// </summary>
    public async Task LoadTagsAsync()
    {
        var tags = await _questionService.GetAllTagsAsync();
        Tags.Clear();
        foreach (var t in tags)
        {
            Tags.Add(t);
        }
    }

    /// <summary>
    /// Loads all questions from the database, applying the currently selected tag filter if any.
    /// </summary>
    public async Task LoadQuestionsAsync()
    {
        var list = await _questionService.GetAllQuestionsAsync(SelectedTag?.Id);
        Questions = new ObservableCollection<Question>(list);
    }

    /// <summary>
    /// Clears the currently selected tag filter.
    /// </summary>
    [RelayCommand]
    private void ClearFilter()
    {
        SelectedTag = null;
    }

    /// <summary>
    /// Performs a semantic search using the vector service based on the current search query.
    /// Refreshes the question list with the search results.
    /// </summary>
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
            // TODO: Inject a proper logger service.
            Debug.WriteLine($"Search failed: {ex}");
            Questions = new ObservableCollection<Question>();
        }
    }
}
