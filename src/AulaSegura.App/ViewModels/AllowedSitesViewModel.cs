using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using AulaSegura.Core.Entities;
using AulaSegura.Core.Interfaces;
using AulaSegura.Core.Utilities;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AulaSegura.App.ViewModels;

/// <summary>
/// ViewModel para gestion de sitios permitidos (whitelist).
/// </summary>
public partial class AllowedSitesViewModel : ObservableObject
{
    private readonly IAllowedSiteService _allowedSiteService;
    private readonly IBlockedSiteService _blockedSiteService;
    private int _currentAdminId;
    private MainWindow? _mainWindow;

    public void SetParentWindow(MainWindow mainWindow)
    {
        _mainWindow = mainWindow;
    }

    public void SetCurrentAdminId(int adminId)
    {
        _currentAdminId = adminId;
    }

    [ObservableProperty]
    private ObservableCollection<AllowedSite> _allowedSites = new();

    [ObservableProperty]
    private AllowedSite? _selectedSite;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _formDomain = string.Empty;

    [ObservableProperty]
    private string _formDescription = string.Empty;

    [ObservableProperty]
    private bool _isFormVisible = false;

    [ObservableProperty]
    private bool _isEditMode = false;

    [ObservableProperty]
    private int? _editingSiteId = null;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    public ICommand LoadDataCommand { get; }
    public ICommand AddNewCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand SearchCommand { get; }
    public ICommand GoBackCommand { get; }

    public AllowedSitesViewModel(
        IAllowedSiteService allowedSiteService,
        IBlockedSiteService blockedSiteService)
    {
        _allowedSiteService = allowedSiteService;
        _blockedSiteService = blockedSiteService;

        LoadDataCommand = new AsyncRelayCommand(LoadDataAsync);
        AddNewCommand = new RelayCommand(ShowAddForm);
        EditCommand = new AsyncRelayCommand<AllowedSite>(EditSiteAsync);
        DeleteCommand = new AsyncRelayCommand<AllowedSite>(DeleteSiteAsync);
        SaveCommand = new AsyncRelayCommand(SaveSiteAsync, CanSaveSite);
        CancelCommand = new RelayCommand(HideForm);
        SearchCommand = new AsyncRelayCommand(LoadDataAsync);
        GoBackCommand = new RelayCommand(GoBack);

        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var sites = (await _allowedSiteService.GetAllAllowedSitesAsync()).ToList();
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var query = SearchText.Trim();
                sites = sites
                    .Where(s => s.Domain.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                                s.Description.Contains(query, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            AllowedSites.Clear();
            foreach (var site in sites)
            {
                AllowedSites.Add(site);
            }
        }
        catch
        {
            ErrorMessage = "No se pudo cargar la lista blanca.";
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
        FormDescription = string.Empty;
        IsFormVisible = true;
        ErrorMessage = string.Empty;
    }

    private Task EditSiteAsync(AllowedSite? site)
    {
        if (site == null)
            return Task.CompletedTask;

        IsEditMode = true;
        EditingSiteId = site.Id;
        FormDomain = site.Domain;
        FormDescription = site.Description ?? string.Empty;
        IsFormVisible = true;
        ErrorMessage = string.Empty;

        return Task.CompletedTask;
    }

    private async Task DeleteSiteAsync(AllowedSite? site)
    {
        if (site == null)
            return;

        var result = MessageBox.Show(
            $"Eliminar '{site.Domain}' de la lista blanca?",
            "Confirmar",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes)
            return;

        try
        {
            await _allowedSiteService.DeleteAllowedSiteAsync(site.Id);
            var syncWarning = await SyncBlockingRulesAsync();
            await LoadDataAsync();
            if (!string.IsNullOrWhiteSpace(syncWarning))
                ErrorMessage = syncWarning;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error: {ex.Message}";
        }
    }

    private async Task SaveSiteAsync()
    {
        try
        {
            if (_currentAdminId <= 0)
            {
                ErrorMessage = "No se encontro la sesion del administrador actual.";
                return;
            }

            var normalizedDomain = ValidationHelper.NormalizeDomain(FormDomain);
            if (!ValidationHelper.IsValidDomain(normalizedDomain))
            {
                ErrorMessage = "Dominio no valido.";
                return;
            }

            if (IsEditMode && EditingSiteId.HasValue)
            {
                await _allowedSiteService.UpdateAllowedSiteAsync(new AllowedSite
                {
                    Id = EditingSiteId.Value,
                    Domain = normalizedDomain,
                    Description = FormDescription,
                    IsActive = true
                });
            }
            else
            {
                await _allowedSiteService.AddAllowedSiteAsync(normalizedDomain, FormDescription, _currentAdminId);
            }

            HideForm();
            var syncWarning = await SyncBlockingRulesAsync();
            await LoadDataAsync();
            if (!string.IsNullOrWhiteSpace(syncWarning))
                ErrorMessage = syncWarning;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al guardar: {ex.Message}";
        }
    }

    private bool CanSaveSite() => !string.IsNullOrWhiteSpace(FormDomain);

    private void HideForm()
    {
        IsFormVisible = false;
        IsEditMode = false;
        EditingSiteId = null;
        FormDomain = string.Empty;
        FormDescription = string.Empty;
    }

    private async Task<string?> SyncBlockingRulesAsync()
    {
        try
        {
            await _blockedSiteService.ApplyBlockingRulesAsync(writeAuditLogEntry: false);
            return null;
        }
        catch (InvalidOperationException ex)
        {
            return $"Cambios guardados, pero no se pudo sincronizar hosts: {ex.Message}";
        }
    }

    partial void OnFormDomainChanged(string value)
    {
        ((AsyncRelayCommand)SaveCommand).NotifyCanExecuteChanged();
    }

    private void GoBack()
    {
        _mainWindow?.NavigateToDashboard();
    }
}
