using AulaSegura.Core.Entities;

namespace AulaSegura.Core.Interfaces;

/// <summary>
/// Servicio de gestión de sitios bloqueados y listas negras
/// </summary>
public interface IBlockedSiteService
{
    Task<BlockedSite> AddBlockedSiteAsync(string domain, int categoryId, string reason, int adminId);
    Task UpdateBlockedSiteAsync(BlockedSite site);
    Task DeleteBlockedSiteAsync(int id);
    Task<BlockedSite?> GetBlockedSiteByIdAsync(int id);
    /// <param name="includeInactive">Si es true, incluye registros con bloqueo desactivado en lista negra (mismo dominio se puede volver a activar).</param>
    Task<IEnumerable<BlockedSite>> GetAllBlockedSitesAsync(bool includeInactive = false);
    Task<IEnumerable<BlockedSite>> GetBlockedSitesByCategoryAsync(int categoryId);
    Task<bool> IsDomainBlockedAsync(string domain);
    /// <param name="writeAuditLogEntry">False al sincronizar tras guardar desde la lista negra para no llenar el log en cada pulsación.</param>
    Task ApplyBlockingRulesAsync(bool writeAuditLogEntry = true);
}
