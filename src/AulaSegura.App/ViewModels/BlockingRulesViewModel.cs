using System.Collections.ObjectModel;
using System.Windows.Input;
using AulaSegura.Core.Entities;
using AulaSegura.Core.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;

namespace AulaSegura.App.ViewModels;

public partial class BlockingRulesViewModel : ObservableObject
{
    private readonly IBlockingRuleService _blockingRuleService;
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
        EditRuleCommand = new RelayCommand<BlockingRule>(EditRule, r => r != null);
        CancelEditCommand = new RelayCommand(CancelEdit);
        GoBackCommand = new RelayCommand(GoBack);
        
        Debug.WriteLine($"[INIT] BlockingRulesViewModel inicializado");
        Debug.WriteLine($"[INIT] EditRuleCommand: {EditRuleCommand != null}");
        Debug.WriteLine($"[INIT] DeleteRuleCommand: {DeleteRuleCommand != null}");
        Debug.WriteLine($"[INIT] UpdateRuleCommand: {UpdateRuleCommand != null}");
    }

    public ICommand LoadRulesCommand { get; }
    public ICommand LoadCategoriesCommand { get; }
    public ICommand AddRuleCommand { get; }
    public ICommand UpdateRuleCommand { get; }
    public ICommand DeleteRuleCommand { get; }
    public ICommand EditRuleCommand { get; }
    public ICommand CancelEditCommand { get; }
    public ICommand GoBackCommand { get; }

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
            var categories = await _categoryService.GetAllCategoriesAsync();
            Categories.Clear();
            foreach (var category in categories.Where(c => c.IsActive))
            {
                Categories.Add(category);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error al cargar categorías: {ex.Message}";
        }
    }

    private async Task AddRuleAsync()
    {
        if (string.IsNullOrWhiteSpace(NewRuleName) || SelectedCategory == null)
        {
            StatusMessage = "Complete todos los campos requeridos";
            return;
        }

        try
        {
            var rule = new BlockingRule
            {
                Name = NewRuleName.Trim(),
                CategoryId = SelectedCategory.Id,
                MaxViolations = MaxViolations,
                BlockDurationMinutes = BlockDurationMinutes,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _blockingRuleService.AddRuleAsync(rule);
            NewRuleName = string.Empty;
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
        {
            Debug.WriteLine("[ERROR] EditRule llamado con rule nulo");
            return;
        }
        
        Debug.WriteLine($"[INFO] Editando regla: {rule.Name}, ID: {rule.Id}");
        SelectedRule = rule;
        NewRuleName = rule.Name;
        SelectedCategory = Categories.FirstOrDefault(c => c.Id == rule.CategoryId);
        MaxViolations = rule.MaxViolations;
        BlockDurationMinutes = rule.BlockDurationMinutes;
        IsEditing = true;
        StatusMessage = $"Editando: {rule.Name}";
        Debug.WriteLine($"[INFO] SelectedRule establecido: {SelectedRule?.Name}");
        Debug.WriteLine($"[INFO] UpdateRuleCommand CanExecute: {UpdateRuleCommand.CanExecute(null)}");
    }

    private async Task UpdateRuleAsync()
    {
        if (SelectedRule == null || string.IsNullOrWhiteSpace(NewRuleName) || SelectedCategory == null)
        {
            StatusMessage = "Complete todos los campos requeridos";
            return;
        }

        try
        {
            SelectedRule.Name = NewRuleName.Trim();
            SelectedRule.CategoryId = SelectedCategory.Id;
            SelectedRule.MaxViolations = MaxViolations;
            SelectedRule.BlockDurationMinutes = BlockDurationMinutes;
            SelectedRule.UpdatedAt = DateTime.UtcNow;

            await _blockingRuleService.UpdateRuleAsync(SelectedRule);
            CancelEdit();
            await LoadRulesAsync();
            StatusMessage = "Regla actualizada exitosamente";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error al actualizar: {ex.Message}";
        }
    }

    private void CancelEdit()
    {
        SelectedRule = null;
        NewRuleName = string.Empty;
        SelectedCategory = null;
        MaxViolations = 3;
        BlockDurationMinutes = 30;
        IsEditing = false;
        StatusMessage = "Edición cancelada";
    }

    private async Task DeleteRuleAsync(BlockingRule? rule)
    {
        if (rule == null) return;

        try
        {
            await _blockingRuleService.DeleteRuleAsync(rule.Id);
            await LoadRulesAsync();
            StatusMessage = "Regla eliminada";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error al eliminar: {ex.Message}";
        }
    }

    private void GoBack()
    {
        _mainWindow?.NavigateToDashboard();
    }

    partial void OnSelectedRuleChanged(BlockingRule? value)
    {
        Debug.WriteLine($"[DEBUG] OnSelectedRuleChanged llamado. Valor: {value?.Name ?? "null"}");
        if (UpdateRuleCommand is AsyncRelayCommand cmd)
        {
            cmd.NotifyCanExecuteChanged();
            Debug.WriteLine($"[DEBUG] NotifyCanExecuteChanged llamado. CanExecute ahora: {cmd.CanExecute(null)}");
        }
        else
        {
            Debug.WriteLine("[ERROR] UpdateRuleCommand no es AsyncRelayCommand");
        }
    }
}
