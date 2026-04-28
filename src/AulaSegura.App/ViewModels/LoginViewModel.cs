using System.Windows;
using System.Windows.Input;
using AulaSegura.Core.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AulaSegura.App.ViewModels;

/// <summary>
/// ViewModel para la pantalla de login
/// </summary>
public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthService _authService;
    private readonly IActivityLogService _activityLogService;
    private readonly IServiceProvider _serviceProvider;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isLoading = false;

    [ObservableProperty]
    private bool _isPasswordVisible = false;

    public ICommand LoginCommand { get; }
    public ICommand ExitCommand { get; }

    public LoginViewModel(IAuthService authService, IActivityLogService activityLogService, IServiceProvider serviceProvider)
    {
        _authService = authService;
        _activityLogService = activityLogService;
        _serviceProvider = serviceProvider;
        
        LoginCommand = new AsyncRelayCommand(ExecuteLoginAsync, CanExecuteLogin);
        ExitCommand = new RelayCommand(ExecuteExit);
    }

    private bool CanExecuteLogin()
    {
        return !string.IsNullOrWhiteSpace(Username) && 
               !string.IsNullOrWhiteSpace(Password) && 
               !IsLoading;
    }

    private async Task ExecuteLoginAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            // Intentar autenticación
            var admin = await _authService.LoginAsync(Username, Password);

            if (admin != null)
            {
                // Login exitoso - cerrar ventana de login y abrir dashboard
                Application.Current.Dispatcher.Invoke(() =>
                {
                    // Find and close the login window specifically
                    var loginWindow = Application.Current.Windows
                        .OfType<Window>()
                        .FirstOrDefault(w => w.DataContext is LoginViewModel);
                    
                    // Create and show main window
                    var mainWindow = new MainWindow(admin, _serviceProvider);
                    mainWindow.Show();
                    
                    // Close login window if found
                    if (loginWindow != null)
                    {
                        loginWindow.Close();
                    }
                });
            }
            else
            {
                // Login fallido
                ErrorMessage = "Usuario o contraseña incorrectos. Verifique sus credenciales.";
                
                // Limpiar contraseña
                Password = string.Empty;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error de autenticación: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ExecuteExit()
    {
        Application.Current.Shutdown();
    }

    partial void OnUsernameChanged(string value)
    {
        ((AsyncRelayCommand)LoginCommand).NotifyCanExecuteChanged();
    }

    partial void OnPasswordChanged(string value)
    {
        ((AsyncRelayCommand)LoginCommand).NotifyCanExecuteChanged();
    }
}
