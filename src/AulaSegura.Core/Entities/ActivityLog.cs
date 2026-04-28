namespace AulaSegura.Core.Entities;

/// <summary>
/// Registro de actividad del sistema (logs)
/// </summary>
public class ActivityLog : BaseEntity
{
    public string Action { get; set; } = string.Empty; // Login, Logout, AddSite, RemoveSite, etc.
    public string Description { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty; // BlockedSite, AllowedSite, Category, etc.
    public int? EntityId { get; set; }
    public int? AdminId { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public bool IsSuccess { get; set; } = true;
    
    // Navegación
    public Administrator? Admin { get; set; }
}
