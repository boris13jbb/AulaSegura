using System.IO;
using System.Windows;
using System.Runtime.InteropServices;
using AulaSegura.App.ViewModels;
using AulaSegura.App.Views;
using AulaSegura.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AulaSegura.App;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private readonly IHost _host;

    // Importar API de Windows para traer la ventana al frente
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    private const int SW_RESTORE = 9;

    public App()
    {
        // Configurar host de .NET con DI
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Configurar infraestructura (DbContext, Services)
                services.AddInfrastructure(context.Configuration);

                // Registrar ViewModels
                services.AddTransient<LoginViewModel>();
                services.AddTransient<BlockedSitesViewModel>();
                services.AddTransient<AllowedSitesViewModel>();
                services.AddTransient<CategoriesViewModel>();
                services.AddTransient<SchedulesViewModel>();
                services.AddTransient<SettingsViewModel>();
                services.AddTransient<KeywordsViewModel>();
                services.AddTransient<BlockingRulesViewModel>();
                services.AddTransient<ReportsViewModel>();

                // Registrar Views
                services.AddTransient<LoginView>();
                services.AddTransient<DashboardView>();
                services.AddTransient<SettingsView>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Prevent application from shutting down when windows are closed
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        try
        {
            var logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "debug-startup.log");
            File.AppendAllText(logFile, $"[{DateTime.Now}] Starting AulaSegura...\n");
            
            // Inicializar base de datos
            File.AppendAllText(logFile, $"[{DateTime.Now}] Initializing database...\n");
            await _host.Services.InitializeDatabaseAsync();
            File.AppendAllText(logFile, $"[{DateTime.Now}] Database initialized successfully\n");
            
            // Mostrar ventana de login
            File.AppendAllText(logFile, $"[{DateTime.Now}] Creating LoginView...\n");
            var loginView = _host.Services.GetRequiredService<LoginView>();
            File.AppendAllText(logFile, $"[{DateTime.Now}] LoginView created: {loginView != null}\n");
            
            // Asegurar que la ventana sea visible y tenga el foco
            loginView.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            loginView.ShowActivated = true;
            loginView.Topmost = true;
            loginView.Owner = null; // No tener owner para que aparezca independiente
            
            File.AppendAllText(logFile, $"[{DateTime.Now}] Showing LoginView...\n");
            loginView.Show();
            File.AppendAllText(logFile, $"[{DateTime.Now}] LoginView.Show() called\n");
            
            // Forzar el enfoque usando Dispatcher
            loginView.Dispatcher.InvokeAsync(() =>
            {
                loginView.Activate();
                loginView.Focus();
                File.AppendAllText(logFile, $"[{DateTime.Now}] LoginView activated\n");
            }, System.Windows.Threading.DispatcherPriority.Normal);
            
            File.AppendAllText(logFile, $"[{DateTime.Now}] Application startup complete\n");
        }
        catch (Exception ex)
        {
            var logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "debug-startup.log");
            File.AppendAllText(logFile, $"[{DateTime.Now}] ERROR: {ex.Message}\n{ex.StackTrace}\n\nInner: {ex.InnerException?.Message}\n{ex.InnerException?.StackTrace}\n");
            
            // Escribir error a un archivo de log
            var errorLogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "startup-error.log");
            File.WriteAllText(errorLogPath, $"{DateTime.Now}: {ex.Message}\n{ex.StackTrace}\n\nInner Exception: {ex.InnerException?.Message}\n{ex.InnerException?.StackTrace}");
            
            MessageBox.Show(
                $"Error al iniciar la aplicación:\n\n{ex.Message}\n\nSe ha creado un archivo de log:\n{errorLogPath}",
                "Error de Inicio - AulaSegura",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        // Limpiar recursos
        await _host.StopAsync();
        _host.Dispose();

        base.OnExit(e);
    }
}

