using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Microsoft.Web.WebView2.Core;
using SmartQB.UI.ViewModels;

namespace SmartQB.UI.Views;

public partial class LibraryView : UserControl
{
    private bool _isWebViewInitialized;

    public LibraryView()
    {
        InitializeComponent();
        _ = InitializeAsync();

        Loaded += (s, e) =>
        {
            if (DataContext is LibraryViewModel vm)
            {
                _ = vm.LoadQuestionsAsync();
                vm.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == nameof(vm.SelectedQuestion))
                    {
                        UpdateWebView(vm.SelectedQuestion);
                    }
                };
            }
        };
    }

    private async Task InitializeAsync()
    {
        string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string userDataFolder = Path.Combine(appDataPath, "SmartQB", "WebView2");
        var env = await CoreWebView2Environment.CreateAsync(null, userDataFolder);
        await DetailsWebView.EnsureCoreWebView2Async(env);
        _isWebViewInitialized = true;

        if (DataContext is LibraryViewModel vm && vm.SelectedQuestion != null)
        {
            UpdateWebView(vm.SelectedQuestion);
        }
    }

    private void UpdateWebView(Core.Entities.Question? question)
    {
        if (!_isWebViewInitialized) return;

        if (question == null)
        {
            DetailsWebView.NavigateToString("<html><body><p>Select a question to view details.</p></body></html>");
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html>");
        sb.AppendLine("<head>");
        sb.AppendLine("<meta charset='utf-8'>");
        sb.AppendLine("<style>");
        sb.AppendLine("body { font-family: sans-serif; padding: 20px; }");
        sb.AppendLine(".difficulty { color: #666; font-size: 0.9em; margin-bottom: 20px; }");
        sb.AppendLine(".logic { margin-top: 20px; padding-top: 20px; border-top: 1px solid #ccc; }");
        sb.AppendLine(".logic-title { font-weight: bold; margin-bottom: 10px; }");
        sb.AppendLine("</style>");
        sb.AppendLine("<script src='https://polyfill.io/v3/polyfill.min.js?features=es6'></script>");
        sb.AppendLine("<script id='MathJax-script' async src='https://cdn.jsdelivr.net/npm/mathjax@3/es5/tex-mml-chtml.js'></script>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");

        var safeContent = System.Net.WebUtility.HtmlEncode(question.Content).Replace("\n", "<br/>");
        sb.AppendLine($"<div class='content'>{safeContent}</div>");
        sb.AppendLine($"<div class='difficulty'>Difficulty: {question.Difficulty}</div>");

        if (!string.IsNullOrWhiteSpace(question.LogicDescriptor))
        {
            var safeLogic = System.Net.WebUtility.HtmlEncode(question.LogicDescriptor).Replace("\n", "<br/>");
            sb.AppendLine("<div class='logic'>");
            sb.AppendLine("<div class='logic-title'>Logic Path:</div>");
            sb.AppendLine($"<div>{safeLogic}</div>");
            sb.AppendLine("</div>");
        }

        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        DetailsWebView.NavigateToString(sb.ToString());
    }
}