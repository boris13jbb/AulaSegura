using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using AulaSegura.Core.Constants;
using AulaSegura.Core.Entities;
using AulaSegura.Core.Interfaces;
using AulaSegura.Core.Utilities;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AulaSegura.App.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly IAuthService _authService;
    private readonly ISettingsService _settingsService;
    private readonly IBackupService _backupService;
    private readonly IBlockedSiteService _blockedSiteService;
    private MainWindow? _mainWindow;
    private int _currentAdminId;

    public void SetParentWindow(MainWindow mainWindow)
    {
        _mainWindow = mainWindow;
    }

    public void SetCurrentAdminId(int adminId)
    {
        _currentAdminId = adminId;
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
    private string _backupDescription = "Respaldo manual";

    [ObservableProperty]
    private ObservableCollection<Backup> _backups = new();

    [ObservableProperty]
    private Backup? _selectedBackup;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private string _successMessage = string.Empty;

    public ICommand SaveSettingsCommand { get; }
    public ICommand ChangePasswordCommand { get; }
    public ICommand LoadBackupsCommand { get; }
    public ICommand CreateBackupCommand { get; }
    public ICommand RestoreBackupCommand { get; }
    public ICommand DeleteBackupCommand { get; }
    public ICommand GoBackCommand { get; }

    public SettingsViewModel(
        IAuthService authService,
        ISettingsService settingsService,
        IBackupService backupService,
        IBlockedSiteService blockedSiteService)
    {
        _authService = authService;
        _settingsService = settingsService;
        _backupService = backupService;
        _blockedSiteService = blockedSiteService;

        SaveSettingsCommand = new AsyncRelayCommand(SaveSettingsAsync);
        ChangePasswordCommand = new AsyncRelayCommand(ChangePasswordAsync, CanChangePassword);
        LoadBackupsCommand = new AsyncRelayCommand(LoadBackupsAsync);
        CreateBackupCommand = new AsyncRelayCommand(CreateBackupAsync);
        RestoreBackupCommand = new AsyncRelayCommand(RestoreBackupAsync, () => SelectedBackup != null && !IsBusy);
        DeleteBackupCommand = new AsyncRelayCommand(DeleteBackupAsync, () => SelectedBackup != null && !IsBusy);
        GoBackCommand = new RelayCommand(GoBack);

        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        await LoadSettingsAsync();
        await LoadBackupsAsync();
    }

    private async Task LoadSettingsAsync()
    {
        var institutionName = await _settingsService.GetSettingAsync(SystemConstants.SettingsKeys.InstitutionName);
        if (!string.IsNullOrWhiteSpace(institutionName))
            InstitutionName = institutionName;
    }

    private async Task SaveSettingsAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(InstitutionName))
            {
                SetError("El nombre de la institucion es obligatorio.");
                return;
            }

            await _settingsService.SetSettingAsync(SystemConstants.SettingsKeys.InstitutionName, InstitutionName.Trim());
            SetSuccess("Configuracion guardada correctamente.");
        }
        catch
        {
            SetError("No se pudo guardar la configuracion. Revise los permisos y vuelva a intentarlo.");
        }
    }

    private async Task ChangePasswordAsync()
    {
        try
        {
            SuccessMessage = string.Empty;

            if (_currentAdminId <= 0)
            {
                SetError("No se encontro la sesion del administrador actual.");
                return;
            }

            if (NewPassword != ConfirmPassword)
            {
                SetError("Las contrasenas no coinciden.");
                return;
            }

            var validation = ValidationHelper.ValidatePassword(NewPassword);
            if (!validation.IsValid)
            {
                SetError(validation.Message);
                return;
            }

            var changed = await _authService.ChangePasswordAsync(_currentAdminId, CurrentPassword, NewPassword);
            if (!changed)
            {
                SetError("No se pudo cambiar la contrasena. Verifique la contrasena actual.");
                return;
            }

            CurrentPassword = string.Empty;
            NewPassword = string.Empty;
            ConfirmPassword = string.Empty;
            SetSuccess("Contrasena cambiada correctamente.");
        }
        catch
        {
            SetError("No se pudo cambiar la contrasena. Intente nuevamente.");
        }
    }

    private async Task LoadBackupsAsync()
    {
        try
        {
            var selectedId = SelectedBackup?.Id;
            var backups = await _backupService.GetBackupsAsync();

            Backups.Clear();
            foreach (var backup in backups)
            {
                Backups.Add(backup);
            }

            SelectedBackup = selectedId.HasValue
                ? Backups.FirstOrDefault(b => b.Id == selectedId.Value)
                : Backups.FirstOrDefault();
        }
        catch
        {
            SetError("No se pudieron cargar los respaldos.");
        }
    }

    private async Task CreateBackupAsync()
    {
        if (_currentAdminId <= 0)
        {
            SetError("No se encontro la sesion del administrador actual.");
            return;
        }

        await RunBusyAsync(async () =>
        {
            var description = string.IsNullOrWhiteSpace(BackupDescription)
                ? "Respaldo manual"
                : BackupDescription.Trim();

            var backupPath = await _backupService.CreateBackupAsync(description, "Full", _currentAdminId);
            await LoadBackupsAsync();
            SetSuccess($"Respaldo creado: {backupPath}");
        }, "No se pudo crear el respaldo.");
    }

    private async Task RestoreBackupAsync()
    {
        if (SelectedBackup == null)
            return;

        var confirmation = MessageBox.Show(
            $"Restaurar el respaldo '{SelectedBackup.Description}'?\n\nSe reemplazaran listas, categorias, horarios, palabras clave, reglas y configuracion.",
            "Restaurar respaldo",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirmation != MessageBoxResult.Yes)
            return;

        await RunBusyAsync(async () =>
        {
            var restored = await _backupService.RestoreBackupAsync(SelectedBackup.Id);
            if (!restored)
            {
                SetError("No se pudo restaurar el respaldo seleccionado.");
                return;
            }

            await _blockedSiteService.ApplyBlockingRulesAsync(writeAuditLogEntry: false);
            await LoadSettingsAsync();
            await LoadBackupsAsync();
            SetSuccess("Respaldo restaurado y reglas sincronizadas.");
        }, "No se pudo restaurar el respaldo.");
    }

    private async Task DeleteBackupAsync()
    {
        if (SelectedBackup == null)
            return;

        var confirmation = MessageBox.Show(
            $"Eliminar el registro del respaldo '{SelectedBackup.Description}'?",
            "Eliminar respaldo",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (confirmation != MessageBoxResult.Yes)
            return;

        await RunBusyAsync(async () =>
        {
            await _backupService.DeleteBackupAsync(SelectedBackup.Id);
            await LoadBackupsAsync();
            SetSuccess("Respaldo eliminado de la lista.");
        }, "No se pudo eliminar el respaldo.");
    }

    private async Task RunBusyAsync(Func<Task> action, string errorMessage)
    {
        try
        {
            IsBusy = true;
            NotifyBackupCommands();
            await action();
        }
        catch
        {
            SetError(errorMessage);
        }
        finally
        {
            IsBusy = false;
            NotifyBackupCommands();
        }
    }

    private bool CanChangePassword() =>
        !string.IsNullOrWhiteSpace(CurrentPassword) &&
        !string.IsNullOrWhiteSpace(NewPassword) &&
        !string.IsNullOrWhiteSpace(ConfirmPassword);

    private void SetSuccess(string message)
    {
        SuccessMessage = message;
        ErrorMessage = string.Empty;
    }

    private void SetError(string message)
    {
        ErrorMessage = message;
        SuccessMessage = string.Empty;
    }

    private void NotifyBackupCommands()
    {
        if (RestoreBackupCommand is AsyncRelayCommand restoreCommand)
            restoreCommand.NotifyCanExecuteChanged();

        if (DeleteBackupCommand is AsyncRelayCommand deleteCommand)
            deleteCommand.NotifyCanExecuteChanged();
    }

    partial void OnCurrentPasswordChanged(string value) => ((AsyncRelayCommand)ChangePasswordCommand).NotifyCanExecuteChanged();
    partial void OnNewPasswordChanged(string value) => ((AsyncRelayCommand)ChangePasswordCommand).NotifyCanExecuteChanged();
    partial void OnConfirmPasswordChanged(string value) => ((AsyncRelayCommand)ChangePasswordCommand).NotifyCanExecuteChanged();
    partial void OnSelectedBackupChanged(Backup? value) => NotifyBackupCommands();
    partial void OnIsBusyChanged(bool value) => NotifyBackupCommands();

    private void GoBack()
    {
        _mainWindow?.NavigateToDashboard();
    }
}
