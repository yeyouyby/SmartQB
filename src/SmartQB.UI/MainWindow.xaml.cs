using System.Windows;
using SmartQB.UI.ViewModels;

namespace SmartQB.UI;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
