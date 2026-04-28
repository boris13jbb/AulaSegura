using AulaSegura.Core.Entities;
using AulaSegura.Core.Interfaces;
using AulaSegura.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AulaSegura.Infrastructure.Services;

public class SettingsService : ISettingsService
{
    private readonly AulaSeguraDbContext _context;

    public SettingsService(AulaSeguraDbContext context)
    {
        _context = context;
    }

    public async Task<string> GetSettingAsync(string key)
    {
        var setting = await _context.Settings.FirstOrDefaultAsync(s => s.Key == key);
        return setting?.Value ?? string.Empty;
    }

    public async Task SetSettingAsync(string key, string value, string description = "")
    {
        var setting = await _context.Settings.FirstOrDefaultAsync(s => s.Key == key);
        
        if (setting == null)
        {
            setting = new Setting
            {
                Key = key,
                Value = value,
                Description = description,
                IsActive = true
            };
            await _context.Settings.AddAsync(setting);
        }
        else
        {
            setting.Value = value;
            setting.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<Dictionary<string, string>> GetAllSettingsAsync()
    {
        return await _context.Settings
            .Where(s => s.IsActive)
            .ToDictionaryAsync(s => s.Key, s => s.Value);
    }

    public async Task<bool> GetBoolSettingAsync(string key, bool defaultValue = false)
    {
        var value = await GetSettingAsync(key);
        return string.IsNullOrEmpty(value) ? defaultValue : bool.Parse(value);
    }

    public async Task<int> GetIntSettingAsync(string key, int defaultValue = 0)
    {
        var value = await GetSettingAsync(key);
        return string.IsNullOrEmpty(value) ? defaultValue : int.Parse(value);
    }
}
