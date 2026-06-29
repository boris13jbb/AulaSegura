using AulaSegura.Core.Constants;
using AulaSegura.Core.Entities;
using AulaSegura.Core.Interfaces;
using AulaSegura.Core.Utilities;
using AulaSegura.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AulaSegura.Infrastructure.Services;

/// <summary>
/// Manages blocked sites and synchronizes effective blocking rules with the hosts file.
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
        var normalizedDomain = ValidationHelper.NormalizeDomain(domain);

        if (!ValidationHelper.IsValidDomain(normalizedDomain))
            throw new ArgumentException($"Dominio invalido: {domain}", nameof(domain));

        await EnsureCategoryExistsAsync(categoryId);

        if (await _context.BlockedSites.AnyAsync(b => b.Domain == normalizedDomain && b.IsActive))
            throw new InvalidOperationException($"El dominio {normalizedDomain} ya esta bloqueado");

        var inactive = await _context.BlockedSites
            .FirstOrDefaultAsync(b => b.Domain == normalizedDomain && !b.IsActive);

        if (inactive != null)
        {
            inactive.IsActive = true;
            inactive.CategoryId = categoryId;
            inactive.Reason = (reason ?? string.Empty).Trim();
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
            Reason = (reason ?? string.Empty).Trim(),
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

        var normalizedDomain = ValidationHelper.NormalizeDomain(site.Domain);
        if (!ValidationHelper.IsValidDomain(normalizedDomain))
            throw new ArgumentException($"Dominio invalido: {site.Domain}", nameof(site));

        await EnsureCategoryExistsAsync(site.CategoryId);

        var duplicate = await _context.BlockedSites.AnyAsync(b =>
            b.Id != site.Id &&
            b.Domain == normalizedDomain &&
            b.IsActive);

        if (duplicate)
            throw new InvalidOperationException($"El dominio {normalizedDomain} ya esta bloqueado");

        existing.Domain = normalizedDomain;
        existing.CategoryId = site.CategoryId;
        existing.Reason = (site.Reason ?? string.Empty).Trim();
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
        var query = _context.BlockedSites
            .Include(b => b.Category)
            .AsQueryable();

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
            .OrderBy(b => b.Domain)
            .ToListAsync();
    }

    public async Task<bool> IsDomainBlockedAsync(string domain)
    {
        var normalizedDomain = ValidationHelper.NormalizeDomain(domain);
        if (string.IsNullOrWhiteSpace(normalizedDomain))
            return false;

        if (await _context.AllowedSites.AnyAsync(a => a.Domain == normalizedDomain && a.IsActive))
            return false;

        var blockedSite = await _context.BlockedSites
            .FirstOrDefaultAsync(b => b.Domain == normalizedDomain && b.IsActive);

        if (blockedSite == null)
            return false;

        var schedules = await _context.Schedules
            .Where(s => s.IsActive && (s.CategoryId == null || s.CategoryId == blockedSite.CategoryId))
            .ToListAsync();

        return schedules.Count == 0 ||
               schedules.Any(schedule => IsScheduleActiveAt(schedule, DateTime.Now));
    }

    public async Task ApplyBlockingRulesAsync(bool writeAuditLogEntry = true)
    {
        try
        {
            await _backupService.BackupHostsFileAsync();

            var blockedSites = await _context.BlockedSites
                .Where(b => b.IsActive)
                .ToListAsync();

            var activeSchedules = await _context.Schedules
                .Where(s => s.IsActive)
                .ToListAsync();

            var allowedNormalized = await _context.AllowedSites
                .Where(a => a.IsActive)
                .Select(a => a.Domain)
                .ToListAsync();

            var allowedDomains = allowedNormalized
                .Select(ValidationHelper.NormalizeDomain)
                .Where(domain => !string.IsNullOrWhiteSpace(domain))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            await _hostsFileManager.ClearAllAulaSeguraEntriesAsync();

            int domainsWrittenToHosts = 0;
            int hostsLinesWritten = 0;
            int domainsSkippedBySchedule = 0;
            var now = DateTime.Now;

            foreach (var site in blockedSites)
            {
                var normalizedSite = ValidationHelper.NormalizeDomain(site.Domain);
                if (string.IsNullOrWhiteSpace(normalizedSite))
                    continue;

                if (allowedDomains.Contains(normalizedSite))
                    continue;

                var schedulesForSite = activeSchedules
                    .Where(schedule => AppliesToCategory(schedule, site.CategoryId))
                    .ToList();

                if (schedulesForSite.Count > 0 &&
                    !schedulesForSite.Any(schedule => IsScheduleActiveAt(schedule, now)))
                {
                    domainsSkippedBySchedule++;
                    continue;
                }

                domainsWrittenToHosts++;

                await _hostsFileManager.AddEntryAsync(normalizedSite);
                hostsLinesWritten++;

                if (site.BlockSubdomains && !normalizedSite.StartsWith("www.", StringComparison.OrdinalIgnoreCase))
                {
                    await _hostsFileManager.AddEntryAsync($"www.{normalizedSite}");
                    hostsLinesWritten++;
                }
            }

            if (writeAuditLogEntry)
            {
                await _activityLogService.LogActivityAsync(
                    SystemConstants.LogActions.ApplyBlockingRules,
                    $"Reglas aplicadas al archivo hosts. Dominios aplicados: {domainsWrittenToHosts}. Omitidos por horario: {domainsSkippedBySchedule}. Entradas anadidas: {hostsLinesWritten}.",
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
                "Ejecute la aplicacion como administrador.");
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

    private async Task EnsureCategoryExistsAsync(int categoryId)
    {
        var exists = await _context.Categories.AnyAsync(c => c.Id == categoryId && c.IsActive);
        if (!exists)
            throw new InvalidOperationException("La categoria seleccionada no existe o esta inactiva");
    }

    private static bool AppliesToCategory(Schedule schedule, int categoryId)
    {
        return schedule.CategoryId == null || schedule.CategoryId == categoryId;
    }

    private static bool IsScheduleActiveAt(Schedule schedule, DateTime dateTime)
    {
        var currentDay = dateTime.DayOfWeek;
        var previousDay = (DayOfWeek)(((int)currentDay + 6) % 7);
        var currentTime = dateTime.TimeOfDay;

        if (schedule.StartTime <= schedule.EndTime)
        {
            return schedule.DayOfWeek == currentDay &&
                   currentTime >= schedule.StartTime &&
                   currentTime <= schedule.EndTime;
        }

        return (schedule.DayOfWeek == currentDay && currentTime >= schedule.StartTime) ||
               (schedule.DayOfWeek == previousDay && currentTime <= schedule.EndTime);
    }
}
