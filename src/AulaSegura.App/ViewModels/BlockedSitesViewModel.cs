using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using AulaSegura.Core.Entities;
using AulaSegura.Core.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AulaSegura.App.ViewModels;

/// <summary>
/// ViewModel para gestión de sitios bloqueados (CRUD)
/// </summary>
public partial class BlockedSitesViewModel : ObservableObject
{
    private readonly IBlockedSiteService _blockedSiteService;
    private readonly ICategoryService _categoryService;
    private readonly int _currentAdminId = 1; // TODO: Get from logged-in admin
    private MainWindow? _mainWindow;

    /// <summary>
    /// Sets the parent MainWindow reference for navigation
    /// </summary>
    public void SetParentWindow(MainWindow mainWindow)
    {
        _mainWindow = mainWindow;
    }

    [ObservableProperty]
    private ObservableCollection<BlockedSite> _blockedSites = new();

    [ObservableProperty]
    private ObservableCollection<Category> _categories = new();

    [ObservableProperty]
    private BlockedSite? _selectedSite;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private int _selectedCategoryId = 0;

    // Form fields for Add/Edit
    [ObservableProperty]
    private string _formDomain = string.Empty;

    [ObservableProperty]
    private string _formReason = string.Empty;

    [ObservableProperty]
    private int _formCategoryId = 0;

    [ObservableProperty]
    private bool _isFormVisible = false;

    [ObservableProperty]
    private bool _isEditMode = false;

    [ObservableProperty]
    private int? _editingSiteId = null;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isLoading = false;

    public ICommand LoadDataCommand { get; }
    public ICommand AddNewCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand ApplyRulesCommand { get; }
    public ICommand SearchCommand { get; }
    public ICommand GoBackCommand { get; }

    public BlockedSitesViewModel(
        IBlockedSiteService blockedSiteService,
        ICategoryService categoryService)
    {
        _blockedSiteService = blockedSiteService;
        _categoryService = categoryService;

        LoadDataCommand = new AsyncRelayCommand(LoadDataAsync);
        AddNewCommand = new RelayCommand(ShowAddForm);
        EditCommand = new AsyncRelayCommand<BlockedSite>(EditSiteAsync);
        DeleteCommand = new AsyncRelayCommand<BlockedSite>(DeleteSiteAsync);
        SaveCommand = new AsyncRelayCommand(SaveSiteAsync, CanSaveSite);
        CancelCommand = new RelayCommand(HideForm);
        ApplyRulesCommand = new AsyncRelayCommand(ApplyBlockingRulesAsync);
        SearchCommand = new RelayCommand(FilterSites);
        GoBackCommand = new RelayCommand(GoBack);

        // Load initial data
        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            // Load blocked sites
            var sites = await _blockedSiteService.GetAllBlockedSitesAsync();
            BlockedSites.Clear();
            foreach (var site in sites)
            {
                BlockedSites.Add(site);
            }

            // Load categories for dropdown
            var categories = await _categoryService.GetAllCategoriesAsync();
            Categories.Clear();
            foreach (var category in categories)
            {
                Categories.Add(category);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al cargar datos: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ShowAddForm()
    {
        IsEditMode = false;
        EditingSiteId = null;
        FormDomain = string.Empty;
        FormReason = string.Empty;
        FormCategoryId = Categories.Any() ? Categories.First().Id : 0;
        IsFormVisible = true;
        ErrorMessage = string.Empty;
    }

    private async Task EditSiteAsync(BlockedSite? site)
    {
        if (site == null) return;

        IsEditMode = true;
        EditingSiteId = site.Id;
        FormDomain = site.Domain;
        FormReason = site.Reason ?? string.Empty;
        FormCategoryId = site.CategoryId;
        IsFormVisible = true;
        ErrorMessage = string.Empty;
    }

    private async Task DeleteSiteAsync(BlockedSite? site)
    {
        if (site == null) return;

        var result = MessageBox.Show(
            $"¿Está seguro de eliminar el sitio '{site.Domain}'?\n\nEsta acción desactivará el bloqueo pero mantendrá el registro.",
            "Confirmar Eliminación",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                await _blockedSiteService.DeleteBlockedSiteAsync(site.Id);
                await LoadDataAsync();
                MessageBox.Show("Sitio eliminado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al eliminar: {ex.Message}";
            }
        }
    }

    private async Task SaveSiteAsync()
    {
        try
        {
            // Validation
            if (string.IsNullOrWhiteSpace(FormDomain))
            {
                ErrorMessage = "El dominio es obligatorio.";
                return;
            }

            // Normalize domain (remove protocol, lowercase)
            var domain = FormDomain.Trim().ToLower();
            if (domain.StartsWith("http://") || domain.StartsWith("https://"))
            {
                domain = domain.Replace("http://", "").Replace("https://", "");
            }
            domain = domain.TrimEnd('/');

            if (IsEditMode && EditingSiteId.HasValue)
            {
                // Update existing site
                var site = new BlockedSite
                {
                    Id = EditingSiteId.Value,
                    Domain = domain,
                    Reason = FormReason,
                    CategoryId = FormCategoryId,
                    IsActive = true
                };

                await _blockedSiteService.UpdateBlockedSiteAsync(site);
                MessageBox.Show("Sitio actualizado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                // Add new site
                await _blockedSiteService.AddBlockedSiteAsync(domain, FormCategoryId, FormReason, _currentAdminId);
                MessageBox.Show("Sitio agregado o reactivado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            HideForm();
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al guardar: {ex.Message}";
        }
    }

    private bool CanSaveSite()
    {
        return !string.IsNullOrWhiteSpace(FormDomain) && FormCategoryId > 0;
    }

    private void HideForm()
    {
        IsFormVisible = false;
        IsEditMode = false;
        EditingSiteId = null;
        FormDomain = string.Empty;
        FormReason = string.Empty;
        FormCategoryId = 0;
        ErrorMessage = string.Empty;
    }

    private async Task ApplyBlockingRulesAsync()
    {
        try
        {
            IsLoading = true;
            await _blockedSiteService.ApplyBlockingRulesAsync();
            MessageBox.Show(
                "Reglas de bloqueo aplicadas correctamente.\n\nEl archivo hosts ha sido actualizado.",
                "Éxito",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (UnauthorizedAccessException)
        {
            MessageBox.Show(
                "Error: Se requieren privilegios de administrador para modificar el archivo hosts.\n\nEjecute la aplicación como administrador.",
                "Error de Permisos",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Error al aplicar reglas: {ex.Message}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void FilterSites()
    {
        // Simple client-side filtering
        // In production, this should use database queries
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            _ = LoadDataAsync();
            return;
        }

        var filtered = BlockedSites
            .Where(s => s.Domain.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                       (s.Reason != null && s.Reason.Contains(SearchText, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        BlockedSites.Clear();
        foreach (var site in filtered)
        {
            BlockedSites.Add(site);
        }
    }

    partial void OnFormDomainChanged(string value)
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
