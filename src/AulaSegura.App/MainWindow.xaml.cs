using System.Windows;
using AulaSegura.App.ViewModels;
using AulaSegura.App.Views;
using AulaSegura.Core.Entities;
using AulaSegura.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

// Add ViewModel namespaces
using BlockedSitesViewModel = AulaSegura.App.ViewModels.BlockedSitesViewModel;
using AllowedSitesViewModel = AulaSegura.App.ViewModels.AllowedSitesViewModel;
using CategoriesViewModel = AulaSegura.App.ViewModels.CategoriesViewModel;
using SchedulesViewModel = AulaSegura.App.ViewModels.SchedulesViewModel;
using SettingsViewModel = AulaSegura.App.ViewModels.SettingsViewModel;
using KeywordsViewModel = AulaSegura.App.ViewModels.KeywordsViewModel;
using BlockingRulesViewModel = AulaSegura.App.ViewModels.BlockingRulesViewModel;
using ReportsViewModel = AulaSegura.App.ViewModels.ReportsViewModel;

namespace AulaSegura.App;

/// <summary>
/// Main application window - Navigation container
/// </summary>
public partial class MainWindow : Window
{
    private readonly Administrator _currentAdmin;
    private readonly IServiceProvider _serviceProvider;

    public MainWindow(Administrator admin, IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _currentAdmin = admin;
        _serviceProvider = serviceProvider;
        
        Title = $"AulaSegura - Panel de Control";
        
        // Navigate to Dashboard on startup
        NavigateToDashboard();
    }

    public void NavigateToDashboard()
    {
        // Manually create DashboardViewModel with Administrator object
        var blockedSiteService = _serviceProvider.GetRequiredService<IBlockedSiteService>();
        var allowedSiteService = _serviceProvider.GetRequiredService<IAllowedSiteService>();
        var categoryService = _serviceProvider.GetRequiredService<ICategoryService>();
        var activityLogService = _serviceProvider.GetRequiredService<IActivityLogService>();
        var hostsFileProbe = _serviceProvider.GetRequiredService<IHostsFileAccessProbe>();

        var viewModel = new DashboardViewModel(
            _currentAdmin,
            blockedSiteService,
            allowedSiteService,
            categoryService,
            activityLogService,
            hostsFileProbe);
        
        // Set the MainWindow reference BEFORE creating the view
        viewModel.SetParentWindow(this);
        
        var dashboardView = new DashboardView(_currentAdmin)
        {
            DataContext = viewModel
        };
        
        // Get the content (Grid) from DashboardView
        var content = dashboardView.Content;
        
        // Ensure DataContext is set on the content directly
        if (content is FrameworkElement fe)
        {
            fe.DataContext = viewModel;
        }
        
        Content = content;
    }
    
    /// <summary>
    /// Navigate to Blocked Sites management screen
    /// </summary>
    public void NavigateToBlockedSites(Administrator admin)
    {
        try
        {
            var viewModel = new BlockedSitesViewModel(
                _serviceProvider.GetRequiredService<IBlockedSiteService>(),
                _serviceProvider.GetRequiredService<ICategoryService>());
            
            // Set the MainWindow reference
            viewModel.SetParentWindow(this);
            
            var view = new BlockedSitesView
            {
                DataContext = viewModel
            };
            
            // Set DataContext on the content directly
            if (view.Content is FrameworkElement fe)
            {
                fe.DataContext = viewModel;
            }
            
            Content = view.Content;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al cargar la pantalla de Sitios Bloqueados:\n\n{ex.Message}", 
                "Error de Navegación", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    /// <summary>
    /// Navigate to Allowed Sites management screen
    /// </summary>
    public void NavigateToAllowedSites(Administrator admin)
    {
        try
        {
            var viewModel = new AllowedSitesViewModel(
                _serviceProvider.GetRequiredService<IAllowedSiteService>());
            
            // Set the MainWindow reference
            viewModel.SetParentWindow(this);
            
            var view = new AllowedSitesView
            {
                DataContext = viewModel
            };
            
            // Set DataContext on the content directly
            if (view.Content is FrameworkElement fe)
            {
                fe.DataContext = viewModel;
            }
            
            Content = view.Content;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al cargar la pantalla de Sitios Permitidos:\n\n{ex.Message}", 
                "Error de Navegación", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    /// <summary>
    /// Navigate to Categories management screen
    /// </summary>
    public void NavigateToCategories(Administrator admin)
    {
        try
        {
            var viewModel = new CategoriesViewModel(
                _serviceProvider.GetRequiredService<ICategoryService>());
            
            // Set the MainWindow reference
            viewModel.SetParentWindow(this);
            
            var view = new CategoriesView
            {
                DataContext = viewModel
            };
            
            // Set DataContext on the content directly
            if (view.Content is FrameworkElement fe)
            {
                fe.DataContext = viewModel;
            }
            
            Content = view.Content;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al cargar la pantalla de Categorías:\n\n{ex.Message}", 
                "Error de Navegación", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    /// <summary>
    /// Navigate to Schedules management screen
    /// </summary>
    public void NavigateToSchedules(Administrator admin)
    {
        try
        {
            var viewModel = new SchedulesViewModel(
                _serviceProvider.GetRequiredService<IScheduleService>(),
                _serviceProvider.GetRequiredService<ICategoryService>());
            
            // Set the MainWindow reference
            viewModel.SetParentWindow(this);
            
            var view = new SchedulesView
            {
                DataContext = viewModel
            };
            
            // Set DataContext on the content directly
            if (view.Content is FrameworkElement fe)
            {
                fe.DataContext = viewModel;
            }
            
            Content = view.Content;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al cargar la pantalla de Horarios:\n\n{ex.Message}", 
                "Error de Navegación", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    /// <summary>
    /// Navigate to Settings screen
    /// </summary>
    public void NavigateToSettings(Administrator admin)
    {
        try
        {
            var viewModel = new SettingsViewModel(
                _serviceProvider.GetRequiredService<IAuthService>(),
                _serviceProvider.GetRequiredService<ISettingsService>());
            
            // Set the MainWindow reference
            viewModel.SetParentWindow(this);
            
            var view = new SettingsView
            {
                DataContext = viewModel
            };
            
            // Set DataContext on the content directly
            if (view.Content is FrameworkElement fe)
            {
                fe.DataContext = viewModel;
            }
            
            Content = view.Content;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al cargar la pantalla de Configuración:\n\n{ex.Message}", 
                "Error de Navegación", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    /// <summary>
    /// Navigate to Keywords management screen
    /// </summary>
    public void NavigateToKeywords(Administrator admin)
    {
        try
        {
            var viewModel = new KeywordsViewModel(
                _serviceProvider.GetRequiredService<IKeywordService>());
            
            // Set the MainWindow reference
            viewModel.SetParentWindow(this);
            
            var view = new KeywordsView
            {
                DataContext = viewModel
            };
            
            // Set DataContext on the content directly
            if (view.Content is FrameworkElement fe)
            {
                fe.DataContext = viewModel;
            }
            
            Content = view.Content;
                    
            // Trigger data loading after navigation
            if (viewModel.LoadKeywordsCommand is CommunityToolkit.Mvvm.Input.AsyncRelayCommand loadCmd)
            {
                _ = loadCmd.ExecuteAsync(null);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al cargar la pantalla de Palabras Clave:\n\n{ex.Message}", 
                "Error de Navegación", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    /// <summary>
    /// Navigate to Blocking Rules management screen
    /// </summary>
    public void NavigateToBlockingRules(Administrator admin)
    {
        try
        {
            var viewModel = new BlockingRulesViewModel(
                _serviceProvider.GetRequiredService<IBlockingRuleService>(),
                _serviceProvider.GetRequiredService<ICategoryService>());
            
            // Set the MainWindow reference
            viewModel.SetParentWindow(this);
            
            var view = new BlockingRulesView
            {
                DataContext = viewModel
            };
            
            // Set DataContext on the content directly
            if (view.Content is FrameworkElement fe)
            {
                fe.DataContext = viewModel;
            }
            
            Content = view.Content;
                    
            // Trigger data loading after navigation
            if (viewModel.LoadRulesCommand is CommunityToolkit.Mvvm.Input.AsyncRelayCommand loadRulesCmd)
            {
                _ = loadRulesCmd.ExecuteAsync(null);
            }
            if (viewModel.LoadCategoriesCommand is CommunityToolkit.Mvvm.Input.AsyncRelayCommand loadCatCmd)
            {
                _ = loadCatCmd.ExecuteAsync(null);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al cargar la pantalla de Reglas de Bloqueo:\n\n{ex.Message}", 
                "Error de Navegación", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    /// <summary>
    /// Navigate to Reports screen
    /// </summary>
    public void NavigateToReports(Administrator admin)
    {
        try
        {
            var viewModel = new ReportsViewModel(
                _serviceProvider.GetRequiredService<IReportService>());
            
            // Set the MainWindow reference
            viewModel.SetParentWindow(this);
            
            var view = new ReportsView
            {
                DataContext = viewModel
            };
            
            // Set DataContext on the content directly
            if (view.Content is FrameworkElement fe)
            {
                fe.DataContext = viewModel;
            }
            
            Content = view.Content;
                    
            // Trigger initial report generation after navigation
            if (viewModel.GenerateReportCommand is CommunityToolkit.Mvvm.Input.AsyncRelayCommand genCmd)
            {
                _ = genCmd.ExecuteAsync(null);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al cargar la pantalla de Reportes:\n\n{ex.Message}", 
                "Error de Navegación", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
