using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using SmartQB.Core.Interfaces;
using System.Text;
using System;

namespace SmartQB.UI.ViewModels;

/// <summary>
/// ViewModel for the Export view, handling the generation of HTML papers from the question bank.
/// </summary>
public partial class ExportViewModel(IQuestionService questionService) : ObservableObject
{
    private readonly IQuestionService _questionService = questionService;

    [ObservableProperty]
    private string _status = "Ready to export";

    /// <summary>
    /// Generates the HTML representation of the question paper, optionally including logic descriptors as answers.
    /// This output is intended to be rendered by WebView2 and exported as a PDF.
    /// </summary>
    /// <param name="includeAnswers">If true, includes the answers/logic paths in the generated HTML.</param>
    /// <returns>A string containing the formatted HTML paper with MathJax enabled for LaTeX.</returns>
    public async Task<string> GenerateHtmlAsync(bool includeAnswers)
    {
        var questions = await _questionService.GetQuestionsAsync();

        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang='en'>");
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
        sb.AppendLine("<main>");
        sb.AppendLine("<h1>Generated Paper</h1>");

        int qIndex = 1;
        foreach (var q in questions)
        {
            sb.AppendLine("<div class='question'>");
            sb.AppendLine($"<h3>Question {qIndex} (Difficulty: {q.Difficulty.ToString(System.Globalization.CultureInfo.InvariantCulture)})</h3>");
            // Do not HtmlEncode the content to let MathJax parse LaTeX ($...$)
            var content = q.Content?.Replace("\n", "<br/>") ?? "";
            sb.AppendLine($"<p>{content}</p>");

            if (includeAnswers)
            {
                var answer = q.LogicDescriptor?.Replace("\n", "<br/>") ?? "No logic provided.";
                sb.AppendLine("<div class='answer'>");
                sb.AppendLine("<strong>Logic/Answer:</strong><br/>");
                sb.AppendLine($"<p>{answer}</p>");
                sb.AppendLine("</div>");
            }
            sb.AppendLine("</div>");
            qIndex++;
        }

        sb.AppendLine("</main>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }
}
