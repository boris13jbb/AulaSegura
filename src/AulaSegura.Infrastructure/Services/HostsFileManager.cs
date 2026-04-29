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

    /// <summary>
    /// Sufijo en la misma línea (tras IP y host). En Windows, una línea que empieza por # es comentario completo y no bloquea; el marcador va al final.
    /// </summary>
    private const string AulaseguraLineSuffix = " # AULASEGURA-BLOCK";

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
            if (IsAulaSeguraManagedLine(line) &&
                TryGetDomainFromManagedLine(line, out var d) &&
                string.Equals(d, normalizedDomain, StringComparison.OrdinalIgnoreCase))
            {
                newLines.Add(BuildBlockLine(ipAddress, normalizedDomain));
                entryExists = true;
            }
            else
            {
                newLines.Add(line);
            }
        }

        if (!entryExists)
        {
            newLines.Add(BuildBlockLine(ipAddress, normalizedDomain));
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
            if (IsAulaSeguraManagedLine(line) &&
                TryGetDomainFromManagedLine(line, out var d) &&
                string.Equals(d, normalizedDomain, StringComparison.OrdinalIgnoreCase))
            {
                removed = true;
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
            if (!IsAulaSeguraManagedLine(line))
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
            if (IsAulaSeguraManagedLine(line) &&
                TryGetDomainFromManagedLine(line, out var d) &&
                !string.IsNullOrEmpty(d))
            {
                entries.Add(d);
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
            if (IsAulaSeguraManagedLine(line))
            {
                aulaSeguraLines.Add(line.Trim());
            }
        }

        return aulaSeguraLines;
    }

    private static string BuildBlockLine(string ipAddress, string normalizedDomain)
        => $"{ipAddress}\t{normalizedDomain}{AulaseguraLineSuffix}";

    /// <summary>Detecta líneas nuevas (sufijo al final) o el formato antiguo erróneo (# al inicio, ignorado por Windows).</summary>
    private static bool IsAulaSeguraManagedLine(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
            return false;

        var t = line.Trim();
        if (t.EndsWith(AulaseguraLineSuffix, StringComparison.OrdinalIgnoreCase))
            return true;

        if (t.StartsWith("# AULASEGURA-BLOCK", StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    }

    private static bool TryGetDomainFromManagedLine(string line, out string? domain)
    {
        domain = null;
        var t = line.Trim();

        if (t.EndsWith(AulaseguraLineSuffix, StringComparison.OrdinalIgnoreCase))
        {
            var head = t[..^AulaseguraLineSuffix.Length].TrimEnd();
            var parts = head.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
            {
                domain = parts[1];
                return true;
            }

            return false;
        }

        const string legacyPrefix = "# AULASEGURA-BLOCK ";
        if (!t.StartsWith(legacyPrefix, StringComparison.OrdinalIgnoreCase))
            return false;

        var rest = t[legacyPrefix.Length..].Trim();
        var legacyParts = rest.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        if (legacyParts.Length >= 2)
        {
            domain = legacyParts[1];
            return true;
        }

        return false;
    }
}
