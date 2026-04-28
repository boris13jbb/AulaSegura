using AulaSegura.Core.Entities;

namespace AulaSegura.Core.Interfaces;

/// <summary>
/// Servicio de autenticación y gestión de administradores
/// </summary>
public interface IAuthService
{
    Task<Administrator?> LoginAsync(string username, string password);
    Task LogoutAsync(int adminId);
    Task<bool> ChangePasswordAsync(int adminId, string currentPassword, string newPassword);
    Task<Administrator> CreateAdminAsync(string username, string password, string email, string fullName);
    Task IncrementFailedAttemptsAsync(int adminId);
    Task ResetFailedAttemptsAsync(int adminId);
    Task<bool> IsAccountLockedAsync(int adminId);
}
