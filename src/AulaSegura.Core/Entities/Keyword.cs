using System;
using AulaSegura.Core.Entities;

namespace AulaSegura.Core.Entities
{
    /// <summary>
    /// Representa una palabra clave utilizada para filtrado de contenido.
    /// </summary>
    public class Keyword : BaseEntity
    {
        /// <summary>
        /// La palabra o frase a buscar en URLs o títulos.
        /// </summary>
        public string Word { get; set; } = string.Empty;

        /// <summary>
        /// Tipo de palabra clave (Bloqueada, Permitida, Advertencia).
        /// </summary>
        public KeywordType Type { get; set; } = KeywordType.Blocked;

        /// <summary>
        /// Categoría asociada (opcional).
        /// </summary>
        public int? CategoryId { get; set; }
        public virtual Category? Category { get; set; }
    }
}
