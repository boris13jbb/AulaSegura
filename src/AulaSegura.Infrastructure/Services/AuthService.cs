using AulaSegura.Core.Constants;
using AulaSegura.Core.Entities;
using AulaSegura.Core.Interfaces;
using AulaSegura.Infrastructure.Data;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;

namespace AulaSegura.Infrastructure.Services;

/// <summary>
/// Implementación del servicio de autenticación
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

        // Verificar si la cuenta está bloqueada
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

        // Verificar contraseña con BCrypt
        if (!BCrypt.Net.BCrypt.Verify(password, admin.PasswordHash))
        {
            admin.FailedLoginAttempts++;
            
            // Bloquear cuenta tras múltiples intentos fallidos
            if (admin.FailedLoginAttempts >= SystemConstants.MaxFailedLoginAttempts)
            {
                admin.LockedUntil = DateTime.UtcNow.AddMinutes(SystemConstants.AccountLockoutDurationMinutes);
            }
            
            await _context.SaveChangesAsync();
            
            await _activityLogService.LogActivityAsync(
                SystemConstants.LogActions.LoginFailed,
                $"Contraseña incorrecta. Intentos fallidos: {admin.FailedLoginAttempts}",
                "Administrator",
                admin.Id,
                null,
                false);
            
            return null;
        }

        // Login exitoso
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
        var admin = await _context.Administrators.FindAsync(adminId);
        if (admin == null)
            return false;

        // Verificar contraseña actual
        if (!BCrypt.Net.BCrypt.Verify(currentPassword, admin.PasswordHash))
            return false;

        // Hashear nueva contraseña
        admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        admin.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await _activityLogService.LogActivityAsync(
            SystemConstants.LogActions.ChangePassword,
            "Contraseña cambiada exitosamente",
            "Administrator",
            adminId,
            adminId,
            true);

        return true;
    }

    public async Task<Administrator> CreateAdminAsync(string username, string password, string email, string fullName)
    {
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
        
        var admin = new Administrator
        {
            Username = username,
            PasswordHash = hashedPassword,
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
        if (admin != null)
        {
            admin.FailedLoginAttempts++;
            await _context.SaveChangesAsync();
        }
    }

    public async Task ResetFailedAttemptsAsync(int adminId)
    {
        var admin = await _context.Administrators.FindAsync(adminId);
        if (admin != null)
        {
            admin.FailedLoginAttempts = 0;
            admin.LockedUntil = null;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> IsAccountLockedAsync(int adminId)
    {
        var admin = await _context.Administrators.FindAsync(adminId);
        if (admin == null)
            return false;

        return admin.LockedUntil.HasValue && admin.LockedUntil.Value > DateTime.UtcNow;
    }
}
