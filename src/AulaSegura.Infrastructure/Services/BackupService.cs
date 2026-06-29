using AulaSegura.Core.Constants;
using AulaSegura.Core.Entities;
using AulaSegura.Core.Interfaces;
using AulaSegura.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AulaSegura.Infrastructure.Services;

/// <summary>
/// Servicio de respaldo y restauracion de configuracion operativa.
/// </summary>
public class BackupService : IBackupService
{
    private readonly AulaSeguraDbContext _context;
    private readonly IActivityLogService _activityLogService;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public BackupService(AulaSeguraDbContext context, IActivityLogService activityLogService)
    {
        _context = context;
        _activityLogService = activityLogService;
    }

    public async Task<string> CreateBackupAsync(string description, string backupType, int? adminId)
    {
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var backupDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SystemConstants.Paths.BackupsPath);
        Directory.CreateDirectory(backupDir);

        var backupFileName = $"backup_{timestamp}.json";
        var backupPath = Path.Combine(backupDir, backupFileName);

        var backupData = new ConfigurationBackupDocument
        {
            Timestamp = DateTime.UtcNow,
            Type = string.IsNullOrWhiteSpace(backupType) ? "Full" : backupType.Trim(),
            Description = description.Trim(),
            Categories = await _context.Categories.AsNoTracking().Select(c => new CategoryBackupItem
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                Color = c.Color,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            }).ToListAsync(),
            BlockedSites = await _context.BlockedSites.AsNoTracking().Select(b => new BlockedSiteBackupItem
            {
                Id = b.Id,
                Domain = b.Domain,
                CategoryId = b.CategoryId,
                Reason = b.Reason,
                BlockSubdomains = b.BlockSubdomains,
                CreatedByAdminId = b.CreatedByAdminId,
                IsActive = b.IsActive,
                CreatedAt = b.CreatedAt,
                UpdatedAt = b.UpdatedAt
            }).ToListAsync(),
            AllowedSites = await _context.AllowedSites.AsNoTracking().Select(a => new AllowedSiteBackupItem
            {
                Id = a.Id,
                Domain = a.Domain,
                Description = a.Description,
                CreatedByAdminId = a.CreatedByAdminId,
                IsActive = a.IsActive,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            }).ToListAsync(),
            Settings = await _context.Settings.AsNoTracking().Select(s => new SettingBackupItem
            {
                Id = s.Id,
                Key = s.Key,
                Value = s.Value,
                Description = s.Description,
                Category = s.Category,
                IsActive = s.IsActive,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt
            }).ToListAsync(),
            Schedules = await _context.Schedules.AsNoTracking().Select(s => new ScheduleBackupItem
            {
                Id = s.Id,
                Name = s.Name,
                CategoryId = s.CategoryId,
                DayOfWeek = s.DayOfWeek,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                IsActive = s.IsActive,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt
            }).ToListAsync(),
            Keywords = await _context.Keywords.AsNoTracking().Select(k => new KeywordBackupItem
            {
                Id = k.Id,
                Word = k.Word,
                CategoryId = k.CategoryId,
                IsActive = k.IsActive,
                CreatedAt = k.CreatedAt,
                UpdatedAt = k.UpdatedAt
            }).ToListAsync(),
            BlockingRules = await _context.BlockingRules.AsNoTracking().Select(r => new BlockingRuleBackupItem
            {
                Id = r.Id,
                Name = r.Name,
                RuleType = r.RuleType,
                Value = r.Value,
                Action = r.Action,
                CategoryId = r.CategoryId,
                MaxViolations = r.MaxViolations,
                BlockDurationMinutes = r.BlockDurationMinutes,
                IsActive = r.IsActive,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt
            }).ToListAsync()
        };

        var jsonContent = JsonSerializer.Serialize(backupData, JsonOptions);
        await File.WriteAllTextAsync(backupPath, jsonContent);

        var backup = new Backup
        {
            BackupPath = backupPath,
            Description = string.IsNullOrWhiteSpace(description) ? "Respaldo manual" : description.Trim(),
            BackupType = backupData.Type,
            SizeInBytes = new FileInfo(backupPath).Length,
            CreatedByAdminId = adminId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Backups.AddAsync(backup);
        await _context.SaveChangesAsync();

        await _activityLogService.LogActivityAsync(
            SystemConstants.LogActions.BackupCreated,
            $"Respaldo creado: {backup.Description}",
            "Backup",
            backup.Id,
            adminId,
            true);

        return backupPath;
    }

    public async Task<bool> RestoreBackupAsync(int backupId)
    {
        var backup = await _context.Backups.FindAsync(backupId);
        if (backup == null)
            return false;

        var jsonPath = ResolveBackupJsonPath(backup.BackupPath);
        if (!File.Exists(jsonPath))
            return false;

        var json = await File.ReadAllTextAsync(jsonPath);
        var document = JsonSerializer.Deserialize<ConfigurationBackupDocument>(json, JsonOptions);
        if (document == null)
            return false;

        await using var transaction = await _context.Database.BeginTransactionAsync();

        await _context.BlockedSites.ExecuteDeleteAsync();
        await _context.AllowedSites.ExecuteDeleteAsync();
        await _context.Schedules.ExecuteDeleteAsync();
        await _context.Keywords.ExecuteDeleteAsync();
        await _context.BlockingRules.ExecuteDeleteAsync();
        await _context.Settings.ExecuteDeleteAsync();
        await _context.Categories.ExecuteDeleteAsync();

        await _context.Categories.AddRangeAsync(document.Categories.Select(c => new Category
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            Color = c.Color,
            IsActive = c.IsActive,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        }));

        await _context.SaveChangesAsync();

        await _context.BlockedSites.AddRangeAsync(document.BlockedSites.Select(b => new BlockedSite
        {
            Id = b.Id,
            Domain = b.Domain,
            CategoryId = b.CategoryId,
            Reason = b.Reason,
            BlockSubdomains = b.BlockSubdomains,
            CreatedByAdminId = b.CreatedByAdminId,
            IsActive = b.IsActive,
            CreatedAt = b.CreatedAt,
            UpdatedAt = b.UpdatedAt
        }));

        await _context.AllowedSites.AddRangeAsync(document.AllowedSites.Select(a => new AllowedSite
        {
            Id = a.Id,
            Domain = a.Domain,
            Description = a.Description,
            CreatedByAdminId = a.CreatedByAdminId,
            IsActive = a.IsActive,
            CreatedAt = a.CreatedAt,
            UpdatedAt = a.UpdatedAt
        }));

        await _context.Settings.AddRangeAsync(document.Settings.Select(s => new Setting
        {
            Id = s.Id,
            Key = s.Key,
            Value = s.Value,
            Description = s.Description,
            Category = s.Category,
            IsActive = s.IsActive,
            CreatedAt = s.CreatedAt,
            UpdatedAt = s.UpdatedAt
        }));

        await _context.Schedules.AddRangeAsync(document.Schedules.Select(s => new Schedule
        {
            Id = s.Id,
            Name = s.Name,
            CategoryId = s.CategoryId,
            DayOfWeek = s.DayOfWeek,
            StartTime = s.StartTime,
            EndTime = s.EndTime,
            IsActive = s.IsActive,
            CreatedAt = s.CreatedAt,
            UpdatedAt = s.UpdatedAt
        }));

        await _context.Keywords.AddRangeAsync(document.Keywords.Select(k => new Keyword
        {
            Id = k.Id,
            Word = k.Word,
            CategoryId = k.CategoryId,
            IsActive = k.IsActive,
            CreatedAt = k.CreatedAt,
            UpdatedAt = k.UpdatedAt
        }));

        await _context.BlockingRules.AddRangeAsync(document.BlockingRules.Select(r => new BlockingRule
        {
            Id = r.Id,
            Name = r.Name,
            RuleType = r.RuleType,
            Value = r.Value,
            Action = r.Action,
            CategoryId = r.CategoryId,
            MaxViolations = r.MaxViolations,
            BlockDurationMinutes = r.BlockDurationMinutes,
            IsActive = r.IsActive,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt
        }));

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        await _activityLogService.LogActivityAsync(
            SystemConstants.LogActions.BackupRestored,
            $"Respaldo restaurado: {backup.Description}",
            "Backup",
            backupId,
            backup.CreatedByAdminId,
            true);

        return true;
    }

    public async Task<IEnumerable<Backup>> GetBackupsAsync()
    {
        return await _context.Backups
            .Where(b => b.IsActive)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
    }

    public async Task DeleteBackupAsync(int backupId)
    {
        var backup = await _context.Backups.FindAsync(backupId);
        if (backup == null)
            return;

        backup.IsActive = false;
        backup.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task BackupHostsFileAsync()
    {
        try
        {
            var hostsPath = SystemConstants.Paths.HostsFile;
            if (!File.Exists(hostsPath))
                return;

            var backupDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SystemConstants.Paths.BackupsPath, "hosts");
            Directory.CreateDirectory(backupDir);

            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var backupPath = Path.Combine(backupDir, $"hosts_backup_{timestamp}");

            File.Copy(hostsPath, backupPath, true);
        }
        catch (Exception ex)
        {
            await _activityLogService.LogActivityAsync(
                "BACKUP_HOSTS",
                $"Error al respaldar archivo hosts: {ex.Message}",
                "System",
                null,
                null,
                false);
        }
    }

    public async Task RestoreHostsFileAsync()
    {
        var backupDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SystemConstants.Paths.BackupsPath, "hosts");

        if (!Directory.Exists(backupDir))
            return;

        var latestBackup = Directory.GetFiles(backupDir)
            .OrderByDescending(f => f)
            .FirstOrDefault();

        if (latestBackup != null && File.Exists(SystemConstants.Paths.HostsFile))
        {
            File.Copy(latestBackup, SystemConstants.Paths.HostsFile, true);
        }

        await Task.CompletedTask;
    }

    private static string ResolveBackupJsonPath(string backupPath)
    {
        return backupPath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)
            ? backupPath.Replace(".zip", ".json", StringComparison.OrdinalIgnoreCase)
            : backupPath;
    }

    private sealed class ConfigurationBackupDocument
    {
        public DateTime Timestamp { get; set; }
        public string Type { get; set; } = "Full";
        public string Description { get; set; } = string.Empty;
        public List<CategoryBackupItem> Categories { get; set; } = [];
        public List<BlockedSiteBackupItem> BlockedSites { get; set; } = [];
        public List<AllowedSiteBackupItem> AllowedSites { get; set; } = [];
        public List<SettingBackupItem> Settings { get; set; } = [];
        public List<ScheduleBackupItem> Schedules { get; set; } = [];
        public List<KeywordBackupItem> Keywords { get; set; } = [];
        public List<BlockingRuleBackupItem> BlockingRules { get; set; } = [];
    }

    private sealed class CategoryBackupItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Color { get; set; } = "#607D8B";
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    private sealed class BlockedSiteBackupItem
    {
        public int Id { get; set; }
        public string Domain { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public bool BlockSubdomains { get; set; }
        public int? CreatedByAdminId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    private sealed class AllowedSiteBackupItem
    {
        public int Id { get; set; }
        public string Domain { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int? CreatedByAdminId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    private sealed class SettingBackupItem
    {
        public int Id { get; set; }
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    private sealed class ScheduleBackupItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int? CategoryId { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    private sealed class KeywordBackupItem
    {
        public int Id { get; set; }
        public string Word { get; set; } = string.Empty;
        public int? CategoryId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    private sealed class BlockingRuleBackupItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string RuleType { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Action { get; set; } = "Block";
        public int? CategoryId { get; set; }
        public int MaxViolations { get; set; }
        public int BlockDurationMinutes { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
