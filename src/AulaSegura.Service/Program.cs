using AulaSegura.Infrastructure;
using AulaSegura.Service;
using Microsoft.Extensions.Hosting.WindowsServices;
using Serilog;

// Configurar Serilog para logging estructurado
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.File(
        path: "Logs/aulasegura-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Iniciando host de AulaSegura Service...");

    var builder = Host.CreateApplicationBuilder(args);

    // Agregar Serilog
    builder.Services.AddSerilog();

    // Configurar capa de infraestructura (DI, DbContext, Services)
    builder.Services.AddInfrastructure(builder.Configuration);

    // Registrar el worker principal
    builder.Services.AddHostedService<BlockingWorker>();

    // Configurar como Windows Service
    builder.Services.Configure<WindowsServiceLifetimeOptions>(options =>
    {
        options.ServiceName = "AulaSeguraService";
    });

    var host = builder.Build();

    Log.Information("Host configurado correctamente. Iniciando servicio...");
    
    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "El servicio terminó inesperadamente");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
