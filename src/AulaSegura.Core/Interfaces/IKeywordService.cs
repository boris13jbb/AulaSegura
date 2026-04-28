using System.Collections.Generic;
using System.Threading.Tasks;
using AulaSegura.Core.Entities;

namespace AulaSegura.Core.Interfaces
{
    public interface IKeywordService
    {
        Task<IEnumerable<Keyword>> GetAllKeywordsAsync();
        Task<Keyword?> GetKeywordByIdAsync(int id);
        Task<Keyword> CreateKeywordAsync(Keyword keyword);
        Task AddKeywordAsync(Keyword keyword);
        Task UpdateKeywordAsync(Keyword keyword);
        Task DeleteKeywordAsync(int id);
        Task<IEnumerable<Keyword>> GetKeywordsByCategoryAsync(int categoryId);
    }
}
