using System.Windows;
using AulaSegura.App.ViewModels;
using AulaSegura.Core.Entities;

namespace AulaSegura.App.Views;

/// <summary>
/// Interaction logic for DashboardView.xaml
/// </summary>
public partial class DashboardView : Window
{
    private readonly Administrator _currentAdmin;
    private MainWindow? _mainWindow;

    public DashboardView(Administrator currentAdmin)
    {
        InitializeComponent();
        _currentAdmin = currentAdmin;
        
        // DataContext will be set by MainWindow
    }
    
    /// <summary>
    /// Sets the parent MainWindow reference for navigation
    /// </summary>
    public void SetParentWindow(MainWindow mainWindow)
    {
        System.Diagnostics.Debug.WriteLine($"[DashboardView] SetParentWindow called");
        _mainWindow = mainWindow;
        
        // If ViewModel is already set, update its navigation reference
        if (DataContext is DashboardViewModel vm)
        {
            System.Diagnostics.Debug.WriteLine($"[DashboardView] DataContext found, calling vm.SetParentWindow");
            vm.SetParentWindow(mainWindow);
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"[DashboardView] WARNING: DataContext is NOT DashboardViewModel!");
            System.Diagnostics.Debug.WriteLine($"[DashboardView] DataContext type: {DataContext?.GetType().Name ?? "null"}");
        }
    }
}
