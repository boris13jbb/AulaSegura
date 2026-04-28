using AulaSegura.Core.Constants;
using AulaSegura.Core.Entities;
using AulaSegura.Infrastructure.Data;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;

namespace AulaSegura.Infrastructure.Seeding;

/// <summary>
/// Servicio para inicializar la base de datos con datos por defecto
/// </summary>
public static class DatabaseSeeder
{
    /// <summary>
    /// Inicializa la base de datos con datos semilla
    /// </summary>
    public static async Task SeedAsync(AulaSeguraDbContext context)
    {
        // Asegurar que la base de datos existe
        await context.Database.EnsureCreatedAsync();

        // Crear administrador por defecto si no existe
        await SeedAdministratorAsync(context);

        // Crear categorías por defecto si no existen
        await SeedCategoriesAsync(context);

        // Crear configuración inicial si no existe
        await SeedSettingsAsync(context);

        // Crear palabras clave iniciales si no existen
        await SeedKeywordsAsync(context);

        // Crear reglas de bloqueo iniciales si no existen
        await SeedBlockingRulesAsync(context);

        await context.SaveChangesAsync();
    }

    private static async Task SeedAdministratorAsync(AulaSeguraDbContext context)
    {
        if (!await context.Administrators.AnyAsync())
        {
            var adminPassword = "Admin@123"; // Contraseña por defecto (debe cambiarse en primer login)
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(adminPassword, workFactor: 11);

            var admin = new Administrator
            {
                Username = "admin",
                PasswordHash = hashedPassword,
                Email = "admin@aulasegura.local",
                FullName = "Administrador Principal",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                FailedLoginAttempts = 0,
                LockedUntil = null
            };

            await context.Administrators.AddAsync(admin);
        }
    }

    private static async Task SeedCategoriesAsync(AulaSeguraDbContext context)
    {
        if (!await context.Categories.AnyAsync())
        {
            var categories = new[]
            {
                new Category
                {
                    Name = SystemConstants.Categories.Adults,
                    Description = "Sitios de contenido para adultos y pornografía",
                    Color = "#FF0000",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Category
                {
                    Name = SystemConstants.Categories.SocialMedia,
                    Description = "Redes sociales: Facebook, Instagram, TikTok, Twitter, etc.",
                    Color = "#3B5998",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Category
                {
                    Name = SystemConstants.Categories.Gaming,
                    Description = "Sitios de videojuegos online y gaming",
                    Color = "#9B59B6",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Category
                {
                    Name = SystemConstants.Categories.Gambling,
                    Description = "Sitios de apuestas y casinos online",
                    Color = "#E74C3C",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Category
                {
                    Name = SystemConstants.Categories.Entertainment,
                    Description = "Sitios de entretenimiento general",
                    Color = "#F39C12",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Category
                {
                    Name = SystemConstants.Categories.Streaming,
                    Description = "Servicios de streaming de video y música",
                    Color = "#1ABC9C",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Category
                {
                    Name = SystemConstants.Categories.Chat,
                    Description = "Aplicaciones y sitios de chat/mensajería",
                    Color = "#3498DB",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Category
                {
                    Name = SystemConstants.Categories.Custom,
                    Description = "Categoría personalizada para sitios específicos",
                    Color = "#95A5A6",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.Categories.AddRangeAsync(categories);
        }
    }

    private static async Task SeedSettingsAsync(AulaSeguraDbContext context)
    {
        if (!await context.Settings.AnyAsync())
        {
            var settings = new[]
            {
                new Setting
                {
                    Key = SystemConstants.SettingsKeys.InstitutionName,
                    Value = "Mi Institución",
                    Description = "Nombre de la institución o familia",
                    Category = "General",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Setting
                {
                    Key = SystemConstants.SettingsKeys.Mode,
                    Value = "Home", // Home o School
                    Description = "Modo de operación: Home (hogar) o School (escuela)",
                    Category = "General",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Setting
                {
                    Key = SystemConstants.SettingsKeys.BlockAdultContentByDefault,
                    Value = "true",
                    Description = "Bloquear contenido para adultos por defecto",
                    Category = "Blocking",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Setting
                {
                    Key = SystemConstants.SettingsKeys.ProtectionLevel,
                    Value = "High", // Low, Medium, High
                    Description = "Nivel de protección: Low, Medium, High",
                    Category = "Security",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Setting
                {
                    Key = SystemConstants.SettingsKeys.EnableLogging,
                    Value = "true",
                    Description = "Activar registro de logs",
                    Category = "System",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Setting
                {
                    Key = SystemConstants.SettingsKeys.EnableReports,
                    Value = "true",
                    Description = "Activar generación de reportes",
                    Category = "System",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Setting
                {
                    Key = SystemConstants.SettingsKeys.WhitelistPriority,
                    Value = "true",
                    Description = "La lista blanca tiene prioridad sobre la lista negra",
                    Category = "Blocking",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Setting
                {
                    Key = SystemConstants.SettingsKeys.BackupPath,
                    Value = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backups"),
                    Description = "Ruta para respaldos de configuración",
                    Category = "System",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.Settings.AddRangeAsync(settings);
        }
    }

    private static async Task SeedKeywordsAsync(AulaSeguraDbContext context)
    {
        if (!await context.Keywords.AnyAsync())
        {
            var keywords = new[]
            {
                new Keyword { Word = "apuestas", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Keyword { Word = "casino", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Keyword { Word = "porno", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Keyword { Word = "xxx", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Keyword { Word = "juegos online", IsActive = true, CreatedAt = DateTime.UtcNow }
            };
            await context.Keywords.AddRangeAsync(keywords);
        }
    }

    private static async Task SeedBlockingRulesAsync(AulaSeguraDbContext context)
    {
        if (!await context.BlockingRules.AnyAsync())
        {
            var rules = new[]
            {
                new BlockingRule 
                { 
                    Name = "Bloqueo por Horario Escolar", 
                    RuleType = "TIME", 
                    Value = "{\"start\": \"08:00\", \"end\": \"15:00\"}", 
                    IsActive = true, 
                    Action = "Block",
                    CreatedAt = DateTime.UtcNow 
                },
                new BlockingRule 
                { 
                    Name = "Bloqueo de IPs Locales", 
                    RuleType = "IP", 
                    Value = "192.168.1.0/24", 
                    IsActive = false, 
                    Action = "Log",
                    CreatedAt = DateTime.UtcNow 
                }
            };
            await context.BlockingRules.AddRangeAsync(rules);
        }
    }
}
