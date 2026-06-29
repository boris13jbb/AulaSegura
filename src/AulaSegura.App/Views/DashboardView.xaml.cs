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
        _mainWindow = mainWindow;
        
        // If ViewModel is already set, update its navigation reference
        if (DataContext is DashboardViewModel vm)
        {
            vm.SetParentWindow(mainWindow);
        }
    }
}
