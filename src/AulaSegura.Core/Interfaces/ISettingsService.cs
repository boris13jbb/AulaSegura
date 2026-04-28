namespace AulaSegura.Core.Interfaces;

/// <summary>
/// Servicio de gestión de configuración del sistema
/// </summary>
public interface ISettingsService
{
    Task<string> GetSettingAsync(string key);
    Task SetSettingAsync(string key, string value, string description = "");
    Task<Dictionary<string, string>> GetAllSettingsAsync();
    Task<bool> GetBoolSettingAsync(string key, bool defaultValue = false);
    Task<int> GetIntSettingAsync(string key, int defaultValue = 0);
}
