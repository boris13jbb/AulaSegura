namespace AulaSegura.Core.Entities;

/// <summary>
/// Configuración general del sistema
/// </summary>
public class Setting : BaseEntity
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = "General"; // General, Security, Blocking, etc.
}
