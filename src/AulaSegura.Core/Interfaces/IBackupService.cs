namespace AulaSegura.Core.Interfaces;

/// <summary>
/// Servicio de gestión de respaldos y restauración
/// </summary>
public interface IBackupService
{
    Task<string> CreateBackupAsync(string description, string backupType, int? adminId);
    Task<bool> RestoreBackupAsync(int backupId);
    Task<IEnumerable<AulaSegura.Core.Entities.Backup>> GetBackupsAsync();
    Task DeleteBackupAsync(int backupId);
    Task BackupHostsFileAsync();
    Task RestoreHostsFileAsync();
}
