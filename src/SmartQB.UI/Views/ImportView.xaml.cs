using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
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
                if (vm != null && vm.ProcessFileCommand.CanExecute(files[0]))
                {
                    vm.ProcessFileCommand.Execute(files[0]);
                }
            }
        }
    }

    private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "PDF Files (*.pdf)|*.pdf|All Files (*.*)|*.*",
            Title = "选择要导入的 PDF 试卷"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            var vm = DataContext as ImportViewModel;
            if (vm != null && vm.ProcessFileCommand.CanExecute(openFileDialog.FileName))
            {
                vm.ProcessFileCommand.Execute(openFileDialog.FileName);
            }
        }
    }
}
