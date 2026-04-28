using System;
using AulaSegura.Core.Entities;

namespace AulaSegura.Core.Entities
{
    /// <summary>
    /// Representa una regla de bloqueo avanzada (por horario, IP, etc.).
    /// </summary>
    public class BlockingRule : BaseEntity
    {
        /// <summary>
        /// Nombre descriptivo de la regla.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Tipo de regla: 'TIME', 'IP', 'KEYWORD', 'CATEGORY'.
        /// </summary>
        public string RuleType { get; set; } = string.Empty;

        /// <summary>
        /// Valor o configuración de la regla (JSON o texto plano).
        /// </summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// Descripción de la acción a tomar (ej. "Bloquear", "Registrar").
        /// </summary>
        public string Action { get; set; } = "Block";

        /// <summary>
        /// ID de la categoría asociada a esta regla.
        /// </summary>
        public int? CategoryId { get; set; }
        public virtual Category? Category { get; set; }

        /// <summary>
        /// Número máximo de violaciones antes de aplicar el bloqueo.
        /// </summary>
        public int MaxViolations { get; set; } = 3;

        /// <summary>
        /// Duración del bloqueo en minutos.
        /// </summary>
        public int BlockDurationMinutes { get; set; } = 30;
    }
}
