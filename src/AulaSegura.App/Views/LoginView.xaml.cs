using System.Windows;
using System.Windows.Controls;
using AulaSegura.App.ViewModels;

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
        
        // Focus en el campo de usuario al iniciar
        Loaded += (s, e) => UsernameTextBox.Focus();
    }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (_viewModel != null)
        {
            _viewModel.Password = ((PasswordBox)sender).Password;
        }
    }
}
