using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Web.WebView2.Core;
using SmartQB.UI.ViewModels;
using SmartQB.UI.Helpers;
using Microsoft.Extensions.Logging;

namespace SmartQB.UI.Views;

public partial class ExportView : UserControl
{
    private bool _isWebViewInitialized;
    private TaskCompletionSource<bool>? _navigationTcs;
    private EventHandler<CoreWebView2NavigationCompletedEventArgs>? _navigationHandler;
    private bool _isExporting;
    private bool _isLoaded;

    public ExportView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;

        _navigationHandler = (sender, args) =>
        {
            if (_navigationTcs != null && !_navigationTcs.Task.IsCompleted)
            {
                _navigationTcs.TrySetResult(args.IsSuccess);
            }
        };
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        _isLoaded = true;
        await InitializeAsync();

        if (_isLoaded && _isWebViewInitialized && _navigationHandler != null)
        {
            WebView.NavigationCompleted -= _navigationHandler;
            WebView.NavigationCompleted += _navigationHandler;
        }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _isLoaded = false;
        if (_isWebViewInitialized && _navigationHandler != null)
        {
            WebView.NavigationCompleted -= _navigationHandler;
        }

        // Failsafe: if the view is unloaded during an export, cancel the pending wait
        if (_navigationTcs != null && !_navigationTcs.Task.IsCompleted)
        {
            _navigationTcs.TrySetCanceled();
        }
    }

    private async Task InitializeAsync()
    {
        if (_isWebViewInitialized) return;

        try
        {
            var env = await WebView2Helper.GetEnvironmentAsync();
            await WebView.EnsureCoreWebView2Async(env);
            _isWebViewInitialized = true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"WebView2 init failed: {ex}");
        }
    }

    private async void Preview_Click(object sender, RoutedEventArgs e)
    {
        if (!_isWebViewInitialized) return;
        if (DataContext is ExportViewModel vm)
        {
            vm.Status = "Generating Preview...";
            try
            {
                string html = await vm.GenerateHtmlAsync(IncludeAnswersCheck.IsChecked == true);
                WebView.NavigateToString(html);
                vm.Status = "Preview Ready";
            }
            catch (Exception ex)
            {
                vm.Status = "Error generating preview.";
                System.Diagnostics.Debug.WriteLine($"Preview error: {ex}");
            }
        }
    }

    private async void Export_Click(object sender, RoutedEventArgs e)
    {
        if (!_isWebViewInitialized || _isExporting) return;

        _isExporting = true;
        try
        {
            if (DataContext is ExportViewModel vm)
            {
                vm.Status = "Generating HTML...";
                string html = await vm.GenerateHtmlAsync(IncludeAnswersCheck.IsChecked == true);

                _navigationTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                WebView.NavigateToString(html);

                vm.Status = "Waiting for render...";

                bool navigationSuccess;
                try
                {
                    navigationSuccess = await _navigationTcs.Task;
                }
                catch (TaskCanceledException)
                {
                    vm.Status = "Export cancelled (View Unloaded).";
                    return;
                }

                if (!navigationSuccess)
                {
                    vm.Status = "Failed to render paper.";
                    return;
                }

                await Task.Delay(500);

                vm.Status = "Printing to PDF...";

                string downloadsPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                string desktopPath = Path.Combine(downloadsPath, "Desktop");
                if (!Directory.Exists(desktopPath)) desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                string filePath = Path.Combine(desktopPath, $"SmartQB_Paper_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");

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
        }
        catch (Exception ex)
        {
            if (DataContext is ExportViewModel vm)
            {
                vm.Status = "An error occurred during export.";
            }
            System.Diagnostics.Debug.WriteLine($"Export error: {ex}");
        }
        finally
        {
            _isExporting = false;
        }
    }
}
