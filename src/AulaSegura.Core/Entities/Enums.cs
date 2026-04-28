namespace AulaSegura.Core.Entities;

/// <summary>
/// Tipos de palabras clave para filtrado de contenido
/// </summary>
public enum KeywordType
{
    /// <summary>
    /// Palabra bloqueada - el contenido que la contenga será bloqueado
    /// </summary>
    Blocked,
    
    /// <summary>
    /// Palabra permitida - excepción a las reglas de bloqueo
    /// </summary>
    Allowed,
    
    /// <summary>
    /// Palabra de advertencia - genera alerta pero no bloquea
    /// </summary>
    Warning
}

/// <summary>
/// Tipos de reportes disponibles
/// </summary>
public enum ReportType
{
    /// <summary>
    /// Reporte resumido con estadísticas generales
    /// </summary>
    Summary,
    
    /// <summary>
    /// Reporte detallado con información completa de cada evento
    /// </summary>
    Detailed,
    
    /// <summary>
    /// Reporte por categorías de bloqueo
    /// </summary>
    ByCategory,
    
    /// <summary>
    /// Reporte temporal de actividad
    /// </summary>
    TimeBased
}
