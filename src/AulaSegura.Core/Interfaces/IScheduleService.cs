using AulaSegura.Core.Entities;

namespace AulaSegura.Core.Interfaces;

/// <summary>
/// Servicio de gestión de horarios de bloqueo
/// </summary>
public interface IScheduleService
{
    Task<Schedule> CreateScheduleAsync(string name, int? categoryId, DayOfWeek dayOfWeek, TimeSpan startTime, TimeSpan endTime);
    Task UpdateScheduleAsync(Schedule schedule);
    Task DeleteScheduleAsync(int id);
    Task<IEnumerable<Schedule>> GetSchedulesByCategoryAsync(int categoryId);
    Task<IEnumerable<Schedule>> GetActiveSchedulesForDayAsync(DayOfWeek dayOfWeek);
    Task<bool> IsBlockedByScheduleAsync(int categoryId, DateTime dateTime);
}
