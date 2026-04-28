using System.Windows;
using System.Windows.Input;
using AulaSegura.Core.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AulaSegura.App.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly IAuthService _authService;
    private readonly ISettingsService _settingsService;
    private MainWindow? _mainWindow;

    /// <summary>
    /// Sets the parent MainWindow reference for navigation
    /// </summary>
    public void SetParentWindow(MainWindow mainWindow)
    {
        _mainWindow = mainWindow;
    }

    [ObservableProperty]
    private string _institutionName = "AulaSegura";

    [ObservableProperty]
    private string _currentPassword = string.Empty;

    [ObservableProperty]
    private string _newPassword = string.Empty;

    [ObservableProperty]
    private string _confirmPassword = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private string _successMessage = string.Empty;

    public ICommand SaveSettingsCommand { get; }
    public ICommand ChangePasswordCommand { get; }
    public ICommand GoBackCommand { get; }

    public SettingsViewModel(IAuthService authService, ISettingsService settingsService)
    {
        _authService = authService;
        _settingsService = settingsService;
        SaveSettingsCommand = new AsyncRelayCommand(SaveSettingsAsync);
        ChangePasswordCommand = new AsyncRelayCommand(ChangePasswordAsync, CanChangePassword);
        GoBackCommand = new RelayCommand(GoBack);
    }

    private async Task SaveSettingsAsync()
    {
        try
        {
            // Save institution name setting
            await _settingsService.SetSettingAsync("InstitutionName", InstitutionName);
            SuccessMessage = "Configuración guardada correctamente.";
            ErrorMessage = string.Empty;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al guardar: {ex.Message}";
            SuccessMessage = string.Empty;
        }
    }

    private async Task ChangePasswordAsync()
    {
        try
        {
            if (NewPassword != ConfirmPassword)
            {
                ErrorMessage = "Las contraseñas no coinciden.";
                return;
            }

            if (NewPassword.Length < 6)
            {
                ErrorMessage = "La contraseña debe tener al menos 6 caracteres.";
                return;
            }

            // TODO: Get current admin ID from session
            await _authService.ChangePasswordAsync(1, CurrentPassword, NewPassword);
            
            SuccessMessage = "Contraseña cambiada correctamente.";
            ErrorMessage = string.Empty;
            CurrentPassword = string.Empty;
            NewPassword = string.Empty;
            ConfirmPassword = string.Empty;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error: {ex.Message}";
            SuccessMessage = string.Empty;
        }
    }

    private bool CanChangePassword() => 
        !string.IsNullOrWhiteSpace(CurrentPassword) && 
        !string.IsNullOrWhiteSpace(NewPassword) && 
        !string.IsNullOrWhiteSpace(ConfirmPassword);

    partial void OnCurrentPasswordChanged(string value) => ((AsyncRelayCommand)ChangePasswordCommand).NotifyCanExecuteChanged();
    partial void OnNewPasswordChanged(string value) => ((AsyncRelayCommand)ChangePasswordCommand).NotifyCanExecuteChanged();
    partial void OnConfirmPasswordChanged(string value) => ((AsyncRelayCommand)ChangePasswordCommand).NotifyCanExecuteChanged();

    private void GoBack()
    {
        _mainWindow?.NavigateToDashboard();
    }
}
