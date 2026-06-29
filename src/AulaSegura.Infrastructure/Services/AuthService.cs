using AulaSegura.Core.Constants;
using AulaSegura.Core.Entities;
using AulaSegura.Core.Interfaces;
using AulaSegura.Core.Utilities;
using AulaSegura.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AulaSegura.Infrastructure.Services;

/// <summary>
/// Servicio de autenticacion y administracion de cuentas.
/// </summary>
public class AuthService : IAuthService
{
    private readonly AulaSeguraDbContext _context;
    private readonly IActivityLogService _activityLogService;

    public AuthService(AulaSeguraDbContext context, IActivityLogService activityLogService)
    {
        _context = context;
        _activityLogService = activityLogService;
    }

    public async Task<Administrator?> LoginAsync(string username, string password)
    {
        username = username.Trim();

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return null;

        var admin = await _context.Administrators
            .FirstOrDefaultAsync(a => a.Username == username && a.IsActive);

        if (admin == null)
        {
            await _activityLogService.LogActivityAsync(
                SystemConstants.LogActions.LoginFailed,
                $"Intento de login fallido para usuario: {username}",
                "Administrator",
                null,
                null,
                false);

            return null;
        }

        if (admin.LockedUntil.HasValue && admin.LockedUntil.Value > DateTime.UtcNow)
        {
            await _activityLogService.LogActivityAsync(
                SystemConstants.LogActions.LoginFailed,
                $"Cuenta bloqueada temporalmente: {username}",
                "Administrator",
                admin.Id,
                null,
                false);

            return null;
        }

        if (!BCrypt.Net.BCrypt.Verify(password, admin.PasswordHash))
        {
            admin.FailedLoginAttempts++;

            if (admin.FailedLoginAttempts >= SystemConstants.MaxFailedLoginAttempts)
            {
                admin.LockedUntil = DateTime.UtcNow.AddMinutes(SystemConstants.AccountLockoutDurationMinutes);
            }

            await _context.SaveChangesAsync();

            await _activityLogService.LogActivityAsync(
                SystemConstants.LogActions.LoginFailed,
                $"Credenciales invalidas. Intentos fallidos: {admin.FailedLoginAttempts}",
                "Administrator",
                admin.Id,
                null,
                false);

            return null;
        }

        admin.FailedLoginAttempts = 0;
        admin.LockedUntil = null;
        admin.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await _activityLogService.LogActivityAsync(
            SystemConstants.LogActions.Login,
            $"Login exitoso: {username}",
            "Administrator",
            admin.Id,
            admin.Id,
            true);

        return admin;
    }

    public async Task LogoutAsync(int adminId)
    {
        await _activityLogService.LogActivityAsync(
            SystemConstants.LogActions.Logout,
            "Logout realizado",
            "Administrator",
            adminId,
            adminId,
            true);
    }

    public async Task<bool> ChangePasswordAsync(int adminId, string currentPassword, string newPassword)
    {
        var passwordValidation = ValidationHelper.ValidatePassword(newPassword);
        if (!passwordValidation.IsValid)
            return false;

        var admin = await _context.Administrators.FirstOrDefaultAsync(a => a.Id == adminId && a.IsActive);
        if (admin == null)
            return false;

        if (!BCrypt.Net.BCrypt.Verify(currentPassword, admin.PasswordHash))
            return false;

        admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword, workFactor: 11);
        admin.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await _activityLogService.LogActivityAsync(
            SystemConstants.LogActions.ChangePassword,
            "Contrasena cambiada exitosamente",
            "Administrator",
            adminId,
            adminId,
            true);

        return true;
    }

    public async Task<Administrator> CreateAdminAsync(string username, string password, string email, string fullName)
    {
        username = username.Trim();
        email = email.Trim();
        fullName = fullName.Trim();

        var passwordValidation = ValidationHelper.ValidatePassword(password);
        if (!passwordValidation.IsValid)
            throw new ArgumentException(passwordValidation.Message, nameof(password));

        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("El usuario es obligatorio.", nameof(username));

        if (await _context.Administrators.AnyAsync(a => a.Username == username))
            throw new InvalidOperationException("Ya existe un administrador con ese usuario.");

        var admin = new Administrator
        {
            Username = username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 11),
            Email = email,
            FullName = fullName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Administrators.AddAsync(admin);
        await _context.SaveChangesAsync();

        return admin;
    }

    public async Task IncrementFailedAttemptsAsync(int adminId)
    {
        var admin = await _context.Administrators.FindAsync(adminId);
        if (admin == null)
            return;

        admin.FailedLoginAttempts++;
        await _context.SaveChangesAsync();
    }

    public async Task ResetFailedAttemptsAsync(int adminId)
    {
        var admin = await _context.Administrators.FindAsync(adminId);
        if (admin == null)
            return;

        admin.FailedLoginAttempts = 0;
        admin.LockedUntil = null;
        await _context.SaveChangesAsync();
    }

    public async Task<bool> IsAccountLockedAsync(int adminId)
    {
        var admin = await _context.Administrators.FindAsync(adminId);
        if (admin == null)
            return false;

        return admin.LockedUntil.HasValue && admin.LockedUntil.Value > DateTime.UtcNow;
    }
}
