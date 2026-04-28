namespace AulaSegura.Core.Constants;

/// <summary>
/// Constantes del sistema
/// </summary>
public static class SystemConstants
{
    // Configuración de seguridad
    public const int MaxFailedLoginAttempts = 5;
    public const int AccountLockoutDurationMinutes = 30;
    public const int PasswordMinLength = 8;
    
    // Categorías predefinidas
    public static class Categories
    {
        public const string Adults = "Adultos";
        public const string SocialMedia = "Redes Sociales";
        public const string Entertainment = "Entretenimiento";
        public const string Gaming = "Videojuegos";
        public const string Gambling = "Apuestas";
        public const string Streaming = "Streaming";
        public const string Chat = "Chat";
        public const string Custom = "Personalizado";
    }
    
    // Claves de configuración
    public static class SettingsKeys
    {
        public const string InstitutionName = "InstitutionName";
        public const string Mode = "Mode"; // School or Home
        public const string BlockAdultContentByDefault = "BlockAdultContentByDefault";
        public const string ProtectionLevel = "ProtectionLevel";
        public const string BackupPath = "BackupPath";
        public const string EnableLogging = "EnableLogging";
        public const string EnableReports = "EnableReports";
        public const string WhitelistPriority = "WhitelistPriority";
    }
    
    // Tipos de acción para logs
    public static class LogActions
    {
        public const string Login = "LOGIN";
        public const string Logout = "LOGOUT";
        public const string LoginFailed = "LOGIN_FAILED";
        public const string AddBlockedSite = "ADD_BLOCKED_SITE";
        public const string UpdateBlockedSite = "UPDATE_BLOCKED_SITE";
        public const string DeleteBlockedSite = "DELETE_BLOCKED_SITE";
        public const string AddAllowedSite = "ADD_ALLOWED_SITE";
        public const string UpdateAllowedSite = "UPDATE_ALLOWED_SITE";
        public const string DeleteAllowedSite = "DELETE_ALLOWED_SITE";
        public const string CreateCategory = "CREATE_CATEGORY";
        public const string UpdateCategory = "UPDATE_CATEGORY";
        public const string DeleteCategory = "DELETE_CATEGORY";
        public const string ChangePassword = "CHANGE_PASSWORD";
        public const string ChangeSettings = "CHANGE_SETTINGS";
        public const string ApplyBlockingRules = "APPLY_BLOCKING_RULES";
        public const string BackupCreated = "BACKUP_CREATED";
        public const string BackupRestored = "BACKUP_RESTORED";
        public const string ServiceStarted = "SERVICE_STARTED";
        public const string ServiceStopped = "SERVICE_STOPPED";
    }
    
    // Rutas del sistema
    public static class Paths
    {
        public const string HostsFile = @"C:\Windows\System32\drivers\etc\hosts";
        public const string DatabasePath = @"Data\aulasegura.db";
        public const string LogsPath = @"Logs";
        public const string BackupsPath = @"Backups";
    }
}
