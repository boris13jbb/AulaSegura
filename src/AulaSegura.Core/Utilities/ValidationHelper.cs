using System.Text.RegularExpressions;

namespace AulaSegura.Core.Utilities;

/// <summary>
/// Utilidades para validación y manipulación de datos
/// </summary>
public static class ValidationHelper
{
    /// <summary>
    /// Valida si un dominio tiene un formato válido
    /// </summary>
    public static bool IsValidDomain(string domain)
    {
        if (string.IsNullOrWhiteSpace(domain))
            return false;

        // Eliminar protocolo si existe
        domain = domain.Replace("http://", "").Replace("https://", "").Replace("www.", "");
        
        // Eliminar path si existe
        var pathIndex = domain.IndexOf('/');
        if (pathIndex > 0)
            domain = domain.Substring(0, pathIndex);

        // Patrón básico de validación de dominio
        var pattern = @"^([a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?\.)+[a-zA-Z]{2,}$";
        return Regex.IsMatch(domain, pattern);
    }

    /// <summary>
    /// Normaliza un dominio (minúsculas, sin espacios, sin protocolo)
    /// </summary>
    public static string NormalizeDomain(string domain)
    {
        if (string.IsNullOrWhiteSpace(domain))
            return string.Empty;

        domain = domain.Trim().ToLowerInvariant();
        domain = domain.Replace("http://", "").Replace("https://", "").Replace("www.", "");
        
        var pathIndex = domain.IndexOf('/');
        if (pathIndex > 0)
            domain = domain.Substring(0, pathIndex);

        return domain;
    }

    /// <summary>
    /// Valida fortaleza de contraseña
    /// </summary>
    public static (bool IsValid, string Message) ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return (false, "La contraseña no puede estar vacía");

        if (password.Length < 8)
            return (false, "La contraseña debe tener al menos 8 caracteres");

        if (!Regex.IsMatch(password, @"[A-Z]"))
            return (false, "La contraseña debe contener al menos una letra mayúscula");

        if (!Regex.IsMatch(password, @"[a-z]"))
            return (false, "La contraseña debe contener al menos una letra minúscula");

        if (!Regex.IsMatch(password, @"\d"))
            return (false, "La contraseña debe contener al menos un número");

        return (true, "Contraseña válida");
    }
}
