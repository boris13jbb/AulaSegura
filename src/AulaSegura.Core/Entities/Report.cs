using System;
using AulaSegura.Core.Entities;

namespace AulaSegura.Core.Entities
{
    /// <summary>
    /// Representa un registro de actividad o reporte de bloqueo.
    /// </summary>
    public class Report : BaseEntity
    {
        /// <summary>
        /// Fecha y hora del evento.
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Fecha de inicio del período del reporte.
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Fecha de fin del período del reporte.
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// URL o dominio intentado.
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// Dirección IP del equipo cliente.
        /// </summary>
        public string ClientIp { get; set; } = string.Empty;

        /// <summary>
        /// Motivo del bloqueo (Categoría, Palabra clave, Lista negra).
        /// </summary>
        public string Reason { get; set; } = string.Empty;

        /// <summary>
        /// Acción tomada (ej. "Blocked", "Logged").
        /// </summary>
        public string ActionTaken { get; set; } = "Blocked";

        /// <summary>
        /// Tipo de reporte generado.
        /// </summary>
        public ReportType ReportType { get; set; } = ReportType.Summary;

        /// <summary>
        /// Total de bloqueos en el período del reporte.
        /// </summary>
        public int TotalBlocks { get; set; }

        /// <summary>
        /// Total de violaciones en el período del reporte.
        /// </summary>
        public int TotalViolations { get; set; }

        /// <summary>
        /// Sitio más bloqueado en el período del reporte.
        /// </summary>
        public string? TopBlockedSite { get; set; }

        /// <summary>
        /// Usuario administrador que generó el reporte (si aplica).
        /// </summary>
        public int? AdministratorId { get; set; }
        public virtual Administrator? Administrator { get; set; }
    }
}
