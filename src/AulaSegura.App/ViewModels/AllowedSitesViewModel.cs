using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using AulaSegura.Core.Entities;
using AulaSegura.Core.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AulaSegura.App.ViewModels;

/// <summary>
/// ViewModel para gestión de sitios permitidos (whitelist)
/// </summary>
public partial class AllowedSitesViewModel : ObservableObject
{
    private readonly IAllowedSiteService _allowedSiteService;
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
    private ObservableCollection<AllowedSite> _allowedSites = new();

    [ObservableProperty]
    private AllowedSite? _selectedSite;

    [ObservableProperty]
    private string _searchText = string.Empty;

    // Form fields
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

    public ICommand LoadDataCommand { get; }
    public ICommand AddNewCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand SearchCommand { get; }
    public ICommand GoBackCommand { get; }

    public AllowedSitesViewModel(IAllowedSiteService allowedSiteService)
    {
        _allowedSiteService = allowedSiteService;

        LoadDataCommand = new AsyncRelayCommand(LoadDataAsync);
        AddNewCommand = new RelayCommand(ShowAddForm);
        EditCommand = new AsyncRelayCommand<AllowedSite>(EditSiteAsync);
        DeleteCommand = new AsyncRelayCommand<AllowedSite>(DeleteSiteAsync);
        SaveCommand = new AsyncRelayCommand(SaveSiteAsync, CanSaveSite);
        CancelCommand = new RelayCommand(HideForm);
        SearchCommand = new RelayCommand(FilterSites);
        GoBackCommand = new RelayCommand(GoBack);

        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            var sites = await _allowedSiteService.GetAllAllowedSitesAsync();
            AllowedSites.Clear();
            foreach (var site in sites)
            {
                AllowedSites.Add(site);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error al cargar datos: {ex.Message}";
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

    private async Task EditSiteAsync(AllowedSite? site)
    {
        if (site == null) return;

        IsEditMode = true;
        EditingSiteId = site.Id;
        FormDomain = site.Domain;
        FormDescription = site.Description ?? string.Empty;
        IsFormVisible = true;
        ErrorMessage = string.Empty;
    }

    private async Task DeleteSiteAsync(AllowedSite? site)
    {
        if (site == null) return;

        var result = MessageBox.Show(
            $"¿Eliminar '{site.Domain}' de la lista blanca?",
            "Confirmar",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                await _allowedSiteService.DeleteAllowedSiteAsync(site.Id);
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
            }
        }
    }

    private async Task SaveSiteAsync()
    {
        try
        {
            var domain = FormDomain.Trim().ToLower();
            
            if (IsEditMode && EditingSiteId.HasValue)
            {
                var site = new AllowedSite
                {
                    Id = EditingSiteId.Value,
                    Domain = domain,
                    Description = FormDescription,
                    IsActive = true
                };
                await _allowedSiteService.UpdateAllowedSiteAsync(site);
            }
            else
            {
                await _allowedSiteService.AddAllowedSiteAsync(domain, FormDescription, _currentAdminId);
            }

            HideForm();
            await LoadDataAsync();
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
        FormDomain = string.Empty;
        FormDescription = string.Empty;
    }

    private void FilterSites()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            _ = LoadDataAsync();
            return;
        }

        var filtered = AllowedSites
            .Where(s => s.Domain.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
            .ToList();

        AllowedSites.Clear();
        foreach (var site in filtered)
        {
            AllowedSites.Add(site);
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
