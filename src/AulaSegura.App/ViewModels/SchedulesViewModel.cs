using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using AulaSegura.Core.Entities;
using AulaSegura.Core.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AulaSegura.App.ViewModels;

/// <summary>
/// ViewModel para gestión de horarios de bloqueo
/// </summary>
public partial class SchedulesViewModel : ObservableObject
{
    private readonly IScheduleService _scheduleService;
    private readonly ICategoryService _categoryService;
    private MainWindow? _mainWindow;

    /// <summary>
    /// Sets the parent MainWindow reference for navigation
    /// </summary>
    public void SetParentWindow(MainWindow mainWindow)
    {
        _mainWindow = mainWindow;
    }

    [ObservableProperty]
    private ObservableCollection<Schedule> _schedules = new();

    [ObservableProperty]
    private ObservableCollection<Category> _categories = new();

    [ObservableProperty]
    private Schedule? _selectedSchedule;

    // Form fields
    [ObservableProperty]
    private string _formName = string.Empty;

    [ObservableProperty]
    private DayOfWeek _formDayOfWeek = DayOfWeek.Monday;

    [ObservableProperty]
    private TimeSpan _formStartTime = TimeSpan.FromHours(8);

    [ObservableProperty]
    private TimeSpan _formEndTime = TimeSpan.FromHours(15);

    [ObservableProperty]
    private int _formCategoryId = 0;

    [ObservableProperty]
    private bool _isFormVisible = false;

    [ObservableProperty]
    private bool _isEditMode = false;

    [ObservableProperty]
    private int? _editingScheduleId = null;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public ICommand LoadDataCommand { get; }
    public ICommand AddNewCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand GoBackCommand { get; }

    public SchedulesViewModel(
        IScheduleService scheduleService,
        ICategoryService categoryService)
    {
        _scheduleService = scheduleService;
        _categoryService = categoryService;

        LoadDataCommand = new AsyncRelayCommand(LoadDataAsync);
        AddNewCommand = new RelayCommand(ShowAddForm);
        EditCommand = new AsyncRelayCommand<Schedule>(EditScheduleAsync);
        DeleteCommand = new AsyncRelayCommand<Schedule>(DeleteScheduleAsync);
        SaveCommand = new AsyncRelayCommand(SaveScheduleAsync, CanSaveSchedule);
        CancelCommand = new RelayCommand(HideForm);
        GoBackCommand = new RelayCommand(GoBack);

        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            // Get all schedules by iterating through days
            var allSchedules = new List<Schedule>();
            foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
            {
                var daySchedules = await _scheduleService.GetActiveSchedulesForDayAsync(day);
                allSchedules.AddRange(daySchedules);
            }
            
            Schedules.Clear();
            foreach (var schedule in allSchedules.DistinctBy(s => s.Id))
            {
                Schedules.Add(schedule);
            }

            var categories = await _categoryService.GetActiveCategoriesAsync();
            Categories.Clear();
            foreach (var category in categories)
            {
                Categories.Add(category);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al cargar: {ex.Message}";
        }
    }

    private void ShowAddForm()
    {
        IsEditMode = false;
        EditingScheduleId = null;
        FormName = string.Empty;
        FormDayOfWeek = DayOfWeek.Monday;
        FormStartTime = TimeSpan.FromHours(8);
        FormEndTime = TimeSpan.FromHours(15);
        FormCategoryId = Categories.Any() ? Categories.First().Id : 0;
        IsFormVisible = true;
        ErrorMessage = string.Empty;
    }

    private async Task EditScheduleAsync(Schedule? schedule)
    {
        if (schedule == null) return;

        IsEditMode = true;
        EditingScheduleId = schedule.Id;
        FormName = schedule.Name;
        FormDayOfWeek = schedule.DayOfWeek;
        FormStartTime = schedule.StartTime;
        FormEndTime = schedule.EndTime;
        FormCategoryId = schedule.CategoryId ?? 0;
        IsFormVisible = true;
        ErrorMessage = string.Empty;
    }

    private async Task DeleteScheduleAsync(Schedule? schedule)
    {
        if (schedule == null) return;

        var result = MessageBox.Show(
            $"¿Eliminar horario '{schedule.Name}'?",
            "Confirmar",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                await _scheduleService.DeleteScheduleAsync(schedule.Id);
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
            }
        }
    }

    private async Task SaveScheduleAsync()
    {
        try
        {
            if (IsEditMode && EditingScheduleId.HasValue)
            {
                var schedule = new Schedule
                {
                    Id = EditingScheduleId.Value,
                    Name = FormName.Trim(),
                    DayOfWeek = FormDayOfWeek,
                    StartTime = FormStartTime,
                    EndTime = FormEndTime,
                    CategoryId = FormCategoryId > 0 ? FormCategoryId : (int?)null,
                    IsActive = true
                };
                await _scheduleService.UpdateScheduleAsync(schedule);
            }
            else
            {
                await _scheduleService.CreateScheduleAsync(
                    FormName.Trim(),
                    FormCategoryId,
                    FormDayOfWeek,
                    FormStartTime,
                    FormEndTime);
            }

            HideForm();
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al guardar: {ex.Message}";
        }
    }

    private bool CanSaveSchedule() => 
        !string.IsNullOrWhiteSpace(FormName) && FormCategoryId > 0;

    private void HideForm()
    {
        IsFormVisible = false;
        FormName = string.Empty;
    }

    partial void OnFormNameChanged(string value)
    {
        ((AsyncRelayCommand)SaveCommand).NotifyCanExecuteChanged();
    }

    partial void OnFormCategoryIdChanged(int value)
    {
        ((AsyncRelayCommand)SaveCommand).NotifyCanExecuteChanged();
    }

    private void GoBack()
    {
        _mainWindow?.NavigateToDashboard();
    }
}
