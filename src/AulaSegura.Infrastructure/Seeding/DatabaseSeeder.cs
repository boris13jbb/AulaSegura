using AulaSegura.Core.Constants;
using AulaSegura.Core.Entities;
using AulaSegura.Core.Utilities;
using AulaSegura.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;

namespace AulaSegura.Infrastructure.Seeding;

/// <summary>
/// Inicializa la base de datos con datos minimos para operar.
/// </summary>
public static class DatabaseSeeder
{
    public static async Task SeedAsync(AulaSeguraDbContext context, IConfiguration configuration)
    {
        await context.Database.EnsureCreatedAsync();

        await SeedAdministratorAsync(context, configuration);
        await SeedCategoriesAsync(context);
        await SeedSettingsAsync(context);
        await SeedKeywordsAsync(context);
        await SeedBlockingRulesAsync(context);

        await context.SaveChangesAsync();
    }

    private static async Task SeedAdministratorAsync(AulaSeguraDbContext context, IConfiguration configuration)
    {
        if (await context.Administrators.AnyAsync())
            return;

        var username = configuration["SeedAdmin:Username"] ?? "admin";
        var adminPassword = configuration["SeedAdmin:Password"];
        var passwordWasGenerated = false;

        if (string.IsNullOrWhiteSpace(adminPassword))
        {
            adminPassword = GenerateTemporaryPassword();
            passwordWasGenerated = true;
        }

        var passwordValidation = ValidationHelper.ValidatePassword(adminPassword);
        if (!passwordValidation.IsValid)
        {
            throw new InvalidOperationException(
                $"SeedAdmin:Password no cumple la politica de contrasenas: {passwordValidation.Message}");
        }

        var admin = new Administrator
        {
            Username = username.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword, workFactor: 11),
            Email = configuration["SeedAdmin:Email"] ?? "admin@aulasegura.local",
            FullName = configuration["SeedAdmin:FullName"] ?? "Administrador Principal",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            FailedLoginAttempts = 0,
            LockedUntil = null
        };

        await context.Administrators.AddAsync(admin);

        if (passwordWasGenerated)
        {
            await WriteFirstRunCredentialsAsync(admin.Username, adminPassword);
        }
    }

    private static string GenerateTemporaryPassword()
    {
        return $"As-{Convert.ToHexString(RandomNumberGenerator.GetBytes(8))}-a1";
    }

    private static async Task WriteFirstRunCredentialsAsync(string username, string password)
    {
        var credentialsPath = Path.Combine(AppContext.BaseDirectory, "first-run-admin.txt");
        var content = string.Join(
            Environment.NewLine,
            "AulaSegura - credenciales temporales de primer inicio",
            "Cambie esta contrasena despues del primer inicio y elimine este archivo.",
            string.Empty,
            $"Usuario: {username}",
            $"Contrasena temporal: {password}",
            string.Empty);

        await File.WriteAllTextAsync(credentialsPath, content);
    }

    private static async Task SeedCategoriesAsync(AulaSeguraDbContext context)
    {
        if (await context.Categories.AnyAsync())
            return;

        var categories = new[]
        {
            new Category
            {
                Name = SystemConstants.Categories.Adults,
                Description = "Sitios de contenido para adultos",
                Color = "#FF0000",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Category
            {
                Name = SystemConstants.Categories.SocialMedia,
                Description = "Redes sociales: Facebook, Instagram, TikTok, X, etc.",
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
                Description = "Servicios de streaming de video y musica",
                Color = "#1ABC9C",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Category
            {
                Name = SystemConstants.Categories.Chat,
                Description = "Aplicaciones y sitios de chat/mensajeria",
                Color = "#3498DB",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Category
            {
                Name = SystemConstants.Categories.Custom,
                Description = "Categoria personalizada para sitios especificos",
                Color = "#95A5A6",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        await context.Categories.AddRangeAsync(categories);
    }

    private static async Task SeedSettingsAsync(AulaSeguraDbContext context)
    {
        if (await context.Settings.AnyAsync())
            return;

        var settings = new[]
        {
            new Setting
            {
                Key = SystemConstants.SettingsKeys.InstitutionName,
                Value = "Mi Institucion",
                Description = "Nombre de la institucion o familia",
                Category = "General",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Setting
            {
                Key = SystemConstants.SettingsKeys.Mode,
                Value = "Home",
                Description = "Modo de operacion: Home (hogar) o School (escuela)",
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
                Value = "High",
                Description = "Nivel de proteccion: Low, Medium, High",
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
                Description = "Activar generacion de reportes",
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
                Description = "Ruta para respaldos de configuracion",
                Category = "System",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        await context.Settings.AddRangeAsync(settings);
    }

    private static async Task SeedKeywordsAsync(AulaSeguraDbContext context)
    {
        if (await context.Keywords.AnyAsync())
            return;

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

    private static async Task SeedBlockingRulesAsync(AulaSeguraDbContext context)
    {
        if (await context.BlockingRules.AnyAsync())
            return;

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
