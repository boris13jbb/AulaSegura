using System.Text.RegularExpressions;

namespace AulaSegura.Core.Utilities;

/// <summary>
/// Utilidades para validacion y normalizacion de datos de entrada.
/// </summary>
public static class ValidationHelper
{
    /// <summary>
    /// Valida si un dominio tiene un formato valido.
    /// </summary>
    public static bool IsValidDomain(string domain)
    {
        var normalizedDomain = NormalizeDomain(domain);

        if (string.IsNullOrWhiteSpace(normalizedDomain) || normalizedDomain.Length > 253)
            return false;

        var labels = normalizedDomain.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (labels.Length < 2)
            return false;

        foreach (var label in labels)
        {
            if (label.Length is < 1 or > 63)
                return false;

            if (label.StartsWith('-') || label.EndsWith('-'))
                return false;

            if (!Regex.IsMatch(label, "^[a-z0-9-]+$", RegexOptions.IgnoreCase))
                return false;
        }

        return !labels[^1].All(char.IsDigit);
    }

    /// <summary>
    /// Normaliza un dominio: minusculas, sin protocolo, sin puerto, sin ruta y sin www inicial.
    /// </summary>
    public static string NormalizeDomain(string domain)
    {
        if (string.IsNullOrWhiteSpace(domain))
            return string.Empty;

        var candidate = domain.Trim();

        if (!candidate.Contains("://", StringComparison.Ordinal))
        {
            candidate = $"http://{candidate}";
        }

        if (Uri.TryCreate(candidate, UriKind.Absolute, out var uri) && !string.IsNullOrWhiteSpace(uri.Host))
        {
            candidate = uri.Host;
        }
        else
        {
            var endIndex = candidate.IndexOfAny(['/', '?', '#']);
            if (endIndex >= 0)
                candidate = candidate[..endIndex];

            var portIndex = candidate.LastIndexOf(':');
            if (portIndex > -1)
                candidate = candidate[..portIndex];
        }

        candidate = candidate.Trim().Trim('.').ToLowerInvariant();

        if (candidate.StartsWith("www.", StringComparison.OrdinalIgnoreCase))
            candidate = candidate[4..];

        return candidate;
    }

    /// <summary>
    /// Valida la fortaleza minima de una contrasena.
    /// </summary>
    public static (bool IsValid, string Message) ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return (false, "La contrasena no puede estar vacia");

        if (password.Length < 8)
            return (false, "La contrasena debe tener al menos 8 caracteres");

        if (!Regex.IsMatch(password, @"[A-Z]"))
            return (false, "La contrasena debe contener al menos una letra mayuscula");

        if (!Regex.IsMatch(password, @"[a-z]"))
            return (false, "La contrasena debe contener al menos una letra minuscula");

        if (!Regex.IsMatch(password, @"\d"))
            return (false, "La contrasena debe contener al menos un numero");

        return (true, "Contrasena valida");
    }
}
