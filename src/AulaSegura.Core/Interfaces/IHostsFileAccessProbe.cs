namespace AulaSegura.Core.Interfaces;

/// <summary>Resultado de comprobar si el proceso actual puede modificar el archivo hosts.</summary>
public sealed record HostsFileWriteAccessResult(bool CanWrite, string DetailMessage);

/// <summary>
/// Comprueba permisos de escritura sobre el archivo hosts del sistema (sin modificarlo).
/// </summary>
public interface IHostsFileAccessProbe
{
    Task<HostsFileWriteAccessResult> ProbeWriteAccessAsync(CancellationToken cancellationToken = default);
}
