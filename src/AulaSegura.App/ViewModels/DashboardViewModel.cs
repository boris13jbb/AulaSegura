using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using AulaSegura.Core.Entities;
using AulaSegura.Core.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;

namespace AulaSegura.App.ViewModels;

/// <summary>
/// ViewModel para el Dashboard principal
/// </summary>
public partial class DashboardViewModel : ObservableObject
{
    private readonly IBlockedSiteService _blockedSiteService;
    private readonly IAllowedSiteService _allowedSiteService;
    private readonly ICategoryService _categoryService;
    private readonly IActivityLogService _activityLogService;
    private readonly IHostsFileAccessProbe _hostsFileAccessProbe;
    private readonly Administrator _currentAdmin;
    private MainWindow? _mainWindow;

    /// <summary>
    /// Sets the parent MainWindow reference for navigation
    /// </summary>
    public void SetParentWindow(MainWindow mainWindow)
    {
        System.Diagnostics.Debug.WriteLine($"[DashboardViewModel] SetParentWindow called");
        _mainWindow = mainWindow;
    }

    [ObservableProperty]
    private int _totalBlockedSites = 0;

    [ObservableProperty]
    private int _totalAllowedSites = 0;

    [ObservableProperty]
    private int _activeCategories = 0;

    [ObservableProperty]
    private int _recentActivities = 0;

    [ObservableProperty]
    private ObservableCollection<ActivityLog> _recentLogs = new();

    /// <summary>Serie principal del gráfico (eventos registrados relacionados con bloqueo).</summary>
    [ObservableProperty]
    private ISeries[] _activitySeries = Array.Empty<ISeries>();

    [ObservableProperty]
    private Axis[] _xAxes =
    [
        new Axis()
    ];

    [ObservableProperty]
    private string _institutionName = "AulaSegura";

    [ObservableProperty]
    private string _protectionLevel = "Alto";

    /// <summary>Estado de la prueba real de escritura en %SystemRoot%\System32\drivers\etc\hosts.</summary>
    [ObservableProperty]
    private bool _hostsFileCanWrite;

    [ObservableProperty]
    private string _hostsFilePrimaryStatus = "Comprobando escritura en hosts…";

    [ObservableProperty]
    private string _hostsFileDetail = string.Empty;

    [ObservableProperty]
    private string _hostsProbeTimeText = string.Empty;

    public ICommand RefreshCommand { get; }
    public ICommand NavigateToBlockedSitesCommand { get; }
    public ICommand NavigateToAllowedSitesCommand { get; }
    public ICommand NavigateToCategoriesCommand { get; }
    public ICommand NavigateToSchedulesCommand { get; }
    public ICommand NavigateToSettingsCommand { get; }
    public ICommand NavigateToKeywordsCommand { get; }
    public ICommand NavigateToBlockingRulesCommand { get; }
    public ICommand NavigateToReportsCommand { get; }
    public ICommand LogoutCommand { get; }

    public DashboardViewModel(
        Administrator currentAdmin,
        IBlockedSiteService blockedSiteService,
        IAllowedSiteService allowedSiteService,
        ICategoryService categoryService,
        IActivityLogService activityLogService,
        IHostsFileAccessProbe hostsFileAccessProbe)
    {
        _currentAdmin = currentAdmin;
        _blockedSiteService = blockedSiteService;
        _allowedSiteService = allowedSiteService;
        _categoryService = categoryService;
        _activityLogService = activityLogService;
        _hostsFileAccessProbe = hostsFileAccessProbe;

        RefreshCommand = new AsyncRelayCommand(LoadDashboardDataAsync);
        NavigateToBlockedSitesCommand = new RelayCommand(() => NavigateTo("BlockedSites"));
        NavigateToAllowedSitesCommand = new RelayCommand(() => NavigateTo("AllowedSites"));
        NavigateToCategoriesCommand = new RelayCommand(() => NavigateTo("Categories"));
        NavigateToSchedulesCommand = new RelayCommand(() => NavigateTo("Schedules"));
        NavigateToSettingsCommand = new RelayCommand(() => NavigateTo("Settings"));
        NavigateToKeywordsCommand = new RelayCommand(() => NavigateTo("Keywords"));
        NavigateToBlockingRulesCommand = new RelayCommand(() => NavigateTo("BlockingRules"));
        NavigateToReportsCommand = new RelayCommand(() => NavigateTo("Reports"));
        LogoutCommand = new RelayCommand(ExecuteLogout);

        // Load initial data
        _ = LoadDashboardDataAsync();
    }

    private async Task LoadDashboardDataAsync()
    {
        try
        {
            // Load statistics
            var blockedSites = await _blockedSiteService.GetAllBlockedSitesAsync();
            TotalBlockedSites = blockedSites.Count();

            var allowedSites = await _allowedSiteService.GetAllAllowedSitesAsync();
            TotalAllowedSites = allowedSites.Count();

            var categories = await _categoryService.GetActiveCategoriesAsync();
            ActiveCategories = categories.Count();

            // Load recent activity logs (last 20)
            var logs = await _activityLogService.GetRecentLogsAsync(20);
            RecentLogs.Clear();
            foreach (var log in logs)
            {
                RecentLogs.Add(log);
            }
            RecentActivities = logs.Count();

            var blockingDailyCounts = await _activityLogService.GetDailyBlockingRelatedLogCountsAsync(7);
            var startUtc = DateTime.UtcNow.Date.AddDays(-6);
            var culture = new CultureInfo("es-ES");
            var chartDayLabels = Enumerable.Range(0, 7)
                .Select(i => startUtc.AddDays(i).ToString("ddd dd/MM", culture))
                .ToArray();
            UpdateActivityChart(
                blockingDailyCounts.Select(static d => (double)d).ToArray(),
                chartDayLabels);

            // Load settings
            var institutionSetting = await GetSettingAsync("InstitutionName");
            if (!string.IsNullOrEmpty(institutionSetting))
            {
                InstitutionName = institutionSetting;
            }

            var protectionSetting = await GetSettingAsync("ProtectionLevel");
            if (!string.IsNullOrEmpty(protectionSetting))
            {
                ProtectionLevel = TranslateProtectionLevel(protectionSetting);
            }

            try
            {
                var hostsProbe = await _hostsFileAccessProbe.ProbeWriteAccessAsync();
                HostsFileCanWrite = hostsProbe.CanWrite;
                HostsFilePrimaryStatus = hostsProbe.CanWrite
                    ? "Hosts: escritura permitida"
                    : "Hosts: sin permiso de escritura";
                HostsFileDetail = hostsProbe.DetailMessage;
                HostsProbeTimeText = $"Comprobado: {DateTime.Now:dd/MM/yyyy HH:mm:ss}";
            }
            catch (Exception hx)
            {
                HostsFileCanWrite = false;
                HostsFilePrimaryStatus = "Hosts: error al comprobar";
                HostsFileDetail = hx.Message;
                HostsProbeTimeText = $"Intento: {DateTime.Now:dd/MM/yyyy HH:mm:ss}";
            }
        }
        catch (Exception ex)
        {
            // Log error but don't crash the UI
            System.Diagnostics.Debug.WriteLine($"Error loading dashboard: {ex.Message}");
        }
    }

    /// <summary>Basado en el log de auditoría (acciones de bloqueo), no en el archivo hosts.</summary>
    private void UpdateActivityChart(double[] values, string[] dayLabels)
    {
        ActivitySeries =
        [
            new ColumnSeries<double>
            {
                Values = values,
                Name = "Eventos relacionados con bloqueo (log)"
            }
        ];

        XAxes =
        [
            new Axis
            {
                Labels = dayLabels,
                LabelsRotation = -14
            }
        ];
    }

    private async Task<string?> GetSettingAsync(string key)
    {
        // This would ideally use ISettingsService
        // For now, return null to use defaults
        return await Task.FromResult<string?>(null);
    }

    private string TranslateProtectionLevel(string level)
    {
        return level.ToLower() switch
        {
            "low" => "Bajo",
            "medium" => "Medio",
            "high" => "Alto",
            "maximum" => "Máximo",
            _ => "Alto"
        };
    }

    private void NavigateTo(string screen)
    {
        System.Diagnostics.Debug.WriteLine($"[DashboardViewModel] NavigateTo called with screen: {screen}");
        System.Diagnostics.Debug.WriteLine($"[DashboardViewModel] _mainWindow is null: {_mainWindow == null}");
        
        if (_mainWindow == null)
        {
            System.Diagnostics.Debug.WriteLine($"[DashboardViewModel] ERROR: MainWindow reference is null!");
            System.Windows.MessageBox.Show(
                $"Error de navegación: No se puede navegar a {screen}.\n\nPor favor, reinicie la aplicación.",
                "Error de Navegación",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
            return;
        }

        try
        {
            System.Diagnostics.Debug.WriteLine($"[DashboardViewModel] Attempting navigation to: {screen}");
            
            // Navigate to the appropriate screen
            switch (screen)
            {
                case "BlockedSites":
                    System.Diagnostics.Debug.WriteLine($"[DashboardViewModel] Calling NavigateToBlockedSites");
                    _mainWindow.NavigateToBlockedSites(_currentAdmin);
                    break;
                case "AllowedSites":
                    System.Diagnostics.Debug.WriteLine($"[DashboardViewModel] Calling NavigateToAllowedSites");
                    _mainWindow.NavigateToAllowedSites(_currentAdmin);
                    break;
                case "Categories":
                    System.Diagnostics.Debug.WriteLine($"[DashboardViewModel] Calling NavigateToCategories");
                    _mainWindow.NavigateToCategories(_currentAdmin);
                    break;
                case "Schedules":
                    System.Diagnostics.Debug.WriteLine($"[DashboardViewModel] Calling NavigateToSchedules");
                    _mainWindow.NavigateToSchedules(_currentAdmin);
                    break;
                case "Settings":
                    System.Diagnostics.Debug.WriteLine($"[DashboardViewModel] Calling NavigateToSettings");
                    _mainWindow.NavigateToSettings(_currentAdmin);
                    break;
                case "Keywords":
                    System.Diagnostics.Debug.WriteLine($"[DashboardViewModel] Calling NavigateToKeywords");
                    _mainWindow.NavigateToKeywords(_currentAdmin);
                    break;
                case "BlockingRules":
                    System.Diagnostics.Debug.WriteLine($"[DashboardViewModel] Calling NavigateToBlockingRules");
                    _mainWindow.NavigateToBlockingRules(_currentAdmin);
                    break;
                case "Reports":
                    System.Diagnostics.Debug.WriteLine($"[DashboardViewModel] Calling NavigateToReports");
                    _mainWindow.NavigateToReports(_currentAdmin);
                    break;
                default:
                    System.Diagnostics.Debug.WriteLine($"[DashboardViewModel] Unknown screen: {screen}");
                    break;
            }
            System.Diagnostics.Debug.WriteLine($"[DashboardViewModel] Navigation completed successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DashboardViewModel] Navigation error: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[DashboardViewModel] Stack trace: {ex.StackTrace}");
            System.Windows.MessageBox.Show(
                $"Error al navegar a {screen}:\n\n{ex.Message}",
                "Error de Navegación",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
        }
    }

    private void ExecuteLogout()
    {
        // Logout logic - close current window and show login
        System.Windows.Application.Current.Shutdown();
    }
}
