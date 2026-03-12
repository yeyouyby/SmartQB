using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartQB.Core.Interfaces;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SmartQB.UI.ViewModels;

public partial class ImportViewModel(IIngestionService ingestionService, ILogger<ImportViewModel> logger) : ObservableObject
{
    private readonly IIngestionService _ingestionService = ingestionService;
    private readonly ILogger<ImportViewModel> _logger = logger;

    [ObservableProperty]
    private string _statusMessage = "Ready to import";

    [ObservableProperty]
    private bool _isBusy;

    public event EventHandler? ImportCompleted;


    [RelayCommand]
    private async Task ProcessFileAsync(string filePath)
    {
        if (IsBusy) return;

        IsBusy = true;
        StatusMessage = $"Processing {System.IO.Path.GetFileName(filePath)}...";

        try
        {
            await _ingestionService.ProcessPdfAsync(filePath);
            StatusMessage = "Import completed successfully!";
            ImportCompleted?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Import failed for {FileName}", System.IO.Path.GetFileName(filePath));
            StatusMessage = "An error occurred during import.";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
