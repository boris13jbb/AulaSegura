using System.Windows;
using System.Windows.Controls;
using AulaSegura.App.ViewModels;
using System.ComponentModel;

namespace AulaSegura.App.Views;

public partial class SettingsView : UserControl
{
    public SettingsViewModel? ViewModel => DataContext as SettingsViewModel;
    private SettingsViewModel? _subscribedViewModel;

    public SettingsView()
    {
        InitializeComponent();
        DataContextChanged += SettingsView_DataContextChanged;
    }

    private void SettingsView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (_subscribedViewModel != null)
            _subscribedViewModel.PropertyChanged -= ViewModel_PropertyChanged;

        _subscribedViewModel = e.NewValue as SettingsViewModel;

        if (_subscribedViewModel != null)
            _subscribedViewModel.PropertyChanged += ViewModel_PropertyChanged;
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (ViewModel == null)
            return;

        if (string.IsNullOrEmpty(ViewModel.CurrentPassword) && CurrentPasswordBox.Password.Length > 0)
            CurrentPasswordBox.Clear();

        if (string.IsNullOrEmpty(ViewModel.NewPassword) && NewPasswordBox.Password.Length > 0)
            NewPasswordBox.Clear();

        if (string.IsNullOrEmpty(ViewModel.ConfirmPassword) && ConfirmPasswordBox.Password.Length > 0)
            ConfirmPasswordBox.Clear();
    }

    private void CurrentPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (ViewModel != null)
            ViewModel.CurrentPassword = CurrentPasswordBox.Password;
    }

    private void NewPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (ViewModel != null)
            ViewModel.NewPassword = NewPasswordBox.Password;
    }

    private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (ViewModel != null)
            ViewModel.ConfirmPassword = ConfirmPasswordBox.Password;
    }
}
