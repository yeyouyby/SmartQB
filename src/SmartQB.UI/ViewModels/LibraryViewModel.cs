using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartQB.Core.Entities;
using SmartQB.Core.Interfaces;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;

namespace SmartQB.UI.ViewModels;

public partial class LibraryViewModel : ObservableObject
{
    private readonly IQuestionService _questionService;
    private readonly IVectorService _vectorService;

    [ObservableProperty]
    private string _searchText = string.Empty;

    // Use private field for binding
    private Tag? _selectedTag;
    public Tag? SelectedTag
    {
        get => _selectedTag;
        set
        {
            if (SetProperty(ref _selectedTag, value))
            {
                OnSelectedTagChanged(value);
            }
        }
    }

    [ObservableProperty]
    private bool _isBusy;

    public ObservableCollection<Question> Questions { get; } = new ObservableCollection<Question>();
    public ObservableCollection<Tag> Tags { get; } = new ObservableCollection<Tag>();

    public LibraryViewModel(IQuestionService questionService, IVectorService vectorService)
    {
        _questionService = questionService;
        _vectorService = vectorService;
    }

    public async Task InitializeAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            var tags = await _questionService.GetTagsAsync();
            Tags.Clear();
            foreach (var t in tags) Tags.Add(t);

            await LoadQuestionsInternalAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadQuestionsInternalAsync()
    {
        var questions = await _questionService.GetQuestionsAsync(_selectedTag?.Id);
        Questions.Clear();
        foreach (var q in questions)
        {
            Questions.Add(q);
        }
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            // If empty search, respect current tag state
            await LoadQuestionsInternalAsync();
            return;
        }

        if (IsBusy) return;
        IsBusy = true;
        try
        {
            var results = await _vectorService.SearchSimilarAsync(SearchText);
            Questions.Clear();
            foreach (var q in results)
            {
                Questions.Add(q);
            }

            // Clear tag selection UI but dont trigger reload logic
            // We set backing field directly to avoid triggering OnSelectedTagChanged
            if (_selectedTag != null)
            {
                _selectedTag = null;
                OnPropertyChanged(nameof(SelectedTag));
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void OnSelectedTagChanged(Tag? value)
    {
        if (IsBusy) return;

        if (value != null)
        {
            SearchText = string.Empty;
            _ = LoadQuestionsInternalAsync();
        }
        else if (string.IsNullOrEmpty(SearchText))
        {
            _ = LoadQuestionsInternalAsync();
        }
    }
}
