using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using AulaSegura.Core.Entities;
using AulaSegura.Core.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AulaSegura.App.ViewModels;

public partial class BlockingRulesViewModel : ObservableObject
{
    private const string CategoryRuleType = "CATEGORY";
    private const string BlockAction = "Block";

    private readonly IBlockingRuleService _blockingRuleService;
    private readonly ICategoryService _categoryService;
    private MainWindow? _mainWindow;

    [ObservableProperty]
    private ObservableCollection<BlockingRule> _rules = new();

    [ObservableProperty]
    private ObservableCollection<Category> _categories = new();

    [ObservableProperty]
    private string _newRuleName = string.Empty;

    [ObservableProperty]
    private Category? _selectedCategory;

    [ObservableProperty]
    private int _maxViolations = 3;

    [ObservableProperty]
    private int _blockDurationMinutes = 30;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private BlockingRule? _selectedRule;

    [ObservableProperty]
    private bool _isEditing;

    public BlockingRulesViewModel(IBlockingRuleService blockingRuleService, ICategoryService categoryService)
    {
        _blockingRuleService = blockingRuleService;
        _categoryService = categoryService;

        LoadRulesCommand = new AsyncRelayCommand(LoadRulesAsync);
        LoadCategoriesCommand = new AsyncRelayCommand(LoadCategoriesAsync);
        AddRuleCommand = new AsyncRelayCommand(AddRuleAsync);
        UpdateRuleCommand = new AsyncRelayCommand(UpdateRuleAsync, () => SelectedRule != null);
        DeleteRuleCommand = new AsyncRelayCommand<BlockingRule>(DeleteRuleAsync);
        EditRuleCommand = new RelayCommand<BlockingRule>(EditRule, rule => rule != null);
        CancelEditCommand = new RelayCommand(CancelEdit);
        GoBackCommand = new RelayCommand(GoBack);
    }

    public ICommand LoadRulesCommand { get; }
    public ICommand LoadCategoriesCommand { get; }
    public ICommand AddRuleCommand { get; }
    public ICommand UpdateRuleCommand { get; }
    public ICommand DeleteRuleCommand { get; }
    public ICommand EditRuleCommand { get; }
    public ICommand CancelEditCommand { get; }
    public ICommand GoBackCommand { get; }

    public void SetParentWindow(MainWindow mainWindow)
    {
        _mainWindow = mainWindow;
    }

    private async Task LoadRulesAsync()
    {
        IsLoading = true;

        try
        {
            var rules = await _blockingRuleService.GetAllRulesAsync();
            Rules.Clear();

            foreach (var rule in rules)
            {
                Rules.Add(rule);
            }

            StatusMessage = $"Cargadas {Rules.Count} reglas de bloqueo";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error al cargar reglas: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadCategoriesAsync()
    {
        try
        {
            var categories = await _categoryService.GetActiveCategoriesAsync();
            Categories.Clear();

            foreach (var category in categories)
            {
                Categories.Add(category);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error al cargar categorias: {ex.Message}";
        }
    }

    private async Task AddRuleAsync()
    {
        if (!ValidateForm())
            return;

        try
        {
            var rule = BuildRuleFromForm(new BlockingRule());
            await _blockingRuleService.AddRuleAsync(rule);

            ResetForm();
            await LoadRulesAsync();
            StatusMessage = "Regla de bloqueo creada exitosamente";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error al crear regla: {ex.Message}";
        }
    }

    private void EditRule(BlockingRule? rule)
    {
        if (rule == null)
            return;

        SelectedRule = rule;
        NewRuleName = rule.Name;
        SelectedCategory = Categories.FirstOrDefault(c => c.Id == rule.CategoryId);
        MaxViolations = Math.Max(1, rule.MaxViolations);
        BlockDurationMinutes = Math.Max(1, rule.BlockDurationMinutes);
        IsEditing = true;
        StatusMessage = $"Editando: {rule.Name}";
    }

    private async Task UpdateRuleAsync()
    {
        if (SelectedRule == null || !ValidateForm())
            return;

        try
        {
            BuildRuleFromForm(SelectedRule);
            await _blockingRuleService.UpdateRuleAsync(SelectedRule);

            ResetForm();
            await LoadRulesAsync();
            StatusMessage = "Regla actualizada exitosamente";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error al actualizar regla: {ex.Message}";
        }
    }

    private async Task DeleteRuleAsync(BlockingRule? rule)
    {
        if (rule == null)
            return;

        var result = MessageBox.Show(
            $"Eliminar la regla '{rule.Name}'?",
            "Confirmar eliminacion",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes)
            return;

        try
        {
            await _blockingRuleService.DeleteRuleAsync(rule.Id);
            await LoadRulesAsync();
            StatusMessage = "Regla eliminada";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error al eliminar regla: {ex.Message}";
        }
    }

    private bool ValidateForm()
    {
        if (string.IsNullOrWhiteSpace(NewRuleName))
        {
            StatusMessage = "Ingrese el nombre de la regla";
            return false;
        }

        if (SelectedCategory == null)
        {
            StatusMessage = "Seleccione una categoria activa";
            return false;
        }

        if (MaxViolations < 1)
        {
            StatusMessage = "El maximo de violaciones debe ser mayor a cero";
            return false;
        }

        if (BlockDurationMinutes < 1)
        {
            StatusMessage = "La duracion del bloqueo debe ser mayor a cero";
            return false;
        }

        return true;
    }

    private BlockingRule BuildRuleFromForm(BlockingRule rule)
    {
        var categoryId = SelectedCategory!.Id;

        rule.Name = NewRuleName.Trim();
        rule.CategoryId = categoryId;
        rule.RuleType = CategoryRuleType;
        rule.Action = BlockAction;
        rule.Value = categoryId.ToString(CultureInfo.InvariantCulture);
        rule.MaxViolations = MaxViolations;
        rule.BlockDurationMinutes = BlockDurationMinutes;
        rule.IsActive = true;

        return rule;
    }

    private void CancelEdit()
    {
        ResetForm();
        StatusMessage = "Edicion cancelada";
    }

    private void ResetForm()
    {
        SelectedRule = null;
        NewRuleName = string.Empty;
        SelectedCategory = null;
        MaxViolations = 3;
        BlockDurationMinutes = 30;
        IsEditing = false;
    }

    private void GoBack()
    {
        _mainWindow?.NavigateToDashboard();
    }

    partial void OnSelectedRuleChanged(BlockingRule? value)
    {
        if (UpdateRuleCommand is AsyncRelayCommand command)
        {
            command.NotifyCanExecuteChanged();
        }
    }
}
