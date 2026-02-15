using System.Windows;
using System.Windows.Controls;
using SmartQB.UI.ViewModels;

namespace SmartQB.UI.Views;

public partial class LibraryView : UserControl
{
    public LibraryView()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is LibraryViewModel vm)
        {
            _ = vm.InitializeAsync();
        }
    }
}
