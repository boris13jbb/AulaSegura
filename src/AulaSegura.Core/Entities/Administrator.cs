namespace AulaSegura.Core.Entities;

/// <summary>
/// Representa un administrador del sistema
/// </summary>
public class Administrator : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public int FailedLoginAttempts { get; set; } = 0;
    public DateTime? LastLoginAt { get; set; }
    public DateTime? LockedUntil { get; set; }
    
    // Navegación
    public ICollection<ActivityLog>? ActivityLogs { get; set; }
}
