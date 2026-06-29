using AulaSegura.Core.Constants;
using AulaSegura.Core.Entities;
using AulaSegura.Core.Interfaces;
using AulaSegura.Core.Utilities;
using AulaSegura.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AulaSegura.Infrastructure.Services;

public class AllowedSiteService : IAllowedSiteService
{
    private readonly AulaSeguraDbContext _context;
    private readonly IActivityLogService _activityLogService;

    public AllowedSiteService(AulaSeguraDbContext context, IActivityLogService activityLogService)
    {
        _context = context;
        _activityLogService = activityLogService;
    }

    public async Task<AllowedSite> AddAllowedSiteAsync(string domain, string description, int adminId)
    {
        var normalizedDomain = ValidationHelper.NormalizeDomain(domain);
        if (!ValidationHelper.IsValidDomain(normalizedDomain))
            throw new ArgumentException($"Dominio invalido: {domain}", nameof(domain));

        if (await _context.AllowedSites.AnyAsync(a => a.Domain == normalizedDomain && a.IsActive))
            throw new InvalidOperationException($"El dominio {normalizedDomain} ya esta en la lista blanca.");

        var inactive = await _context.AllowedSites
            .FirstOrDefaultAsync(a => a.Domain == normalizedDomain && !a.IsActive);

        if (inactive != null)
        {
            inactive.Description = description.Trim();
            inactive.CreatedByAdminId = adminId;
            inactive.IsActive = true;
            inactive.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await LogAllowedSiteChangeAsync(SystemConstants.LogActions.UpdateAllowedSite, inactive, adminId);
            return inactive;
        }

        var site = new AllowedSite
        {
            Domain = normalizedDomain,
            Description = description.Trim(),
            CreatedByAdminId = adminId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _context.AllowedSites.AddAsync(site);
        await _context.SaveChangesAsync();

        await LogAllowedSiteChangeAsync(SystemConstants.LogActions.AddAllowedSite, site, adminId);
        return site;
    }

    public async Task UpdateAllowedSiteAsync(AllowedSite site)
    {
        var existing = await _context.AllowedSites.FindAsync(site.Id);
        if (existing == null)
            throw new InvalidOperationException("Sitio permitido no encontrado");

        var normalizedDomain = ValidationHelper.NormalizeDomain(site.Domain);
        if (!ValidationHelper.IsValidDomain(normalizedDomain))
            throw new ArgumentException($"Dominio invalido: {site.Domain}", nameof(site));

        var duplicated = await _context.AllowedSites.AnyAsync(a =>
            a.Id != site.Id &&
            a.Domain == normalizedDomain &&
            a.IsActive);

        if (duplicated)
            throw new InvalidOperationException($"El dominio {normalizedDomain} ya esta en la lista blanca.");

        existing.Domain = normalizedDomain;
        existing.Description = site.Description?.Trim() ?? string.Empty;
        existing.IsActive = site.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await LogAllowedSiteChangeAsync(SystemConstants.LogActions.UpdateAllowedSite, existing, existing.CreatedByAdminId);
    }

    public async Task DeleteAllowedSiteAsync(int id)
    {
        var site = await _context.AllowedSites.FindAsync(id);
        if (site == null)
            return;

        site.IsActive = false;
        site.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await LogAllowedSiteChangeAsync(SystemConstants.LogActions.DeleteAllowedSite, site, site.CreatedByAdminId);
    }

    public async Task<AllowedSite?> GetAllowedSiteByIdAsync(int id)
    {
        return await _context.AllowedSites.FindAsync(id);
    }

    public async Task<IEnumerable<AllowedSite>> GetAllAllowedSitesAsync()
    {
        return await _context.AllowedSites
            .Where(a => a.IsActive)
            .OrderBy(a => a.Domain)
            .ToListAsync();
    }

    public async Task<bool> IsDomainAllowedAsync(string domain)
    {
        var normalized = ValidationHelper.NormalizeDomain(domain);
        if (string.IsNullOrEmpty(normalized))
            return false;

        return await _context.AllowedSites.AnyAsync(a =>
            a.Domain == normalized && a.IsActive);
    }

    private async Task LogAllowedSiteChangeAsync(string action, AllowedSite site, int? adminId)
    {
        await _activityLogService.LogActivityAsync(
            action,
            $"Lista blanca actualizada: {site.Domain}",
            "AllowedSite",
            site.Id,
            adminId,
            true);
    }
}
