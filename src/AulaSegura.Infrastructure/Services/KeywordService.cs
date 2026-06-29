using AulaSegura.Core.Entities;
using AulaSegura.Core.Interfaces;
using AulaSegura.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AulaSegura.Infrastructure.Services;

public class KeywordService : IKeywordService
{
    private readonly AulaSeguraDbContext _context;

    public KeywordService(AulaSeguraDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Keyword>> GetAllKeywordsAsync()
    {
        return await _context.Keywords
            .Include(k => k.Category)
            .Where(k => k.IsActive)
            .OrderBy(k => k.Word)
            .ToListAsync();
    }

    public async Task<Keyword?> GetKeywordByIdAsync(int id)
    {
        return await _context.Keywords
            .Include(k => k.Category)
            .FirstOrDefaultAsync(k => k.Id == id);
    }

    public async Task<Keyword> CreateKeywordAsync(Keyword keyword)
    {
        await NormalizeAndValidateAsync(keyword);

        var inactive = await FindInactiveDuplicateAsync(keyword);
        if (inactive != null)
        {
            inactive.Word = keyword.Word;
            inactive.Type = keyword.Type;
            inactive.CategoryId = keyword.CategoryId;
            inactive.IsActive = true;
            inactive.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return inactive;
        }

        await _context.Keywords.AddAsync(keyword);
        await _context.SaveChangesAsync();
        return keyword;
    }

    public async Task AddKeywordAsync(Keyword keyword)
    {
        await CreateKeywordAsync(keyword);
    }

    public async Task UpdateKeywordAsync(Keyword keyword)
    {
        var existing = await _context.Keywords.FindAsync(keyword.Id);
        if (existing == null)
            throw new InvalidOperationException("Palabra clave no encontrada");

        await NormalizeAndValidateAsync(keyword, existing.Id);

        existing.Word = keyword.Word;
        existing.Type = keyword.Type;
        existing.CategoryId = keyword.CategoryId;
        existing.IsActive = keyword.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteKeywordAsync(int id)
    {
        var keyword = await _context.Keywords.FindAsync(id);
        if (keyword == null)
            return;

        keyword.IsActive = false;
        keyword.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Keyword>> GetKeywordsByCategoryAsync(int categoryId)
    {
        return await _context.Keywords
            .Include(k => k.Category)
            .Where(k => k.CategoryId == categoryId && k.IsActive)
            .OrderBy(k => k.Word)
            .ToListAsync();
    }

    private async Task NormalizeAndValidateAsync(Keyword keyword, int? currentId = null)
    {
        if (string.IsNullOrWhiteSpace(keyword.Word))
            throw new ArgumentException("La palabra clave es obligatoria", nameof(keyword));

        keyword.Word = keyword.Word.Trim();

        if (keyword.CategoryId is <= 0)
            keyword.CategoryId = null;

        if (keyword.CategoryId.HasValue)
        {
            var categoryExists = await _context.Categories
                .AnyAsync(c => c.Id == keyword.CategoryId.Value && c.IsActive);

            if (!categoryExists)
                throw new InvalidOperationException("La categoria seleccionada no existe o esta inactiva");
        }

        var normalizedWord = keyword.Word.ToLower();
        var duplicate = await _context.Keywords.AnyAsync(k =>
            k.IsActive &&
            k.Type == keyword.Type &&
            k.Word.ToLower() == normalizedWord &&
            (!currentId.HasValue || k.Id != currentId.Value));

        if (duplicate)
            throw new InvalidOperationException("Ya existe una palabra clave activa con ese texto y tipo");

        keyword.IsActive = true;
    }

    private async Task<Keyword?> FindInactiveDuplicateAsync(Keyword keyword)
    {
        var normalizedWord = keyword.Word.ToLower();

        return await _context.Keywords.FirstOrDefaultAsync(k =>
            !k.IsActive &&
            k.Type == keyword.Type &&
            k.Word.ToLower() == normalizedWord);
    }
}
