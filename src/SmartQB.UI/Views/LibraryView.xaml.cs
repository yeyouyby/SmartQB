using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Microsoft.Web.WebView2.Core;
using SmartQB.UI.ViewModels;
using SmartQB.UI.Helpers;
using System.ComponentModel;

namespace SmartQB.UI.Views;

public partial class LibraryView : UserControl
{
    private bool _isWebViewInitialized;

    public LibraryView()
    {
        InitializeComponent();

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private async void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
    {
        await InitializeAsync();
        if (DataContext is LibraryViewModel vm)
        {
            _ = vm.LoadQuestionsAsync();
            vm.PropertyChanged += OnViewModelPropertyChanged;
        }
    }

    private void OnUnloaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is LibraryViewModel vm)
        {
            vm.PropertyChanged -= OnViewModelPropertyChanged;
        }
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName == nameof(LibraryViewModel.SelectedQuestion) && DataContext is LibraryViewModel vm)
        {
            UpdateWebView(vm.SelectedQuestion);
        }
    }

    private async Task InitializeAsync()
    {
        if (_isWebViewInitialized) return;

        try
        {
            var env = await WebView2Helper.GetEnvironmentAsync();
            await DetailsWebView.EnsureCoreWebView2Async(env);
            _isWebViewInitialized = true;

            if (DataContext is LibraryViewModel vm && vm.SelectedQuestion != null)
            {
                UpdateWebView(vm.SelectedQuestion);
            }
        }
        catch
        {
            // Initialization failed, safe to ignore for now as UpdateWebView checks _isWebViewInitialized
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
        sb.AppendLine("<html lang='en'>");
        sb.AppendLine("<head>");
        sb.AppendLine("<meta charset='utf-8'>");
        sb.AppendLine("<title>Question Details</title>");
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
        sb.AppendLine("<main>");
        sb.AppendLine("<h1>Question Details</h1>");

        // Do not HtmlEncode to allow LaTeX parsing by MathJax
        var content = question.Content?.Replace("\n", "<br/>") ?? "";
        sb.AppendLine($"<div class='content'>{content}</div>");
        sb.AppendLine($"<div class='difficulty'>Difficulty: {question.Difficulty.ToString(System.Globalization.CultureInfo.InvariantCulture)}</div>");

        if (!string.IsNullOrWhiteSpace(question.LogicDescriptor))
        {
            var logic = question.LogicDescriptor.Replace("\n", "<br/>");
            sb.AppendLine("<div class='logic'>");
            sb.AppendLine("<div class='logic-title'>Logic Path:</div>");
            sb.AppendLine($"<div>{logic}</div>");
            sb.AppendLine("</div>");
        }

        sb.AppendLine("</main>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        DetailsWebView.NavigateToString(sb.ToString());
    }
}