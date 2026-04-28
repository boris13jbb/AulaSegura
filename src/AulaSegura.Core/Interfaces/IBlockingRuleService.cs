using System.Collections.Generic;
using System.Threading.Tasks;
using AulaSegura.Core.Entities;

namespace AulaSegura.Core.Interfaces
{
    public interface IBlockingRuleService
    {
        Task<IEnumerable<BlockingRule>> GetAllRulesAsync();
        Task<BlockingRule?> GetRuleByIdAsync(int id);
        Task<BlockingRule> CreateRuleAsync(BlockingRule rule);
        Task AddRuleAsync(BlockingRule rule);
        Task UpdateRuleAsync(BlockingRule rule);
        Task DeleteRuleAsync(int id);
        Task<IEnumerable<BlockingRule>> GetActiveRulesByTypeAsync(string ruleType);
    }
}
