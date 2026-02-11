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
                if (DataContext is IngestionViewModel viewModel)
                {
                    if (viewModel.ProcessFilesCommand.CanExecute(files))
                    {
                        viewModel.ProcessFilesCommand.Execute(files);
                    }
                }
            }
        }
    }
}
