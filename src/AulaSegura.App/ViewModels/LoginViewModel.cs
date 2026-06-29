using System.Windows;
using System.Windows.Input;
using AulaSegura.Core.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AulaSegura.App.ViewModels;

/// <summary>
/// ViewModel para la pantalla de login.
/// </summary>
public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthService _authService;
    private readonly IServiceProvider _serviceProvider;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isPasswordVisible;

    public ICommand LoginCommand { get; }
    public ICommand ExitCommand { get; }

    public LoginViewModel(IAuthService authService, IServiceProvider serviceProvider)
    {
        _authService = authService;
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

            var admin = await _authService.LoginAsync(Username, Password);

            if (admin != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var loginWindow = Application.Current.Windows
                        .OfType<Window>()
                        .FirstOrDefault(w => w.DataContext is LoginViewModel);

                    var mainWindow = new MainWindow(admin, _serviceProvider);
                    Application.Current.MainWindow = mainWindow;
                    Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
                    mainWindow.Show();

                    loginWindow?.Close();
                });
            }
            else
            {
                ErrorMessage = "Usuario o contrasena incorrectos. Verifique sus credenciales.";
                Password = string.Empty;
            }
        }
        catch
        {
            ErrorMessage = "No se pudo iniciar sesion. Revise la configuracion y vuelva a intentarlo.";
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
