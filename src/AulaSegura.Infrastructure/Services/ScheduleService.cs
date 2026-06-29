using AulaSegura.Core.Entities;
using AulaSegura.Core.Interfaces;
using AulaSegura.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AulaSegura.Infrastructure.Services;

public class ScheduleService : IScheduleService
{
    private readonly AulaSeguraDbContext _context;

    public ScheduleService(AulaSeguraDbContext context)
    {
        _context = context;
    }

    public async Task<Schedule> CreateScheduleAsync(
        string name,
        int? categoryId,
        DayOfWeek dayOfWeek,
        TimeSpan startTime,
        TimeSpan endTime)
    {
        var normalizedCategoryId = NormalizeCategoryId(categoryId);
        await ValidateScheduleAsync(name, normalizedCategoryId, startTime, endTime);

        var schedule = new Schedule
        {
            Name = name.Trim(),
            CategoryId = normalizedCategoryId,
            DayOfWeek = dayOfWeek,
            StartTime = startTime,
            EndTime = endTime,
            IsActive = true
        };

        await _context.Schedules.AddAsync(schedule);
        await _context.SaveChangesAsync();
        return schedule;
    }

    public async Task UpdateScheduleAsync(Schedule schedule)
    {
        var existing = await _context.Schedules.FindAsync(schedule.Id);
        if (existing == null)
            throw new InvalidOperationException("Horario no encontrado");

        var normalizedCategoryId = NormalizeCategoryId(schedule.CategoryId);
        await ValidateScheduleAsync(schedule.Name, normalizedCategoryId, schedule.StartTime, schedule.EndTime);

        existing.Name = schedule.Name.Trim();
        existing.CategoryId = normalizedCategoryId;
        existing.DayOfWeek = schedule.DayOfWeek;
        existing.StartTime = schedule.StartTime;
        existing.EndTime = schedule.EndTime;
        existing.IsActive = schedule.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteScheduleAsync(int id)
    {
        var schedule = await _context.Schedules.FindAsync(id);
        if (schedule == null)
            return;

        schedule.IsActive = false;
        schedule.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Schedule>> GetSchedulesByCategoryAsync(int categoryId)
    {
        return await _context.Schedules
            .Include(s => s.Category)
            .Where(s => s.IsActive && (s.CategoryId == null || s.CategoryId == categoryId))
            .OrderBy(s => s.DayOfWeek)
            .ThenBy(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Schedule>> GetActiveSchedulesForDayAsync(DayOfWeek dayOfWeek)
    {
        return await _context.Schedules
            .Include(s => s.Category)
            .Where(s => s.DayOfWeek == dayOfWeek && s.IsActive)
            .OrderBy(s => s.StartTime)
            .ThenBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<bool> IsBlockedByScheduleAsync(int categoryId, DateTime dateTime)
    {
        var schedules = await _context.Schedules
            .Where(s => s.IsActive && (s.CategoryId == null || s.CategoryId == categoryId))
            .ToListAsync();

        return schedules.Any(schedule => IsScheduleActiveAt(schedule, dateTime));
    }

    private async Task ValidateScheduleAsync(
        string name,
        int? categoryId,
        TimeSpan startTime,
        TimeSpan endTime)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre del horario es obligatorio", nameof(name));

        if (startTime == endTime)
            throw new ArgumentException("La hora de inicio y fin no pueden ser iguales");

        if (categoryId.HasValue)
        {
            var categoryExists = await _context.Categories
                .AnyAsync(c => c.Id == categoryId.Value && c.IsActive);

            if (!categoryExists)
                throw new InvalidOperationException("La categoria seleccionada no existe o esta inactiva");
        }
    }

    private static int? NormalizeCategoryId(int? categoryId)
    {
        return categoryId is > 0 ? categoryId : null;
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
