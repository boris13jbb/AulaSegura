using System.Windows;
using System.Windows.Controls;
using AulaSegura.App.ViewModels;

namespace AulaSegura.App.Views;

public partial class SettingsView : UserControl
{
    public SettingsViewModel? ViewModel => DataContext as SettingsViewModel;

    public SettingsView()
    {
        InitializeComponent();
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
