using AulaSegura.Core.Constants;
using AulaSegura.Core.Interfaces;
using AulaSegura.Infrastructure;

namespace AulaSegura.Service;

/// <summary>
/// Worker principal del servicio Windows que monitorea y aplica reglas de bloqueo
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
        _logger.LogInformation("=== AulaSegura Control Web Service Iniciado ===");
        _logger.LogInformation("Versión: 1.0.0");
        _logger.LogInformation("Framework: .NET {Version}", Environment.Version);

        try
        {
            // Inicializar base de datos con seed data
            await InitializeDatabaseAsync();

            using var scope = _serviceProvider.CreateScope();
            var blockedSiteService = scope.ServiceProvider.GetRequiredService<IBlockedSiteService>();
            var activityLogService = scope.ServiceProvider.GetRequiredService<IActivityLogService>();
            var settingsService = scope.ServiceProvider.GetRequiredService<ISettingsService>();
            var reportService = scope.ServiceProvider.GetRequiredService<IReportService>();

            // Registrar inicio del servicio
            await activityLogService.LogActivityAsync(
                SystemConstants.LogActions.ServiceStarted,
                "Servicio de bloqueo web iniciado correctamente",
                "System",
                null,
                null,
                true);

            _logger.LogInformation("Base de datos inicializada correctamente");

            // Iniciar servidor de página de bloqueo
            await StartBlockingPageServerAsync();

            // Aplicar reglas de bloqueo iniciales
            _logger.LogInformation("Aplicando reglas de bloqueo iniciales...");
            await blockedSiteService.ApplyBlockingRulesAsync();
            _logger.LogInformation("Reglas de bloqueo aplicadas exitosamente");

            // Obtener intervalo de verificación desde configuración (default: 60 segundos)
            var checkIntervalSeconds = _configuration.GetValue<int>("AppSettings:CheckBlockingRulesIntervalSeconds", 60);
            _logger.LogInformation("Intervalo de verificación: {Interval} segundos", checkIntervalSeconds);

            // Ciclo principal de monitoreo
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Verificar y aplicar cambios en las reglas de bloqueo
                    await blockedSiteService.ApplyBlockingRulesAsync();
                    
                    // Mantenimiento: Limpiar reportes antiguos (cada 24 horas aprox)
                    if (DateTime.Now.Hour == 3 && DateTime.Now.Minute < 10)
                    {
                        await reportService.DeleteOldReportsAsync(90);
                        _logger.LogInformation("Mantenimiento de reportes completado.");
                    }
                    
                    _logger.LogDebug("Reglas de bloqueo verificadas y aplicadas - {Time}", DateTime.Now);
                    
                    // Esperar el intervalo configurado
                    await Task.Delay(TimeSpan.FromSeconds(checkIntervalSeconds), stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    // Cancelación normal del servicio
                    _logger.LogInformation("Servicio detenido por solicitud de cancelación");
                    break;
                }
                catch (UnauthorizedAccessException ex)
                {
                    _logger.LogError(ex, "Error de permisos al aplicar reglas de bloqueo. Verifique que el servicio se ejecute como administrador.");
                    
                    await activityLogService.LogActivityAsync(
                        "BLOCKING_ERROR",
                        $"Error de permisos: {ex.Message}",
                        "System",
                        null,
                        null,
                        false);

                    // Esperar antes de reintentar
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error inesperado en el ciclo de bloqueo");
                    
                    await activityLogService.LogActivityAsync(
                        "BLOCKING_ERROR",
                        $"Error en ciclo de bloqueo: {ex.Message}",
                        "System",
                        null,
                        null,
                        false);

                    // Esperar antes de reintentar para evitar loops de error
                    await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                }
            }

            // Registrar detención del servicio
            await activityLogService.LogActivityAsync(
                SystemConstants.LogActions.ServiceStopped,
                "Servicio de bloqueo web detenido",
                "System",
                null,
                null,
                true);

            // Detener servidor de página de bloqueo
            StopBlockingPageServer();

            _logger.LogInformation("=== AulaSegura Control Web Service Detenido ===");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Error crítico en el servicio de bloqueo");
            throw;
        }
    }

    /// <summary>
    /// Inicializa la base de datos y crea datos semilla
    /// </summary>
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

    /// <summary>
    /// Inicia el servidor HTTP de página de bloqueo
    /// </summary>
    private async Task StartBlockingPageServerAsync()
    {
        try
        {
            var blockingPagePath = Path.Combine(AppContext.BaseDirectory, "blocking-page.html");
            
            if (!File.Exists(blockingPagePath))
            {
                _logger.LogWarning("Archivo de página de bloqueo no encontrado en: {Path}", blockingPagePath);
                _logger.LogWarning("El servidor de bloqueo no se iniciará. Los usuarios no verán página de bloqueo personalizada.");
                return;
            }

            var port = _configuration.GetValue<int>("AppSettings:BlockingPagePort", 8080);
            _blockingPageServer = new BlockingPageServer(
                _loggerFactory.CreateLogger<BlockingPageServer>(),
                blockingPagePath,
                port);

            await _blockingPageServer.StartAsync();
            _logger.LogInformation("Servidor de página de bloqueo iniciado en puerto {Port}", port);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al iniciar servidor de página de bloqueo");
            // No lanzar excepción para no detener el servicio completo
        }
    }

    /// <summary>
    /// Detiene el servidor HTTP de página de bloqueo
    /// </summary>
    private void StopBlockingPageServer()
    {
        try
        {
            _blockingPageServer?.Stop();
            _blockingPageServer?.Dispose();
            _logger.LogInformation("Servidor de página de bloqueo detenido");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al detener servidor de página de bloqueo");
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
