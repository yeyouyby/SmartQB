using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using SmartQB.Core.Interfaces;
using System;
using System.Threading.Tasks;

namespace SmartQB.UI.ViewModels;

public partial class ImportViewModel(IIngestionService ingestionService, ILogger<ImportViewModel> logger) : ObservableObject
{
    private readonly IIngestionService _ingestionService = ingestionService;
    private readonly ILogger<ImportViewModel> _logger = logger;

    [ObservableProperty]
    private string _statusMessage = "Ready to import";

    [ObservableProperty]
    private bool _isBusy;

    [RelayCommand]
    private async Task ProcessFileAsync(string filePath)
    {
        if (IsBusy) return;

        IsBusy = true;
        StatusMessage = $"Processing {System.IO.Path.GetFileName(filePath)}...";

        try
        {
            // Removed Task.Run since ProcessPdfAsync is already async and IO-bound
            await _ingestionService.ProcessPdfAsync(filePath);
            StatusMessage = "Import completed successfully!";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing PDF file: {FilePath}", filePath);
            StatusMessage = "An error occurred during import. Please check logs for details.";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
