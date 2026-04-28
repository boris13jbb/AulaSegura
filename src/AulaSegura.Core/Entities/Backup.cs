namespace AulaSegura.Core.Entities;

/// <summary>
/// Registro de respaldo de configuración
/// </summary>
public class Backup : BaseEntity
{
    public string BackupPath { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string BackupType { get; set; } = "Full"; // Full, Config, Database, Hosts
    public long SizeInBytes { get; set; }
    public int? CreatedByAdminId { get; set; }
    public Administrator? CreatedByAdmin { get; set; }
}
