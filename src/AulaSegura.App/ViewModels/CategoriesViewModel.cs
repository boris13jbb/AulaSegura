using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using AulaSegura.Core.Entities;
using AulaSegura.Core.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AulaSegura.App.ViewModels;

/// <summary>
/// ViewModel para gestión de categorías
/// </summary>
public partial class CategoriesViewModel : ObservableObject
{
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
    private ObservableCollection<Category> _categories = new();

    [ObservableProperty]
    private Category? _selectedCategory;

    // Form fields
    [ObservableProperty]
    private string _formName = string.Empty;

    [ObservableProperty]
    private string _formDescription = string.Empty;

    [ObservableProperty]
    private string _formColor = "#2196F3";

    [ObservableProperty]
    private bool _formIsActive = true;

    [ObservableProperty]
    private bool _isFormVisible = false;

    [ObservableProperty]
    private bool _isEditMode = false;

    [ObservableProperty]
    private int? _editingCategoryId = null;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public ICommand LoadDataCommand { get; }
    public ICommand AddNewCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand ToggleActiveCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand GoBackCommand { get; }

    public CategoriesViewModel(ICategoryService categoryService)
    {
        _categoryService = categoryService;

        LoadDataCommand = new AsyncRelayCommand(LoadDataAsync);
        AddNewCommand = new RelayCommand(ShowAddForm);
        EditCommand = new AsyncRelayCommand<Category>(EditCategoryAsync);
        ToggleActiveCommand = new AsyncRelayCommand<Category>(ToggleActiveAsync);
        SaveCommand = new AsyncRelayCommand(SaveCategoryAsync, CanSaveCategory);
        CancelCommand = new RelayCommand(HideForm);
        GoBackCommand = new RelayCommand(GoBack);

        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
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
        EditingCategoryId = null;
        FormName = string.Empty;
        FormDescription = string.Empty;
        FormColor = "#2196F3";
        FormIsActive = true;
        IsFormVisible = true;
        ErrorMessage = string.Empty;
    }

    private async Task EditCategoryAsync(Category? category)
    {
        if (category == null) return;

        IsEditMode = true;
        EditingCategoryId = category.Id;
        FormName = category.Name;
        FormDescription = category.Description ?? string.Empty;
        FormColor = category.Color;
        FormIsActive = category.IsActive;
        IsFormVisible = true;
        ErrorMessage = string.Empty;
    }

    private async Task ToggleActiveAsync(Category? category)
    {
        if (category == null) return;

        try
        {
            category.IsActive = !category.IsActive;
            await _categoryService.UpdateCategoryAsync(category);
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error: {ex.Message}";
        }
    }

    private async Task SaveCategoryAsync()
    {
        try
        {
            if (IsEditMode && EditingCategoryId.HasValue)
            {
                var category = new Category
                {
                    Id = EditingCategoryId.Value,
                    Name = FormName.Trim(),
                    Description = FormDescription,
                    Color = FormColor,
                    IsActive = FormIsActive
                };
                await _categoryService.UpdateCategoryAsync(category);
            }
            else
            {
                await _categoryService.CreateCategoryAsync(FormName.Trim(), FormDescription, FormColor);
            }

            HideForm();
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al guardar: {ex.Message}";
        }
    }

    private bool CanSaveCategory() => !string.IsNullOrWhiteSpace(FormName);

    private void HideForm()
    {
        IsFormVisible = false;
        FormName = string.Empty;
        FormDescription = string.Empty;
    }

    partial void OnFormNameChanged(string value)
    {
        ((AsyncRelayCommand)SaveCommand).NotifyCanExecuteChanged();
    }

    private void GoBack()
    {
        _mainWindow?.NavigateToDashboard();
    }
}
