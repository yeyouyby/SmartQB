using System.Windows.Controls;
using System.Windows;
using SmartQB.UI.ViewModels;
using System.ComponentModel;

namespace SmartQB.UI.Views;

public partial class SettingsView : UserControl
{
    private bool _isSyncing;

    public SettingsView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is SettingsViewModel oldVm)
        {
            oldVm.PropertyChanged -= Vm_PropertyChanged;
        }

        if (e.NewValue is SettingsViewModel newVm)
        {
            newVm.PropertyChanged += Vm_PropertyChanged;
            SyncPasswordBox(newVm.ApiKey);
        }
    }

    private void Vm_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SettingsViewModel.ApiKey))
        {
            if (DataContext is SettingsViewModel vm)
            {
                SyncPasswordBox(vm.ApiKey);
            }
        }
    }

    private void SyncPasswordBox(string apiKey)
    {
        if (_isSyncing) return;
        _isSyncing = true;
        ApiKeyPasswordBox.Password = apiKey ?? string.Empty;
        _isSyncing = false;
    }

    private void ApiKeyPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (_isSyncing) return;

        if (DataContext is SettingsViewModel vm)
        {
            _isSyncing = true;
            vm.ApiKey = ApiKeyPasswordBox.Password;
            _isSyncing = false;
        }
    }
}
