using AulaSegura.Core.Interfaces;
using AulaSegura.Infrastructure.Data;
using AulaSegura.Infrastructure.Repositories;
using AulaSegura.Infrastructure.Services;
using AulaSegura.Infrastructure.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AulaSegura.Infrastructure;

/// <summary>
/// Extensión para configurar la capa de infraestructura
/// </summary>
public static class InfrastructureConfiguration
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Configurar DbContext con SQLite
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Data Source=aulasegura.db";
        
        services.AddDbContext<AulaSeguraDbContext>(options =>
            options.UseSqlite(connectionString));

        // Registrar repositorios
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        // Registrar servicios
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IBlockedSiteService, BlockedSiteService>();
        services.AddScoped<IAllowedSiteService, AllowedSiteService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IScheduleService, ScheduleService>();
        services.AddScoped<IActivityLogService, ActivityLogService>();
        services.AddScoped<ISettingsService, SettingsService>();
        services.AddScoped<IBackupService, BackupService>();
        services.AddScoped<IKeywordService, KeywordService>();
        services.AddScoped<IBlockingRuleService, BlockingRuleService>();
        services.AddScoped<IReportService, ReportService>();

        services.AddSingleton<IHostsFileAccessProbe, HostsFileAccessProbe>();

        return services;
    }

    /// <summary>
    /// Inicializa la base de datos y crea datos semilla
    /// </summary>
    public static async Task InitializeDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AulaSeguraDbContext>();
        
        bool needsRecreation = false;
        
        // Verificar si la base de datos existe
        var databaseExists = await context.Database.EnsureCreatedAsync();
        
        if (!databaseExists)
        {
            // La base de datos ya existía, verificar que todas las tablas estén presentes
            try
            {
                // Verificar si las tablas nuevas existen
                var keywordsExist = await context.Database.ExecuteSqlRawAsync(
                    "SELECT count(*) FROM sqlite_master WHERE type='table' AND name='Keywords';") > 0;
                var blockingRulesExist = await context.Database.ExecuteSqlRawAsync(
                    "SELECT count(*) FROM sqlite_master WHERE type='table' AND name='BlockingRules';") > 0;
                var reportsExist = await context.Database.ExecuteSqlRawAsync(
                    "SELECT count(*) FROM sqlite_master WHERE type='table' AND name='Reports';") > 0;
                
                // Si falta alguna tabla, marcar para recrear
                if (!keywordsExist || !blockingRulesExist || !reportsExist)
                {
                    needsRecreation = true;
                }
            }
            catch
            {
                // Si hay error al verificar, recrear la base de datos
                needsRecreation = true;
            }
        }
        
        // Recrear la base de datos si es necesario
        if (needsRecreation)
        {
            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();
        }
        
        // Crear datos semilla
        await DatabaseSeeder.SeedAsync(context);
    }
}
