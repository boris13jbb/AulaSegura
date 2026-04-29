using AulaSegura.Core.Constants;
using AulaSegura.Core.Entities;
using AulaSegura.Core.Interfaces;
using AulaSegura.Core.Utilities;
using AulaSegura.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AulaSegura.Infrastructure.Services;

/// <summary>
/// Servicio de gestión de sitios bloqueados y archivo hosts
/// </summary>
public class BlockedSiteService : IBlockedSiteService
{
    private readonly AulaSeguraDbContext _context;
    private readonly IActivityLogService _activityLogService;
    private readonly IBackupService _backupService;
    private readonly HostsFileManager _hostsFileManager;

    public BlockedSiteService(
        AulaSeguraDbContext context, 
        IActivityLogService activityLogService,
        IBackupService backupService)
    {
        _context = context;
        _activityLogService = activityLogService;
        _backupService = backupService;
        _hostsFileManager = new HostsFileManager();
    }

    public async Task<BlockedSite> AddBlockedSiteAsync(string domain, int categoryId, string reason, int adminId)
    {
        // Normalizar dominio
        var normalizedDomain = ValidationHelper.NormalizeDomain(domain);
        
        if (!ValidationHelper.IsValidDomain(normalizedDomain))
            throw new ArgumentException($"Dominio inválido: {domain}");

        // Duplicado activo
        if (await _context.BlockedSites.AnyAsync(b => b.Domain == normalizedDomain && b.IsActive))
            throw new InvalidOperationException($"El dominio {normalizedDomain} ya está bloqueado");

        // Mismo dominio pero registro desactivado ("eliminado"): reactivar fila existente (Domain es único en BD)
        var inactive = await _context.BlockedSites.FirstOrDefaultAsync(b => b.Domain == normalizedDomain && !b.IsActive);
        if (inactive != null)
        {
            inactive.IsActive = true;
            inactive.CategoryId = categoryId;
            inactive.Reason = reason;
            inactive.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await _activityLogService.LogActivityAsync(
                SystemConstants.LogActions.UpdateBlockedSite,
                $"Sitio reactivado en lista negra: {normalizedDomain}",
                "BlockedSite",
                inactive.Id,
                adminId,
                true);

            await ApplyBlockingRulesAsync(writeAuditLogEntry: false);

            return inactive;
        }

        var blockedSite = new BlockedSite
        {
            Domain = normalizedDomain,
            CategoryId = categoryId,
            Reason = reason,
            CreatedByAdminId = adminId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _context.BlockedSites.AddAsync(blockedSite);
        await _context.SaveChangesAsync();

        await _activityLogService.LogActivityAsync(
            SystemConstants.LogActions.AddBlockedSite,
            $"Sitio agregado a lista negra: {normalizedDomain}",
            "BlockedSite",
            blockedSite.Id,
            adminId,
            true);

        await ApplyBlockingRulesAsync(writeAuditLogEntry: false);

        return blockedSite;
    }

    public async Task UpdateBlockedSiteAsync(BlockedSite site)
    {
        var existing = await _context.BlockedSites.FindAsync(site.Id);
        if (existing == null)
            throw new InvalidOperationException("Sitio no encontrado");

        existing.Domain = ValidationHelper.NormalizeDomain(site.Domain);
        existing.CategoryId = site.CategoryId;
        existing.Reason = site.Reason;
        existing.BlockSubdomains = site.BlockSubdomains;
        existing.IsActive = site.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _activityLogService.LogActivityAsync(
            SystemConstants.LogActions.UpdateBlockedSite,
            $"Sitio actualizado: {existing.Domain}",
            "BlockedSite",
            existing.Id,
            existing.CreatedByAdminId,
            true);

        await ApplyBlockingRulesAsync(writeAuditLogEntry: false);
    }

    public async Task DeleteBlockedSiteAsync(int id)
    {
        var site = await _context.BlockedSites.FindAsync(id);
        if (site == null)
            throw new InvalidOperationException("Sitio no encontrado");

        site.IsActive = false;
        site.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await _activityLogService.LogActivityAsync(
            SystemConstants.LogActions.DeleteBlockedSite,
            $"Sitio eliminado de lista negra: {site.Domain}",
            "BlockedSite",
            site.Id,
            site.CreatedByAdminId,
            true);

        await ApplyBlockingRulesAsync(writeAuditLogEntry: false);
    }

    public async Task<BlockedSite?> GetBlockedSiteByIdAsync(int id)
    {
        return await _context.BlockedSites
            .Include(b => b.Category)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<IEnumerable<BlockedSite>> GetAllBlockedSitesAsync(bool includeInactive = false)
    {
        var query = _context.BlockedSites.Include(b => b.Category).AsQueryable();

        if (!includeInactive)
            query = query.Where(b => b.IsActive);

        return await query
            .OrderBy(b => b.IsActive ? 0 : 1)
            .ThenBy(b => b.Domain)
            .ToListAsync();
    }

    public async Task<IEnumerable<BlockedSite>> GetBlockedSitesByCategoryAsync(int categoryId)
    {
        return await _context.BlockedSites
            .Include(b => b.Category)
            .Where(b => b.CategoryId == categoryId && b.IsActive)
            .ToListAsync();
    }

    public async Task<bool> IsDomainBlockedAsync(string domain)
    {
        var normalizedDomain = ValidationHelper.NormalizeDomain(domain);
        return await _context.BlockedSites
            .AnyAsync(b => b.Domain == normalizedDomain && b.IsActive);
    }

    public async Task ApplyBlockingRulesAsync(bool writeAuditLogEntry = true)
    {
        try
        {
            await _backupService.BackupHostsFileAsync();

            var blockedSites = await _context.BlockedSites
                .Where(b => b.IsActive)
                .ToListAsync();

            var allowedSitesRaw = await _context.AllowedSites
                .Where(a => a.IsActive)
                .Select(a => a.Domain)
                .ToListAsync();

            var allowedNormalized = allowedSitesRaw
                .Select(ValidationHelper.NormalizeDomain)
                .Where(d => !string.IsNullOrEmpty(d))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            await _hostsFileManager.ClearAllAulaSeguraEntriesAsync();

            int domainsWrittenToHosts = 0;
            int hostsLinesWritten = 0;

            foreach (var site in blockedSites)
            {
                var normalizedSite = ValidationHelper.NormalizeDomain(site.Domain);

                var isInWhitelist = allowedNormalized.Contains(normalizedSite);

                if (!isInWhitelist)
                {
                    domainsWrittenToHosts++;

                    await _hostsFileManager.AddEntryAsync(site.Domain);
                    hostsLinesWritten++;

                    if (site.BlockSubdomains && !site.Domain.StartsWith("www.", StringComparison.OrdinalIgnoreCase))
                    {
                        await _hostsFileManager.AddEntryAsync($"www.{site.Domain}");
                        hostsLinesWritten++;
                    }
                }
            }

            if (writeAuditLogEntry)
            {
                await _activityLogService.LogActivityAsync(
                    SystemConstants.LogActions.ApplyBlockingRules,
                    $"Reglas aplicadas al archivo hosts. Dominios aplicados (no lista blanca): {domainsWrittenToHosts}. Entradas en hosts añadidas: {hostsLinesWritten} (p. ej. dominio + www).",
                    "System",
                    null,
                    null,
                    true);
            }
        }
        catch (UnauthorizedAccessException)
        {
            throw new InvalidOperationException(
                "Se requieren permisos de administrador para modificar el archivo hosts. " +
                "Ejecute la aplicación como administrador.");
        }
        catch (Exception ex)
        {
            await _activityLogService.LogActivityAsync(
                SystemConstants.LogActions.ApplyBlockingRules,
                $"Error al aplicar reglas de bloqueo: {ex.Message}",
                "System",
                null,
                null,
                false);

            throw;
        }
    }
}
