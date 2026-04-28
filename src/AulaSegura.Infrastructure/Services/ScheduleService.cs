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

    public async Task<Schedule> CreateScheduleAsync(string name, int? categoryId, DayOfWeek dayOfWeek, TimeSpan startTime, TimeSpan endTime)
    {
        var schedule = new Schedule
        {
            Name = name,
            CategoryId = categoryId,
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
        _context.Schedules.Update(schedule);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteScheduleAsync(int id)
    {
        var schedule = await _context.Schedules.FindAsync(id);
        if (schedule != null)
        {
            schedule.IsActive = false;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Schedule>> GetSchedulesByCategoryAsync(int categoryId)
    {
        return await _context.Schedules.Where(s => s.CategoryId == categoryId && s.IsActive).ToListAsync();
    }

    public async Task<IEnumerable<Schedule>> GetActiveSchedulesForDayAsync(DayOfWeek dayOfWeek)
    {
        return await _context.Schedules.Where(s => s.DayOfWeek == dayOfWeek && s.IsActive).ToListAsync();
    }

    public async Task<bool> IsBlockedByScheduleAsync(int categoryId, DateTime dateTime)
    {
        var schedules = await GetSchedulesByCategoryAsync(categoryId);
        var time = dateTime.TimeOfDay;

        return schedules.Any(s => time >= s.StartTime && time <= s.EndTime);
    }
}
