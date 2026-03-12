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
    private string _statusMessage = "准备就绪";

    [ObservableProperty]
    private bool _isBusy;

    public event EventHandler? ImportCompleted;

    [RelayCommand]
    private async Task ProcessFileAsync(string filePath)
    {
        if (IsBusy) return;

        IsBusy = true;
        StatusMessage = $"正在处理 {System.IO.Path.GetFileName(filePath)}，请稍候...";

        try
        {
            await _ingestionService.ProcessPdfAsync(filePath);
            StatusMessage = "试卷导入并解析成功！";
            ImportCompleted?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Import failed for {FileName}", System.IO.Path.GetFileName(filePath));
            StatusMessage = "导入过程中发生错误，请检查日志。";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
