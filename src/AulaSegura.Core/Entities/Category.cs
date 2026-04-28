namespace AulaSegura.Core.Entities;

/// <summary>
/// Representa una categoría de sitios web (Adultos, Redes Sociales, etc.)
/// </summary>
public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Color { get; set; } = "#FF5733"; // Color para UI
    
    // Navegación
    public ICollection<BlockedSite>? BlockedSites { get; set; }
    public ICollection<Schedule>? Schedules { get; set; }
}
