using System.Text;

namespace AulaSegura.Infrastructure.Services;

/// <summary>
/// Gestiona la lectura, escritura y modificación del archivo hosts de Windows
/// Ubicación: C:\Windows\System32\drivers\etc\hosts
/// </summary>
public class HostsFileManager
{
    private static readonly string HostsFilePath = 
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), 
                     "drivers", "etc", "hosts");

    private const string AULASEGURA_MARKER = "# AULASEGURA-BLOCK";
    private const string BACKUP_EXTENSION = ".aulasegura.bak";

    /// <summary>
    /// Agrega una entrada al archivo hosts apuntando un dominio a una IP específica
    /// </summary>
    /// <param name="domain">Dominio a bloquear (ej: facebook.com)</param>
    /// <param name="ipAddress">IP de destino (default: 127.0.0.1)</param>
    public async Task AddEntryAsync(string domain, string ipAddress = "127.0.0.1")
    {
        if (string.IsNullOrWhiteSpace(domain))
            throw new ArgumentException("El dominio no puede estar vacío", nameof(domain));

        // Normalizar dominio (quitar www. si existe para evitar duplicados)
        var normalizedDomain = NormalizeDomain(domain);
        
        await EnsureBackupAsync();
        
        var lines = await ReadAllLinesAsync();
        var newLines = new List<string>();
        bool entryExists = false;

        foreach (var line in lines)
        {
            // Si la línea ya contiene este dominio, la actualizamos
            if (line.Contains(normalizedDomain, StringComparison.OrdinalIgnoreCase) && 
                line.StartsWith(AULASEGURA_MARKER))
            {
                newLines.Add($"{AULASEGURA_MARKER} {ipAddress} {normalizedDomain}");
                entryExists = true;
            }
            else
            {
                newLines.Add(line);
            }
        }

        // Si no existía, la agregamos al final
        if (!entryExists)
        {
            newLines.Add($"{AULASEGURA_MARKER} {ipAddress} {normalizedDomain}");
        }

        await WriteAllLinesAsync(newLines);
    }

    /// <summary>
    /// Elimina una entrada específica del archivo hosts
    /// </summary>
    /// <param name="domain">Dominio a eliminar</param>
    public async Task RemoveEntryAsync(string domain)
    {
        if (string.IsNullOrWhiteSpace(domain))
            throw new ArgumentException("El dominio no puede estar vacío", nameof(domain));

        var normalizedDomain = NormalizeDomain(domain);
        var lines = await ReadAllLinesAsync();
        var newLines = new List<string>();
        bool removed = false;

        foreach (var line in lines)
        {
            // Solo eliminamos líneas marcadas por AulaSegura que contengan el dominio
            if (line.StartsWith(AULASEGURA_MARKER) && 
                line.Contains(normalizedDomain, StringComparison.OrdinalIgnoreCase))
            {
                removed = true;
                // No agregamos esta línea (la eliminamos)
            }
            else
            {
                newLines.Add(line);
            }
        }

        if (removed)
        {
            await WriteAllLinesAsync(newLines);
        }
    }

    /// <summary>
    /// Elimina TODAS las entradas agregadas por AulaSegura del archivo hosts
    /// </summary>
    public async Task ClearAllAulaSeguraEntriesAsync()
    {
        var lines = await ReadAllLinesAsync();
        var newLines = new List<string>();

        foreach (var line in lines)
        {
            // Solo mantenemos líneas que NO sean de AulaSegura
            if (!line.StartsWith(AULASEGURA_MARKER))
            {
                newLines.Add(line);
            }
        }

        await WriteAllLinesAsync(newLines);
    }

    /// <summary>
    /// Obtiene todas las entradas actuales de AulaSegura en el archivo hosts
    /// </summary>
    /// <returns>Lista de dominios bloqueados actualmente</returns>
    public async Task<List<string>> GetAulaSeguraEntriesAsync()
    {
        var lines = await ReadAllLinesAsync();
        var entries = new List<string>();

        foreach (var line in lines)
        {
            if (line.StartsWith(AULASEGURA_MARKER))
            {
                // Formato: "# AULASEGURA-BLOCK 127.0.0.1 dominio.com"
                var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 3)
                {
                    entries.Add(parts[2]); // El dominio está en la tercera posición
                }
            }
        }

        return entries;
    }

    /// <summary>
    /// Verifica si el archivo hosts ha sido modificado externamente
    /// comparando con el backup más reciente
    /// </summary>
    /// <returns>True si se detectaron modificaciones no autorizadas</returns>
    public async Task<bool> HasBeenModifiedExternallyAsync()
    {
        var backupPath = HostsFilePath + BACKUP_EXTENSION;
        
        if (!File.Exists(backupPath))
            return false; // No hay backup para comparar

        var currentContent = await File.ReadAllTextAsync(HostsFilePath);
        var backupContent = await File.ReadAllTextAsync(backupPath);

        // Extraer solo las líneas de AulaSegura de ambos archivos
        var currentEntries = ExtractAulaSeguraLines(currentContent);
        var backupEntries = ExtractAulaSeguraLines(backupContent);

        // Si las entradas de AulaSegura difieren, alguien modificó el archivo
        return !currentEntries.SequenceEqual(backupEntries);
    }

    /// <summary>
    /// Restaura el archivo hosts desde el backup
    /// </summary>
    public async Task RestoreFromBackupAsync()
    {
        var backupPath = HostsFilePath + BACKUP_EXTENSION;
        
        if (!File.Exists(backupPath))
            throw new InvalidOperationException("No existe backup para restaurar");

        // Copiar backup sobre el archivo actual
        File.Copy(backupPath, HostsFilePath, overwrite: true);
    }

    /// <summary>
    /// Crea un backup del archivo hosts actual antes de modificarlo
    /// </summary>
    private Task EnsureBackupAsync()
    {
        var backupPath = HostsFilePath + BACKUP_EXTENSION;
        
        if (!File.Exists(HostsFilePath))
        {
            // Si no existe el archivo hosts, creamos uno vacío
            File.WriteAllText(HostsFilePath, string.Empty);
        }

        // Crear o actualizar backup
        File.Copy(HostsFilePath, backupPath, overwrite: true);
        
        return Task.CompletedTask;
    }

    /// <summary>
    /// Lee todas las líneas del archivo hosts
    /// </summary>
    private async Task<List<string>> ReadAllLinesAsync()
    {
        if (!File.Exists(HostsFilePath))
            return new List<string>();

        var content = await File.ReadAllTextAsync(HostsFilePath);
        var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        
        return lines;
    }

    /// <summary>
    /// Escribe todas las líneas al archivo hosts
    /// </summary>
    private async Task WriteAllLinesAsync(List<string> lines)
    {
        var content = string.Join(Environment.NewLine, lines) + Environment.NewLine;
        await File.WriteAllTextAsync(HostsFilePath, content, Encoding.UTF8);
    }

    /// <summary>
    /// Normaliza un dominio quitando www. y convirtiendo a minúsculas
    /// </summary>
    private string NormalizeDomain(string domain)
    {
        var normalized = domain.Trim().ToLowerInvariant();
        
        // Quitar www. si existe
        if (normalized.StartsWith("www."))
        {
            normalized = normalized.Substring(4);
        }

        return normalized;
    }

    /// <summary>
    /// Extrae solo las líneas de AulaSegura del contenido del archivo
    /// </summary>
    private List<string> ExtractAulaSeguraLines(string content)
    {
        var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var aulaSeguraLines = new List<string>();

        foreach (var line in lines)
        {
            if (line.StartsWith(AULASEGURA_MARKER))
            {
                aulaSeguraLines.Add(line.Trim());
            }
        }

        return aulaSeguraLines;
    }
}
