using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Web.WebView2.Core;
using SmartQB.UI.ViewModels;

namespace SmartQB.UI.Views;

public partial class ExportView : UserControl
{
    private bool _isWebViewInitialized;
    private TaskCompletionSource<bool>? _navigationTcs;

    public ExportView()
    {
        InitializeComponent();
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        var userDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SmartQB", "WebView2");
        var env = await CoreWebView2Environment.CreateAsync(null, userDataFolder);
        await WebView.EnsureCoreWebView2Async(env);
        _isWebViewInitialized = true;

        WebView.NavigationCompleted += (sender, args) =>
        {
            if (_navigationTcs != null && !_navigationTcs.Task.IsCompleted)
            {
                _navigationTcs.SetResult(args.IsSuccess);
            }
        };
    }

    private async void Preview_Click(object sender, RoutedEventArgs e)
    {
        if (!_isWebViewInitialized) return;
        if (DataContext is ExportViewModel vm)
        {
            vm.Status = "Generating Preview...";
            string html = await vm.GenerateHtmlAsync(IncludeAnswersCheck.IsChecked == true);
            WebView.NavigateToString(html);
            vm.Status = "Preview Ready";
        }
    }

    private async void Export_Click(object sender, RoutedEventArgs e)
    {
        if (!_isWebViewInitialized) return;

        if (DataContext is ExportViewModel vm)
        {
            vm.Status = "Generating HTML...";
            string html = await vm.GenerateHtmlAsync(IncludeAnswersCheck.IsChecked == true);

            _navigationTcs = new TaskCompletionSource<bool>();
            WebView.NavigateToString(html);

            vm.Status = "Waiting for render...";
            bool navigationSuccess = await _navigationTcs.Task;

            // Give MathJax a tiny bit of time to render the equations after navigation finishes
            await Task.Delay(500);

            vm.Status = "Printing to PDF...";

            string downloadsPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string desktopPath = Path.Combine(downloadsPath, "Desktop");
            if (!Directory.Exists(desktopPath)) desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            string filePath = Path.Combine(desktopPath, $"SmartQB_Paper_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");

            try
            {
                var printSettings = WebView.CoreWebView2.Environment.CreatePrintSettings();
                printSettings.ShouldPrintBackgrounds = true;
                printSettings.ShouldPrintSelectionOnly = false;

                bool success = await WebView.CoreWebView2.PrintToPdfAsync(filePath, printSettings);

                if (success)
                {
                    vm.Status = $"Saved to {filePath}";
                }
                else
                {
                    vm.Status = "Failed to export PDF.";
                }
            }
            catch (Exception ex)
            {
                vm.Status = $"Error: {ex.Message}";
            }
        }
    }
}