using AulaSegura.Core.Entities;

namespace AulaSegura.Core.Interfaces;

/// <summary>
/// Servicio de gestión de listas blancas (sitios permitidos)
/// </summary>
public interface IAllowedSiteService
{
    Task<AllowedSite> AddAllowedSiteAsync(string domain, string description, int adminId);
    Task UpdateAllowedSiteAsync(AllowedSite site);
    Task DeleteAllowedSiteAsync(int id);
    Task<AllowedSite?> GetAllowedSiteByIdAsync(int id);
    Task<IEnumerable<AllowedSite>> GetAllAllowedSitesAsync();
    Task<bool> IsDomainAllowedAsync(string domain);
}
