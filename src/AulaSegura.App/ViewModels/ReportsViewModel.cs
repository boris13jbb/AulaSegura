using System.Collections.ObjectModel;
using System.Windows.Input;
using AulaSegura.Core.Entities;
using AulaSegura.Core.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AulaSegura.App.ViewModels;

public partial class ReportsViewModel : ObservableObject
{
    private readonly IReportService _reportService;
    private MainWindow? _mainWindow;

    /// <summary>
    /// Sets the parent MainWindow reference for navigation
    /// </summary>
    public void SetParentWindow(MainWindow mainWindow)
    {
        _mainWindow = mainWindow;
    }

    [ObservableProperty]
    private ObservableCollection<Report> _reports = new();

    [ObservableProperty]
    private DateTime _startDate = DateTime.Now.AddDays(-7);

    [ObservableProperty]
    private DateTime _endDate = DateTime.Now;

    [ObservableProperty]
    private ReportType _selectedReportType = ReportType.Summary;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private int _totalBlocks;

    [ObservableProperty]
    private int _totalViolations;

    [ObservableProperty]
    private string _topBlockedSite = "N/A";

    public ReportsViewModel(IReportService reportService)
    {
        _reportService = reportService;
        
        GenerateReportCommand = new AsyncRelayCommand(GenerateReportAsync);
        LoadReportsCommand = new AsyncRelayCommand(LoadReportsAsync);
        ExportReportCommand = new AsyncRelayCommand(ExportReportAsync);
        GoBackCommand = new RelayCommand(GoBack);
    }

    public ICommand GenerateReportCommand { get; }
    public ICommand LoadReportsCommand { get; }
    public ICommand ExportReportCommand { get; }
    public ICommand GoBackCommand { get; }

    private async Task GenerateReportAsync()
    {
        IsLoading = true;
        try
        {
            var report = await _reportService.GenerateReportAsync(
                StartDate,
                EndDate,
                SelectedReportType
            );

            Reports.Clear();
            Reports.Add(report);

            // Actualizar estadísticas
            TotalBlocks = report.TotalBlocks;
            TotalViolations = report.TotalViolations;
            TopBlockedSite = report.TopBlockedSite ?? "N/A";

            StatusMessage = "Reporte generado exitosamente";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error al generar reporte: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadReportsAsync()
    {
        IsLoading = true;
        try
        {
            var reports = await _reportService.GetReportsByDateRangeAsync(StartDate, EndDate);
            Reports.Clear();
            foreach (var report in reports)
            {
                Reports.Add(report);
            }
            StatusMessage = $"Cargados {Reports.Count} reportes";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error al cargar reportes: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ExportReportAsync()
    {
        if (Reports.Count == 0)
        {
            StatusMessage = "No hay reportes para exportar";
            return;
        }

        try
        {
            var fileName = $"Reporte_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            await _reportService.ExportReportToCsvAsync(Reports.Last(), fileName);
            StatusMessage = $"Reporte exportado: {fileName}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error al exportar: {ex.Message}";
        }
    }

    private void GoBack()
    {
        _mainWindow?.NavigateToDashboard();
    }
}
