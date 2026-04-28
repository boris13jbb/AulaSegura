using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AulaSegura.Core.Entities;
using AulaSegura.Core.Interfaces;
using AulaSegura.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AulaSegura.Infrastructure.Services
{
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
            _context.Keywords.Add(keyword);
            await _context.SaveChangesAsync();
            return keyword;
        }

        public async Task AddKeywordAsync(Keyword keyword)
        {
            _context.Keywords.Add(keyword);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateKeywordAsync(Keyword keyword)
        {
            _context.Keywords.Update(keyword);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteKeywordAsync(int id)
        {
            var keyword = await _context.Keywords.FindAsync(id);
            if (keyword != null)
            {
                _context.Keywords.Remove(keyword);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Keyword>> GetKeywordsByCategoryAsync(int categoryId)
        {
            return await _context.Keywords
                .Where(k => k.CategoryId == categoryId && k.IsActive)
                .ToListAsync();
        }
    }
}
