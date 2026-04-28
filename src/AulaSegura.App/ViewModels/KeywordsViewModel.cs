using System.Collections.ObjectModel;
using System.Windows.Input;
using AulaSegura.Core.Entities;
using AulaSegura.Core.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;

namespace AulaSegura.App.ViewModels;

public partial class KeywordsViewModel : ObservableObject
{
    private readonly IKeywordService _keywordService;
    private MainWindow? _mainWindow;

    /// <summary>
    /// Sets the parent MainWindow reference for navigation
    /// </summary>
    public void SetParentWindow(MainWindow mainWindow)
    {
        _mainWindow = mainWindow;
    }

    [ObservableProperty]
    private ObservableCollection<Keyword> _keywords = new();

    [ObservableProperty]
    private string _newKeyword = string.Empty;

    [ObservableProperty]
    private KeywordType _selectedType = KeywordType.Blocked;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private Keyword? _selectedKeyword;

    [ObservableProperty]
    private bool _isEditing;

    public KeywordsViewModel(IKeywordService keywordService)
    {
        _keywordService = keywordService;
        LoadKeywordsCommand = new AsyncRelayCommand(LoadKeywordsAsync);
        AddKeywordCommand = new AsyncRelayCommand(AddKeywordAsync);
        UpdateKeywordCommand = new AsyncRelayCommand(UpdateKeywordAsync, () => SelectedKeyword != null);
        DeleteKeywordCommand = new AsyncRelayCommand<Keyword>(DeleteKeywordAsync);
        EditKeywordCommand = new RelayCommand<Keyword>(EditKeyword, k => k != null);
        CancelEditCommand = new RelayCommand(CancelEdit);
        SearchCommand = new RelayCommand(FilterKeywords);
        GoBackCommand = new RelayCommand(GoBack);
        
        Debug.WriteLine($"[INIT] KeywordsViewModel inicializado");
        Debug.WriteLine($"[INIT] EditKeywordCommand: {EditKeywordCommand != null}");
        Debug.WriteLine($"[INIT] DeleteKeywordCommand: {DeleteKeywordCommand != null}");
        Debug.WriteLine($"[INIT] UpdateKeywordCommand: {UpdateKeywordCommand != null}");
    }

    public ICommand LoadKeywordsCommand { get; }
    public ICommand AddKeywordCommand { get; }
    public ICommand UpdateKeywordCommand { get; }
    public ICommand DeleteKeywordCommand { get; }
    public ICommand EditKeywordCommand { get; }
    public ICommand CancelEditCommand { get; }
    public ICommand SearchCommand { get; }
    public ICommand GoBackCommand { get; }

    private async Task LoadKeywordsAsync()
    {
        IsLoading = true;
        try
        {
            var keywords = await _keywordService.GetAllKeywordsAsync();
            Keywords.Clear();
            foreach (var keyword in keywords)
            {
                Keywords.Add(keyword);
            }
            StatusMessage = $"Cargadas {Keywords.Count} palabras clave";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error al cargar: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task AddKeywordAsync()
    {
        if (string.IsNullOrWhiteSpace(NewKeyword))
        {
            StatusMessage = "La palabra clave no puede estar vacía";
            return;
        }

        try
        {
            var keyword = new Keyword
            {
                Word = NewKeyword.Trim(),
                Type = SelectedType,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _keywordService.AddKeywordAsync(keyword);
            NewKeyword = string.Empty;
            await LoadKeywordsAsync();
            StatusMessage = "Palabra clave agregada exitosamente";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error al agregar: {ex.Message}";
        }
    }

    private void EditKeyword(Keyword? keyword)
    {
        if (keyword == null) 
        {
            Debug.WriteLine("[ERROR] EditKeyword llamado con keyword nulo");
            return;
        }
        
        Debug.WriteLine($"[INFO] Editando keyword: {keyword.Word}, ID: {keyword.Id}");
        SelectedKeyword = keyword;
        NewKeyword = keyword.Word;
        SelectedType = keyword.Type;
        IsEditing = true;
        StatusMessage = $"Editando: {keyword.Word}";
        Debug.WriteLine($"[INFO] SelectedKeyword establecido: {SelectedKeyword?.Word}");
        Debug.WriteLine($"[INFO] UpdateKeywordCommand CanExecute: {UpdateKeywordCommand.CanExecute(null)}");
    }

    private async Task UpdateKeywordAsync()
    {
        if (SelectedKeyword == null || string.IsNullOrWhiteSpace(NewKeyword))
        {
            StatusMessage = "Seleccione una palabra clave para editar";
            return;
        }

        try
        {
            SelectedKeyword.Word = NewKeyword.Trim();
            SelectedKeyword.Type = SelectedType;
            SelectedKeyword.UpdatedAt = DateTime.UtcNow;

            await _keywordService.UpdateKeywordAsync(SelectedKeyword);
            CancelEdit();
            await LoadKeywordsAsync();
            StatusMessage = "Palabra clave actualizada exitosamente";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error al actualizar: {ex.Message}";
        }
    }

    private void CancelEdit()
    {
        SelectedKeyword = null;
        NewKeyword = string.Empty;
        SelectedType = KeywordType.Blocked;
        IsEditing = false;
        StatusMessage = "Edición cancelada";
    }

    private async Task DeleteKeywordAsync(Keyword? keyword)
    {
        if (keyword == null) return;

        try
        {
            await _keywordService.DeleteKeywordAsync(keyword.Id);
            await LoadKeywordsAsync();
            StatusMessage = "Palabra clave eliminada";
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

    private void FilterKeywords()
    {
        // Implementación básica de filtrado
        // En una implementación completa, esto filtraría la colección observable
    }

    partial void OnSelectedKeywordChanged(Keyword? value)
    {
        Debug.WriteLine($"[DEBUG] OnSelectedKeywordChanged llamado. Valor: {value?.Word ?? "null"}");
        if (UpdateKeywordCommand is AsyncRelayCommand cmd)
        {
            cmd.NotifyCanExecuteChanged();
            Debug.WriteLine($"[DEBUG] NotifyCanExecuteChanged llamado. CanExecute ahora: {cmd.CanExecute(null)}");
        }
        else
        {
            Debug.WriteLine("[ERROR] UpdateKeywordCommand no es AsyncRelayCommand");
        }
    }
}
