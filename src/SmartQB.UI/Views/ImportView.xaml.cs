using System.Windows;
using System.Windows.Controls;
using SmartQB.UI.ViewModels;

namespace SmartQB.UI.Views;

public partial class ImportView : UserControl
{
    public ImportView()
    {
        InitializeComponent();
    }

    private void Border_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files != null && files.Length > 0)
            {
                var vm = DataContext as ImportViewModel;
                if (vm != null)
                {
                    vm.ProcessFileCommand.Execute(files[0]);
                }
            }
        }
    }
}
