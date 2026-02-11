using System.Windows;
using System.Windows.Controls;
using SmartQB.UI.ViewModels;

namespace SmartQB.UI.Views;

public partial class IngestionView : UserControl
{
    public IngestionView()
    {
        InitializeComponent();
    }

    private void OnDrop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files != null && files.Length > 0)
            {
                var viewModel = (IngestionViewModel)DataContext;
                // Assuming ViewModel has a ProcessFilesCommand
                // We need to pass files.
                // Since RelayCommand usually takes parameter, we can execute directly.
                // But RelayCommand<string[]> requires strict type.
                // Let's call method or command.

                // Using dynamic dispatch or reflection is one way if command is untyped in XAML binding,
                // but here we have typed DataContext.

                // But wait, ProcessFilesCommand is IAsyncRelayCommand<string[]> or similar?
                // The generated code for [RelayCommand] creates ProcessFilesCommand.

                if (viewModel.ProcessFilesCommand.CanExecute(files))
                {
                    viewModel.ProcessFilesCommand.Execute(files);
                }
            }
        }
    }
}
