using AulaSegura.Core.Interfaces;

namespace AulaSegura.Infrastructure.Services;

/// <summary>
/// Prueba de apertura en lectura/escritura sobre <c>%SystemRoot%\System32\drivers\etc\hosts</c>.
/// </summary>
public sealed class HostsFileAccessProbe : IHostsFileAccessProbe
{
    private static readonly string HostsFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.System),
        "drivers", "etc", "hosts");

    public Task<HostsFileWriteAccessResult> ProbeWriteAccessAsync(CancellationToken cancellationToken = default)
    {
        return Task.Run(() => ProbeCore(), cancellationToken);
    }

    private static HostsFileWriteAccessResult ProbeCore()
    {
        try
        {
            if (!File.Exists(HostsFilePath))
            {
                return new HostsFileWriteAccessResult(false,
                    "No se encontró el archivo hosts en la ruta esperada de Windows.");
            }

            using (var fs = new FileStream(HostsFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read))
            {
                // Solo comprobar apertura RW; no escribir contenido.
                _ = fs.Length;
            }

            return new HostsFileWriteAccessResult(true,
                "Puede modificar hosts: los bloqueos y desbloques podrán aplicarse desde esta sesión.");
        }
        catch (UnauthorizedAccessException)
        {
            return new HostsFileWriteAccessResult(false,
                "Acceso denegado. Cierre la app y ejecútela como administrador (clic derecho → Ejecutar como administrador).");
        }
        catch (IOException ex)
        {
            return new HostsFileWriteAccessResult(false,
                $"No se pudo abrir el archivo hosts ({ex.Message}). Compruebe que otro programa no lo tenga bloqueado.");
        }
        catch (Exception ex)
        {
            return new HostsFileWriteAccessResult(false,
                $"Error al comprobar hosts: {ex.Message}");
        }
    }
}
