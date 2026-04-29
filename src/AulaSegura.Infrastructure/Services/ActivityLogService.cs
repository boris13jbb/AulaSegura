using AulaSegura.Core.Constants;
using AulaSegura.Core.Entities;
using AulaSegura.Core.Interfaces;
using AulaSegura.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AulaSegura.Infrastructure.Services;

public class ActivityLogService : IActivityLogService
{
    private readonly AulaSeguraDbContext _context;

    public ActivityLogService(AulaSeguraDbContext context)
    {
        _context = context;
    }

    public async Task LogActivityAsync(string action, string description, string entityType, int? entityId, int? adminId, bool isSuccess = true)
    {
        var log = new ActivityLog
        {
            Action = action,
            Description = description,
            EntityType = entityType,
            EntityId = entityId,
            AdminId = adminId,
            IsSuccess = isSuccess,
            CreatedAt = DateTime.UtcNow
        };

        await _context.ActivityLogs.AddAsync(log);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<ActivityLog>> GetRecentLogsAsync(int count = 50)
    {
        return await _context.ActivityLogs
            .OrderByDescending(l => l.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<IEnumerable<ActivityLog>> GetLogsByAdminAsync(int adminId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.ActivityLogs.Where(l => l.AdminId == adminId);

        if (startDate.HasValue)
            query = query.Where(l => l.CreatedAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(l => l.CreatedAt <= endDate.Value);

        return await query.OrderByDescending(l => l.CreatedAt).ToListAsync();
    }

    public async Task<IEnumerable<ActivityLog>> GetLogsByEntityTypeAsync(string entityType, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.ActivityLogs.Where(l => l.EntityType == entityType);

        if (startDate.HasValue)
            query = query.Where(l => l.CreatedAt >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(l => l.CreatedAt <= endDate.Value);

        return await query.OrderByDescending(l => l.CreatedAt).ToListAsync();
    }

    public async Task<int[]> GetDailyBlockingRelatedLogCountsAsync(int days = 7)
    {
        if (days < 1) days = 1;

        var actions = new[]
        {
            SystemConstants.LogActions.ApplyBlockingRules,
            SystemConstants.LogActions.AddBlockedSite,
            SystemConstants.LogActions.UpdateBlockedSite,
            SystemConstants.LogActions.DeleteBlockedSite
        };

        var startInclusiveUtc = DateTime.UtcNow.Date.AddDays(-(days - 1));
        var endExclusiveUtc = DateTime.UtcNow.Date.AddDays(1);

        var timestamps = await _context.ActivityLogs
            .AsNoTracking()
            .Where(l => actions.Contains(l.Action))
            .Where(l => l.CreatedAt >= startInclusiveUtc && l.CreatedAt < endExclusiveUtc)
            .Select(l => l.CreatedAt)
            .ToListAsync();

        var counts = new int[days];
        foreach (var t in timestamps)
        {
            var utcDay = NormalizeToUtcDate(t);
            var idx = (int)(utcDay - startInclusiveUtc).TotalDays;
            if ((uint)idx < (uint)days)
                counts[idx]++;
        }

        return counts;
    }

    /// <summary>Asume valores guardados como UTC (CreatedAt usa UtcNow), tolera SQLite sin especificar zona.</summary>
    private static DateTime NormalizeToUtcDate(DateTime t)
    {
        var utc = t.Kind switch
        {
            DateTimeKind.Utc => t,
            DateTimeKind.Local => t.ToUniversalTime(),
            _ => DateTime.SpecifyKind(t, DateTimeKind.Utc)
        };
        return utc.Date;
    }
}
