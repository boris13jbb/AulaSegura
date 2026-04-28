namespace AulaSegura.Core.Entities;

/// <summary>
/// Representa un horario de bloqueo para categorías o sitios específicos
/// </summary>
public class Schedule : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public int? CategoryId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    
    // Navegación
    public Category? Category { get; set; }
}
