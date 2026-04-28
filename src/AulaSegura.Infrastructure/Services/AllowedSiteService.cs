using AulaSegura.Core.Entities;
using AulaSegura.Core.Interfaces;
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
        var site = new AllowedSite
        {
            Domain = domain.ToLower().Trim(),
            Description = description,
            CreatedByAdminId = adminId,
            IsActive = true
        };

        await _context.AllowedSites.AddAsync(site);
        await _context.SaveChangesAsync();
        return site;
    }

    public async Task UpdateAllowedSiteAsync(AllowedSite site)
    {
        _context.AllowedSites.Update(site);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAllowedSiteAsync(int id)
    {
        var site = await _context.AllowedSites.FindAsync(id);
        if (site != null)
        {
            site.IsActive = false;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<AllowedSite?> GetAllowedSiteByIdAsync(int id)
    {
        return await _context.AllowedSites.FindAsync(id);
    }

    public async Task<IEnumerable<AllowedSite>> GetAllAllowedSitesAsync()
    {
        return await _context.AllowedSites.Where(a => a.IsActive).ToListAsync();
    }

    public async Task<bool> IsDomainAllowedAsync(string domain)
    {
        return await _context.AllowedSites.AnyAsync(a => 
            a.Domain.Equals(domain.ToLower().Trim(), StringComparison.OrdinalIgnoreCase) && a.IsActive);
    }
}
