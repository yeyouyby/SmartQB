using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmartQB.Core.Entities;
using SmartQB.Infrastructure.Data;
using SmartQB.UI.Messages;

namespace SmartQB.UI.ViewModels;

public partial class QuestionBankViewModel : ObservableObject
{
    private readonly IServiceScopeFactory _scopeFactory;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private Question? _selectedQuestion;

    public ObservableCollection<Question> Questions { get; } = new();

    public QuestionBankViewModel(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
        // Load initial data (fire and forget)
        _ = Task.Run(() => LoadQuestionsAsync());
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        await LoadQuestionsAsync();
    }

    private async Task LoadQuestionsAsync()
    {
        using (var scope = _scopeFactory.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SmartQBDbContext>();

            var query = db.Questions.Include(q => q.Tags).AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                // Simple search on Content or LogicDescriptor
                query = query.Where(q => q.Content.Contains(SearchText) ||
                                         (q.LogicDescriptor != null && q.LogicDescriptor.Contains(SearchText)));
            }

            var list = await query.OrderByDescending(q => q.Id).Take(100).ToListAsync();

            App.Current.Dispatcher.Invoke(() =>
            {
                Questions.Clear();
                foreach (var q in list)
                {
                    Questions.Add(q);
                }
            });
        }
    }

    [RelayCommand]
    private void AddToBasket()
    {
        if (SelectedQuestion != null)
        {
            WeakReferenceMessenger.Default.Send(new AddToBasketMessage(SelectedQuestion));
        }
    }
}
