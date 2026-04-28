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
}
