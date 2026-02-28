using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using SmartQB.Core.Entities;
using System.Threading.Tasks;

namespace SmartQB.UI.ViewModels;

public partial class LibraryViewModel(Core.Interfaces.IQuestionService questionService) : ObservableObject
{
    private readonly Core.Interfaces.IQuestionService _questionService = questionService;

    [ObservableProperty]
    private ObservableCollection<Question> _questions = new();

    [ObservableProperty]
    private Question? _selectedQuestion;

    public async Task LoadQuestionsAsync()
    {
        var list = await _questionService.GetAllQuestionsAsync();
        Questions.Clear();
        foreach (var q in list)
        {
            Questions.Add(q);
        }
    }
}