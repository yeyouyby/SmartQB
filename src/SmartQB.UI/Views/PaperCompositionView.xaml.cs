using System;
using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Win32;
using SmartQB.UI.Messages;

namespace SmartQB.UI.Views;

public partial class PaperCompositionView : UserControl, IRecipient<PrintHtmlMessage>
{
    public PaperCompositionView()
    {
        InitializeComponent();
        if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
        {
            WeakReferenceMessenger.Default.Register<PrintHtmlMessage>(this);
        }
    }

    public async void Receive(PrintHtmlMessage message)
    {
        try
        {
            await PreviewWebView.EnsureCoreWebView2Async();
            PreviewWebView.NavigateToString(message.Value);

            // Wait for navigation to complete before printing
            PreviewWebView.NavigationCompleted += OnNavigationCompleted;
        }
        catch (Exception ex)
        {
             MessageBox.Show($"WebView2 Initialization Failed: {ex.Message}\nMake sure WebView2 Runtime is installed.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void OnNavigationCompleted(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
    {
        PreviewWebView.NavigationCompleted -= OnNavigationCompleted;

        if (e.IsSuccess)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf",
                FileName = "SmartQB_Exam.pdf"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    await PreviewWebView.CoreWebView2.PrintToPdfAsync(dialog.FileName);
                    MessageBox.Show("Export Successful!", "SmartQB", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                     MessageBox.Show($"Export Failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
