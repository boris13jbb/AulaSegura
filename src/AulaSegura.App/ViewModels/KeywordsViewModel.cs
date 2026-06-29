using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using AulaSegura.Core.Entities;
using AulaSegura.Core.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AulaSegura.App.ViewModels;

public partial class KeywordsViewModel : ObservableObject
{
    private readonly IKeywordService _keywordService;
    private readonly List<Keyword> _allKeywords = [];
    private MainWindow? _mainWindow;

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
        EditKeywordCommand = new RelayCommand<Keyword>(EditKeyword, keyword => keyword != null);
        CancelEditCommand = new RelayCommand(CancelEdit);
        SearchCommand = new RelayCommand(ApplyFilter);
        GoBackCommand = new RelayCommand(GoBack);
    }

    public ICommand LoadKeywordsCommand { get; }
    public ICommand AddKeywordCommand { get; }
    public ICommand UpdateKeywordCommand { get; }
    public ICommand DeleteKeywordCommand { get; }
    public ICommand EditKeywordCommand { get; }
    public ICommand CancelEditCommand { get; }
    public ICommand SearchCommand { get; }
    public ICommand GoBackCommand { get; }
    public IReadOnlyList<KeywordType> KeywordTypes { get; } =
    [
        KeywordType.Blocked,
        KeywordType.Allowed,
        KeywordType.Warning
    ];

    public void SetParentWindow(MainWindow mainWindow)
    {
        _mainWindow = mainWindow;
    }

    private async Task LoadKeywordsAsync()
    {
        IsLoading = true;

        try
        {
            var keywords = await _keywordService.GetAllKeywordsAsync();

            _allKeywords.Clear();
            _allKeywords.AddRange(keywords);
            ApplyFilter();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error al cargar palabras clave: {ex.Message}";
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
            StatusMessage = "La palabra clave no puede estar vacia";
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
            StatusMessage = $"Error al agregar palabra clave: {ex.Message}";
        }
    }

    private void EditKeyword(Keyword? keyword)
    {
        if (keyword == null)
            return;

        SelectedKeyword = keyword;
        NewKeyword = keyword.Word;
        SelectedType = keyword.Type;
        IsEditing = true;
        StatusMessage = $"Editando: {keyword.Word}";
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
            ResetForm();
            await LoadKeywordsAsync();
            StatusMessage = "Palabra clave actualizada exitosamente";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error al actualizar palabra clave: {ex.Message}";
        }
    }

    private async Task DeleteKeywordAsync(Keyword? keyword)
    {
        if (keyword == null)
            return;

        var result = MessageBox.Show(
            $"Eliminar la palabra clave '{keyword.Word}'?",
            "Confirmar eliminacion",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes)
            return;

        try
        {
            await _keywordService.DeleteKeywordAsync(keyword.Id);
            await LoadKeywordsAsync();
            StatusMessage = "Palabra clave eliminada";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error al eliminar palabra clave: {ex.Message}";
        }
    }

    private void ApplyFilter()
    {
        var filter = SearchText.Trim();
        var filtered = string.IsNullOrWhiteSpace(filter)
            ? _allKeywords
            : _allKeywords
                .Where(keyword => keyword.Word.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                                  keyword.Type.ToString().Contains(filter, StringComparison.OrdinalIgnoreCase));

        Keywords.Clear();
        foreach (var keyword in filtered.OrderBy(keyword => keyword.Word))
        {
            Keywords.Add(keyword);
        }

        StatusMessage = string.IsNullOrWhiteSpace(filter)
            ? $"Cargadas {Keywords.Count} palabras clave"
            : $"Filtro aplicado: {Keywords.Count} resultado(s)";
    }

    private void CancelEdit()
    {
        ResetForm();
        StatusMessage = "Edicion cancelada";
    }

    private void ResetForm()
    {
        SelectedKeyword = null;
        NewKeyword = string.Empty;
        SelectedType = KeywordType.Blocked;
        IsEditing = false;
    }

    private void GoBack()
    {
        _mainWindow?.NavigateToDashboard();
    }

    partial void OnSearchTextChanged(string value)
    {
        ApplyFilter();
    }

    partial void OnSelectedKeywordChanged(Keyword? value)
    {
        if (UpdateKeywordCommand is AsyncRelayCommand command)
        {
            command.NotifyCanExecuteChanged();
        }
    }
}
