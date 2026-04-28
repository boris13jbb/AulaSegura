namespace AulaSegura.Core.Entities;

/// <summary>
/// Representa un sitio web bloqueado
/// </summary>
public class BlockedSite : BaseEntity
{
    public string Domain { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public bool BlockSubdomains { get; set; } = true;
    
    // Claves foráneas
    public Category? Category { get; set; }
    public int? CreatedByAdminId { get; set; }
    public Administrator? CreatedByAdmin { get; set; }
}
