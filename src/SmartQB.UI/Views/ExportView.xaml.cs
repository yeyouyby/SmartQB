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
        await InitializeAsync();

        if (_isWebViewInitialized && _navigationHandler != null)
        {
            WebView.NavigationCompleted -= _navigationHandler;
            WebView.NavigationCompleted += _navigationHandler;
        }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
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

            WebView.NavigationCompleted += _navigationHandler;
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
            vm.Status = "正在生成预览...";
            try
            {
                string html = await vm.GenerateHtmlAsync(IncludeAnswersCheck.IsChecked == true);

                _navigationTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                WebView.NavigateToString(html);

                bool success = await _navigationTcs.Task;
                if (success)
                {
                    vm.Status = "预览准备就绪";
                }
                else
                {
                    vm.Status = "生成预览时出错。";
                }
            }
            catch (Exception ex)
            {
                vm.Status = "生成预览时出错。";
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
                vm.Status = "正在生成 HTML...";
                string html = await vm.GenerateHtmlAsync(IncludeAnswersCheck.IsChecked == true);

                _navigationTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                WebView.NavigateToString(html);

                vm.Status = "等待渲染完成...";

                bool navigationSuccess;
                try
                {
                    navigationSuccess = await _navigationTcs.Task;
                }
                catch (TaskCanceledException)
                {
                    vm.Status = "导出已取消（视图已卸载）。";
                    return;
                }

                if (!navigationSuccess)
                {
                    vm.Status = "渲染试卷失败。";
                    return;
                }

                await Task.Delay(500);

                vm.Status = "正在输出 PDF...";

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
                    vm.Status = $"已保存至 {filePath}";
                }
                else
                {
                    vm.Status = "导出 PDF 失败。";
                }
            }
        }
        catch (Exception ex)
        {
            if (DataContext is ExportViewModel vm)
            {
                vm.Status = "导出过程中发生错误。";
            }
            System.Diagnostics.Debug.WriteLine($"Export error: {ex}");
        }
        finally
        {
            _isExporting = false;
        }
    }
}
