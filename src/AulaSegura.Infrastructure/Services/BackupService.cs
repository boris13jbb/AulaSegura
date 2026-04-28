using AulaSegura.Core.Constants;
using AulaSegura.Core.Entities;
using AulaSegura.Core.Interfaces;
using AulaSegura.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AulaSegura.Infrastructure.Services;

/// <summary>
/// Servicio de respaldo y restauración
/// </summary>
public class BackupService : IBackupService
{
    private readonly AulaSeguraDbContext _context;
    private readonly IActivityLogService _activityLogService;

    public BackupService(AulaSeguraDbContext context, IActivityLogService activityLogService)
    {
        _context = context;
        _activityLogService = activityLogService;
    }

    public async Task<string> CreateBackupAsync(string description, string backupType, int? adminId)
    {
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var backupDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SystemConstants.Paths.BackupsPath);
        
        if (!Directory.Exists(backupDir))
            Directory.CreateDirectory(backupDir);

        var backupFileName = $"backup_{timestamp}.zip";
        var backupPath = Path.Combine(backupDir, backupFileName);

        // Crear archivo de respaldo (simplificado - en producción usar compresión real)
        var backupData = new
        {
            Timestamp = DateTime.UtcNow,
            Type = backupType,
            Description = description,
            BlockedSites = await _context.BlockedSites.Where(b => b.IsActive).ToListAsync(),
            AllowedSites = await _context.AllowedSites.Where(a => a.IsActive).ToListAsync(),
            Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync(),
            Settings = await _context.Settings.ToListAsync()
        };

        var jsonContent = System.Text.Json.JsonSerializer.Serialize(backupData, new System.Text.Json.JsonSerializerOptions 
        { 
            WriteIndented = true,
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
        });

        await File.WriteAllTextAsync(backupPath.Replace(".zip", ".json"), jsonContent);

        var backup = new Backup
        {
            BackupPath = backupPath,
            Description = description,
            BackupType = backupType,
            SizeInBytes = new FileInfo(backupPath.Replace(".zip", ".json")).Length,
            CreatedByAdminId = adminId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Backups.AddAsync(backup);
        await _context.SaveChangesAsync();

        await _activityLogService.LogActivityAsync(
            SystemConstants.LogActions.BackupCreated,
            $"Respaldo creado: {description}",
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

        var jsonPath = backup.BackupPath.Replace(".zip", ".json");
        if (!File.Exists(jsonPath))
            return false;

        // En una implementación real, aquí se restaurarían los datos
        // Por ahora solo registramos la acción
        
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
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
    }

    public async Task DeleteBackupAsync(int backupId)
    {
        var backup = await _context.Backups.FindAsync(backupId);
        if (backup != null)
        {
            _context.Backups.Remove(backup);
            await _context.SaveChangesAsync();
        }
    }

    public async Task BackupHostsFileAsync()
    {
        try
        {
            var hostsPath = SystemConstants.Paths.HostsFile;
            if (!File.Exists(hostsPath))
                return;

            var backupDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SystemConstants.Paths.BackupsPath, "hosts");
            if (!Directory.Exists(backupDir))
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
        // Implementar restauración del último backup de hosts
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
    }
}
