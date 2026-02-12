using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SmartQB.Core.Interfaces;
using System;
using System.Threading.Tasks;

namespace SmartQB.UI.ViewModels;

public partial class ImportViewModel : ObservableObject
{
    private readonly IIngestionService _ingestionService;

    [ObservableProperty]
    private string _statusMessage = "Ready to import";

    [ObservableProperty]
    private bool _isBusy;

    public ImportViewModel(IIngestionService ingestionService)
    {
        _ingestionService = ingestionService;
    }

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
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
