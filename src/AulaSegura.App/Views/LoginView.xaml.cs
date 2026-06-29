using System.Windows;
using System.Windows.Controls;
using AulaSegura.App.ViewModels;
using System.ComponentModel;

namespace AulaSegura.App.Views;

/// <summary>
/// Interaction logic for LoginView.xaml
/// </summary>
public partial class LoginView : Window
{
    private readonly LoginViewModel _viewModel;

    public LoginView(LoginViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        _viewModel.PropertyChanged += ViewModel_PropertyChanged;
        
        // Focus en el campo de usuario al iniciar
        Loaded += (s, e) => UsernameTextBox.Focus();
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (string.IsNullOrEmpty(_viewModel.Password) && PasswordBox.Password.Length > 0)
            PasswordBox.Clear();
    }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (_viewModel != null)
        {
            _viewModel.Password = ((PasswordBox)sender).Password;
        }
    }
}
