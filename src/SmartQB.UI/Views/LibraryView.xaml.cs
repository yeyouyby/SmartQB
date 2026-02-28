using System.Windows.Controls;
using SmartQB.UI.ViewModels;

namespace SmartQB.UI.Views;

public partial class LibraryView : UserControl
{
    public LibraryView()
    {
        InitializeComponent();
        Loaded += (s, e) =>
        {
            if (DataContext is LibraryViewModel vm)
            {
                _ = vm.LoadQuestionsAsync();
            }
        };
    }
}