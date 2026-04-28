namespace AulaSegura.Core.Entities;

/// <summary>
/// Representa un sitio web permitido (lista blanca)
/// </summary>
public class AllowedSite : BaseEntity
{
    public string Domain { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int? CreatedByAdminId { get; set; }
    public Administrator? CreatedByAdmin { get; set; }
}
