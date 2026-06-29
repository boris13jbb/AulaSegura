using AulaSegura.Core.Constants;
using AulaSegura.Core.Interfaces;
using AulaSegura.Infrastructure;

namespace AulaSegura.Service;

/// <summary>
/// Worker principal del servicio Windows que monitorea y aplica reglas de bloqueo.
/// </summary>
public class BlockingWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BlockingWorker> _logger;
    private readonly IConfiguration _configuration;
    private readonly ILoggerFactory _loggerFactory;
    private BlockingPageServer? _blockingPageServer;

    public BlockingWorker(
        IServiceProvider serviceProvider,
        ILogger<BlockingWorker> logger,
        IConfiguration configuration,
        ILoggerFactory loggerFactory)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _configuration = configuration;
        _loggerFactory = loggerFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("=== AulaSegura Control Web Service iniciado ===");
        _logger.LogInformation("Version: 1.0.0");
        _logger.LogInformation("Framework: .NET {Version}", Environment.Version);

        try
        {
            await InitializeDatabaseAsync();

            await LogSystemActivityAsync(
                SystemConstants.LogActions.ServiceStarted,
                "Servicio de bloqueo web iniciado correctamente",
                true);

            await StartBlockingPageServerAsync();

            _logger.LogInformation("Aplicando reglas de bloqueo iniciales...");
            await ApplyBlockingRulesOnceAsync();
            _logger.LogInformation("Reglas de bloqueo aplicadas exitosamente");

            var checkIntervalSeconds = Math.Max(
                5,
                _configuration.GetValue("AppSettings:CheckBlockingRulesIntervalSeconds", 60));
            _logger.LogInformation("Intervalo de verificacion: {Interval} segundos", checkIntervalSeconds);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ApplyBlockingRulesOnceAsync();

                    if (DateTime.Now.Hour == 3 && DateTime.Now.Minute < 10)
                    {
                        await DeleteOldReportsAsync(90);
                        _logger.LogInformation("Mantenimiento de reportes completado.");
                    }

                    _logger.LogDebug("Reglas de bloqueo verificadas y aplicadas - {Time}", DateTime.Now);

                    await Task.Delay(TimeSpan.FromSeconds(checkIntervalSeconds), stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Servicio detenido por solicitud de cancelacion");
                    break;
                }
                catch (UnauthorizedAccessException ex)
                {
                    _logger.LogError(ex, "Error de permisos al aplicar reglas de bloqueo.");
                    await LogBlockingErrorAsync("Error de permisos al aplicar reglas de bloqueo.");
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error inesperado en el ciclo de bloqueo");
                    await LogBlockingErrorAsync("Error inesperado en el ciclo de bloqueo.");
                    await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                }
            }

            await LogSystemActivityAsync(
                SystemConstants.LogActions.ServiceStopped,
                "Servicio de bloqueo web detenido",
                true);

            StopBlockingPageServer();

            _logger.LogInformation("=== AulaSegura Control Web Service detenido ===");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Error critico en el servicio de bloqueo");
            throw;
        }
    }

    private async Task InitializeDatabaseAsync()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            await scope.ServiceProvider.InitializeDatabaseAsync();
            _logger.LogInformation("Base de datos inicializada con datos semilla");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al inicializar la base de datos");
            throw;
        }
    }

    private async Task ApplyBlockingRulesOnceAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var blockedSiteService = scope.ServiceProvider.GetRequiredService<IBlockedSiteService>();
        await blockedSiteService.ApplyBlockingRulesAsync();
    }

    private async Task DeleteOldReportsAsync(int retentionDays)
    {
        using var scope = _serviceProvider.CreateScope();
        var reportService = scope.ServiceProvider.GetRequiredService<IReportService>();
        await reportService.DeleteOldReportsAsync(retentionDays);
    }

    private async Task LogSystemActivityAsync(string action, string description, bool success)
    {
        using var scope = _serviceProvider.CreateScope();
        var activityLogService = scope.ServiceProvider.GetRequiredService<IActivityLogService>();
        await activityLogService.LogActivityAsync(action, description, "System", null, null, success);
    }

    private async Task LogBlockingErrorAsync(string description)
    {
        await LogSystemActivityAsync("BLOCKING_ERROR", description, false);
    }

    private async Task StartBlockingPageServerAsync()
    {
        try
        {
            var blockingPagePath = Path.Combine(AppContext.BaseDirectory, "blocking-page.html");

            if (!File.Exists(blockingPagePath))
            {
                _logger.LogWarning("Archivo de pagina de bloqueo no encontrado en: {Path}", blockingPagePath);
                _logger.LogWarning("El servidor de bloqueo no se iniciara.");
                return;
            }

            var port = _configuration.GetValue("AppSettings:BlockingPagePort", 8080);
            _blockingPageServer = new BlockingPageServer(
                _loggerFactory.CreateLogger<BlockingPageServer>(),
                blockingPagePath,
                port);

            await _blockingPageServer.StartAsync();
            _logger.LogInformation("Servidor de pagina de bloqueo iniciado en puerto {Port}", port);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al iniciar servidor de pagina de bloqueo");
        }
    }

    private void StopBlockingPageServer()
    {
        try
        {
            _blockingPageServer?.Stop();
            _blockingPageServer?.Dispose();
            _logger.LogInformation("Servidor de pagina de bloqueo detenido");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al detener servidor de pagina de bloqueo");
        }
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando servicio AulaSegura...");
        await base.StartAsync(cancellationToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deteniendo servicio AulaSegura...");
        await base.StopAsync(cancellationToken);
    }
}
