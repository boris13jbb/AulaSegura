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
    Task<IEnumerable<BlockedSite>> GetAllBlockedSitesAsync();
    Task<IEnumerable<BlockedSite>> GetBlockedSitesByCategoryAsync(int categoryId);
    Task<bool> IsDomainBlockedAsync(string domain);
    Task ApplyBlockingRulesAsync();
}
