using AulaSegura.Core.Entities;

namespace AulaSegura.Core.Interfaces;

/// <summary>
/// Servicio de registro de actividad (logging)
/// </summary>
public interface IActivityLogService
{
    Task LogActivityAsync(string action, string description, string entityType, int? entityId, int? adminId, bool isSuccess = true);
    Task<IEnumerable<ActivityLog>> GetRecentLogsAsync(int count = 50);
    Task<IEnumerable<ActivityLog>> GetLogsByAdminAsync(int adminId, DateTime? startDate = null, DateTime? endDate = null);
    Task<IEnumerable<ActivityLog>> GetLogsByEntityTypeAsync(string entityType, DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// Por cada día UTC (desde el inicio hasta hoy inclusivo), cuenta eventos relacionados con bloqueo (acciones conocidas).
    /// </summary>
    Task<int[]> GetDailyBlockingRelatedLogCountsAsync(int days = 7);
}
