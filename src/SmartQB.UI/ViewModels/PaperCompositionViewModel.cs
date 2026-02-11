using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SmartQB.Core.Entities;
using SmartQB.UI.Messages;

namespace SmartQB.UI.ViewModels;

public partial class PaperCompositionViewModel : ObservableObject, IRecipient<AddToBasketMessage>
{
    [ObservableProperty]
    private bool _isTeacherVersion;

    public ObservableCollection<Question> Basket { get; } = new();

    public PaperCompositionViewModel()
    {
        WeakReferenceMessenger.Default.Register<AddToBasketMessage>(this);
    }

    public void Receive(AddToBasketMessage message)
    {
        if (message.Value == null) return;

        // Avoid duplicates by ID if > 0, otherwise by reference
        bool exists = message.Value.Id > 0
            ? Basket.Any(q => q.Id == message.Value.Id)
            : Basket.Contains(message.Value);

        if (!exists)
        {
            Basket.Add(message.Value);
        }
    }

    [RelayCommand]
    private void RemoveFromBasket(Question question)
    {
        if (Basket.Contains(question))
        {
            Basket.Remove(question);
        }
    }

    [RelayCommand]
    private void Export()
    {
        if (Basket.Count == 0) return;

        var html = GenerateHtml();
        WeakReferenceMessenger.Default.Send(new PrintHtmlMessage(html));
    }

    private string GenerateHtml()
    {
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html>");
        sb.AppendLine("<head>");
        sb.AppendLine("<meta charset='utf-8'/>");
        // Using MathJax for LaTeX rendering
        sb.AppendLine("<script src='https://polyfill.io/v3/polyfill.min.js?features=es6'></script>");
        sb.AppendLine("<script id='MathJax-script' async src='https://cdn.jsdelivr.net/npm/mathjax@3/es5/tex-mml-chtml.js'></script>");
        sb.AppendLine("<style>");
        sb.AppendLine("body { font-family: 'Segoe UI', sans-serif; padding: 20px; }");
        sb.AppendLine(".question { margin-bottom: 20px; padding: 10px; border-bottom: 1px solid #ccc; }");
        sb.AppendLine(".meta { font-size: 0.9em; color: #666; margin-top: 5px; }");
        sb.AppendLine("</style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        sb.AppendLine("<h1>Exam Paper</h1>");

        int index = 1;
        foreach (var q in Basket)
        {
            sb.AppendLine($"<div class='question'>");
            sb.AppendLine($"<p><strong>{index}.</strong> {q.Content}</p>");

            if (IsTeacherVersion)
            {
                sb.AppendLine("<div class='meta'>");
                sb.AppendLine($"<p><strong>Logic:</strong> {q.LogicDescriptor}</p>");
                sb.AppendLine($"<p><strong>Difficulty:</strong> {q.Difficulty}</p>");
                if (q.Tags != null)
                {
                    sb.AppendLine($"<p><strong>Tags:</strong> {string.Join(", ", q.Tags.Select(t => t.Name))}</p>");
                }
                sb.AppendLine("</div>");
            }

            sb.AppendLine("</div>");
            index++;
        }

        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }
}
