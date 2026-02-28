using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using SmartQB.Core.Interfaces;
using System.Text;
using System;

namespace SmartQB.UI.ViewModels;

public partial class ExportViewModel(IQuestionService questionService) : ObservableObject
{
    private readonly IQuestionService _questionService = questionService;

    [ObservableProperty]
    private string _status = "Ready to export";

    // A real implementation would allow users to select specific questions.
    // For now, we will just export all questions in a simple HTML template.
    public async Task<string> GenerateHtmlAsync(bool includeAnswers)
    {
        var questions = await _questionService.GetAllQuestionsAsync();

        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html>");
        sb.AppendLine("<head>");
        sb.AppendLine("<meta charset='utf-8'>");
        sb.AppendLine("<title>SmartQB Paper</title>");
        // Basic styling
        sb.AppendLine("<style>");
        sb.AppendLine("body { font-family: sans-serif; margin: 40px; }");
        sb.AppendLine(".question { margin-bottom: 20px; padding: 10px; border: 1px solid #ccc; }");
        sb.AppendLine(".answer { margin-top: 10px; color: #555; background: #f9f9f9; padding: 10px; }");
        sb.AppendLine("</style>");
        // Add MathJax for LaTeX support
        sb.AppendLine("<script src='https://polyfill.io/v3/polyfill.min.js?features=es6'></script>");
        sb.AppendLine("<script id='MathJax-script' async src='https://cdn.jsdelivr.net/npm/mathjax@3/es5/tex-mml-chtml.js'></script>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        sb.AppendLine("<h1>Generated Paper</h1>");

        int qIndex = 1;
        foreach (var q in questions)
        {
            sb.AppendLine("<div class='question'>");
            sb.AppendLine($"<h3>Question {qIndex} (Difficulty: {q.Difficulty})</h3>");
            // HTML encode the content but let MathJax parse LaTeX ($...$)
            var safeContent = System.Net.WebUtility.HtmlEncode(q.Content).Replace("\n", "<br/>");
            sb.AppendLine($"<p>{safeContent}</p>");

            if (includeAnswers)
            {
                var safeAnswer = System.Net.WebUtility.HtmlEncode(q.LogicDescriptor ?? "No logic provided.").Replace("\n", "<br/>");
                sb.AppendLine("<div class='answer'>");
                sb.AppendLine("<strong>Logic/Answer:</strong><br/>");
                sb.AppendLine($"<p>{safeAnswer}</p>");
                sb.AppendLine("</div>");
            }
            sb.AppendLine("</div>");
            qIndex++;
        }

        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }
}