using System;
using System.IO;
using System.Text;
using System.Windows;
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

        _ = InitializeWebViewAsync();

        DataContextChanged += OnDataContextChanged;

        Loaded += (s, e) =>
        {
            if (DataContext is LibraryViewModel vm)
            {
                _ = vm.LoadQuestionsAsync();
            }
        };
    }

    private async System.Threading.Tasks.Task InitializeWebViewAsync()
    {
        var userDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SmartQB", "WebView2");
        var env = await CoreWebView2Environment.CreateAsync(null, userDataFolder);
        await DetailsWebView.EnsureCoreWebView2Async(env);
        _isWebViewInitialized = true;

        UpdateWebViewContent();
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is LibraryViewModel oldVm)
        {
            oldVm.PropertyChanged -= Vm_PropertyChanged;
        }

        if (e.NewValue is LibraryViewModel newVm)
        {
            newVm.PropertyChanged += Vm_PropertyChanged;
            UpdateWebViewContent();
        }
    }

    private void Vm_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(LibraryViewModel.SelectedQuestion))
        {
            UpdateWebViewContent();
        }
    }

    private void UpdateWebViewContent()
    {
        if (!_isWebViewInitialized) return;

        if (DataContext is LibraryViewModel vm && vm.SelectedQuestion != null)
        {
            var q = vm.SelectedQuestion;
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("<meta charset='utf-8'>");
            sb.AppendLine("<style>");
            sb.AppendLine("body { font-family: sans-serif; margin: 20px; color: #333; }");
            sb.AppendLine(".difficulty { font-weight: bold; color: #555; margin-bottom: 10px; }");
            sb.AppendLine(".content { margin-bottom: 20px; }");
            sb.AppendLine(".logic-title { font-weight: bold; margin-bottom: 5px; color: #4B5563; }");
            sb.AppendLine(".logic { color: #4B5563; padding: 10px; background-color: #F3F4F6; border-radius: 4px; }");
            sb.AppendLine("</style>");
            sb.AppendLine("<script src='https://polyfill.io/v3/polyfill.min.js?features=es6'></script>");
            sb.AppendLine("<script id='MathJax-script' async src='https://cdn.jsdelivr.net/npm/mathjax@3/es5/tex-mml-chtml.js'></script>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");

            sb.AppendLine($"<div class='difficulty'>Difficulty: {q.Difficulty}</div>");

            var safeContent = System.Net.WebUtility.HtmlEncode(q.Content ?? "").Replace("\n", "<br/>");
            sb.AppendLine($"<div class='content'>{safeContent}</div>");

            if (!string.IsNullOrWhiteSpace(q.LogicDescriptor))
            {
                var safeLogic = System.Net.WebUtility.HtmlEncode(q.LogicDescriptor).Replace("\n", "<br/>");
                sb.AppendLine("<div class='logic-title'>Logic Path:</div>");
                sb.AppendLine($"<div class='logic'>{safeLogic}</div>");
            }

            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            DetailsWebView.NavigateToString(sb.ToString());
        }
        else
        {
            DetailsWebView.NavigateToString("<html><body style='font-family:sans-serif;color:#999;text-align:center;margin-top:50px;'>Select a question to view details.</body></html>");
        }
    }
}