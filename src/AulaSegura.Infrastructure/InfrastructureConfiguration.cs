using AulaSegura.Core.Interfaces;
using AulaSegura.Infrastructure.Data;
using AulaSegura.Infrastructure.Repositories;
using AulaSegura.Infrastructure.Seeding;
using AulaSegura.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using System.Data.Common;

namespace AulaSegura.Infrastructure;

/// <summary>
/// Configura la capa de infraestructura: base de datos, repositorios y servicios.
/// </summary>
public static class InfrastructureConfiguration
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=%PROGRAMDATA%\\AulaSegura\\aulasegura.db";
        connectionString = NormalizeSqliteConnectionString(connectionString);

        services.AddDbContext<AulaSeguraDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

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
    /// Inicializa la base de datos y crea datos semilla sin borrar datos existentes por defecto.
    /// </summary>
    public static async Task InitializeDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AulaSeguraDbContext>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        var databaseCreated = await context.Database.EnsureCreatedAsync();

        if (!databaseCreated && await IsSchemaIncompleteAsync(context))
        {
            var allowRecreate = bool.TryParse(
                configuration["Database:RecreateWhenSchemaIsIncomplete"],
                out var recreate) && recreate;
            if (!allowRecreate)
            {
                throw new InvalidOperationException(
                    "La base de datos existe pero su esquema esta incompleto. " +
                    "Haga un respaldo y habilite Database:RecreateWhenSchemaIsIncomplete solo si acepta recrearla.");
            }

            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();
        }

        await DatabaseSeeder.SeedAsync(context, configuration);
    }

    private static async Task<bool> IsSchemaIncompleteAsync(AulaSeguraDbContext context)
    {
        var requiredTables = new[]
        {
            "Administrators",
            "Categories",
            "BlockedSites",
            "AllowedSites",
            "Schedules",
            "ActivityLogs",
            "Settings",
            "Backups",
            "Keywords",
            "BlockingRules",
            "Reports"
        };

        foreach (var table in requiredTables)
        {
            if (!await TableExistsAsync(context, table))
                return true;
        }

        return false;
    }

    private static async Task<bool> TableExistsAsync(AulaSeguraDbContext context, string tableName)
    {
        var connection = context.Database.GetDbConnection();
        var shouldClose = connection.State == ConnectionState.Closed;

        if (shouldClose)
            await connection.OpenAsync();

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = @tableName;";

            var parameter = command.CreateParameter();
            parameter.ParameterName = "@tableName";
            parameter.Value = tableName;
            command.Parameters.Add(parameter);

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result) > 0;
        }
        finally
        {
            if (shouldClose)
                await connection.CloseAsync();
        }
    }

    private static string NormalizeSqliteConnectionString(string connectionString)
    {
        var builder = new DbConnectionStringBuilder
        {
            ConnectionString = connectionString
        };

        if (!builder.TryGetValue("Data Source", out var rawDataSource))
            return Environment.ExpandEnvironmentVariables(connectionString);

        var dataSource = Environment.ExpandEnvironmentVariables(Convert.ToString(rawDataSource) ?? string.Empty);
        if (string.IsNullOrWhiteSpace(dataSource) || dataSource.Equals(":memory:", StringComparison.OrdinalIgnoreCase))
            return builder.ConnectionString;

        if (!Path.IsPathRooted(dataSource))
            dataSource = Path.Combine(AppContext.BaseDirectory, dataSource);

        var directory = Path.GetDirectoryName(dataSource);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        builder["Data Source"] = dataSource;
        return builder.ConnectionString;
    }
}
