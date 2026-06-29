using System.IO;
using System.Windows;
using AulaSegura.App.ViewModels;
using AulaSegura.App.Views;
using AulaSegura.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AulaSegura.App;

/// <summary>
/// Punto de arranque de la aplicacion WPF.
/// </summary>
public partial class App : Application
{
    private readonly IHost _host;

    public App()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddInfrastructure(context.Configuration);

                services.AddTransient<LoginViewModel>();
                services.AddTransient<BlockedSitesViewModel>();
                services.AddTransient<AllowedSitesViewModel>();
                services.AddTransient<CategoriesViewModel>();
                services.AddTransient<SchedulesViewModel>();
                services.AddTransient<SettingsViewModel>();
                services.AddTransient<KeywordsViewModel>();
                services.AddTransient<BlockingRulesViewModel>();
                services.AddTransient<ReportsViewModel>();

                services.AddTransient<LoginView>();
                services.AddTransient<DashboardView>();
                services.AddTransient<SettingsView>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        try
        {
            await _host.Services.InitializeDatabaseAsync();

            var loginView = _host.Services.GetRequiredService<LoginView>();
            loginView.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            loginView.ShowActivated = true;
            loginView.Show();
            loginView.Activate();
        }
        catch (Exception ex)
        {
            var errorLogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "startup-error.log");
            File.WriteAllText(
                errorLogPath,
                $"{DateTime.Now}: {ex.Message}{Environment.NewLine}{ex.StackTrace}{Environment.NewLine}{Environment.NewLine}Inner Exception: {ex.InnerException?.Message}{Environment.NewLine}{ex.InnerException?.StackTrace}");

            MessageBox.Show(
                $"Error al iniciar la aplicacion.{Environment.NewLine}{Environment.NewLine}Revise el archivo de log:{Environment.NewLine}{errorLogPath}",
                "Error de Inicio - AulaSegura",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await _host.StopAsync();
        _host.Dispose();

        base.OnExit(e);
    }
}
