using System.Collections.ObjectModel;
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

    [ObservableProperty]
    private string _institutionName = "AulaSegura";

    [ObservableProperty]
    private string _protectionLevel = "Alto";

    // LiveCharts series for activity visualization
    public ISeries[] ActivitySeries { get; set; } = Array.Empty<ISeries>();

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
        IActivityLogService activityLogService)
    {
        _currentAdmin = currentAdmin;
        _blockedSiteService = blockedSiteService;
        _allowedSiteService = allowedSiteService;
        _categoryService = categoryService;
        _activityLogService = activityLogService;

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

            // Update chart data
            UpdateActivityChart();
        }
        catch (Exception ex)
        {
            // Log error but don't crash the UI
            System.Diagnostics.Debug.WriteLine($"Error loading dashboard: {ex.Message}");
        }
    }

    private void UpdateActivityChart()
    {
        // Create sample data for the last 7 days
        // In a real implementation, this would query actual activity by date
        var labels = new string[] { "Lun", "Mar", "Mié", "Jue", "Vie", "Sáb", "Dom" };
        var values = new double[] { 15, 22, 18, 25, 20, 10, 8 };

        ActivitySeries = new ISeries[]
        {
            new ColumnSeries<double>
            {
                Values = values,
                Name = "Actividad de Bloqueo"
            }
        };
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
