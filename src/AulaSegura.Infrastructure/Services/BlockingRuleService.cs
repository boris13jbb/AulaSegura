using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AulaSegura.Core.Entities;
using AulaSegura.Core.Interfaces;
using AulaSegura.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AulaSegura.Infrastructure.Services
{
    public class BlockingRuleService : IBlockingRuleService
    {
        private readonly AulaSeguraDbContext _context;

        public BlockingRuleService(AulaSeguraDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BlockingRule>> GetAllRulesAsync()
        {
            return await _context.BlockingRules
                .OrderBy(r => r.Name)
                .ToListAsync();
        }

        public async Task<BlockingRule?> GetRuleByIdAsync(int id)
        {
            return await _context.BlockingRules
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<BlockingRule> CreateRuleAsync(BlockingRule rule)
        {
            _context.BlockingRules.Add(rule);
            await _context.SaveChangesAsync();
            return rule;
        }

        public async Task AddRuleAsync(BlockingRule rule)
        {
            _context.BlockingRules.Add(rule);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateRuleAsync(BlockingRule rule)
        {
            _context.BlockingRules.Update(rule);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteRuleAsync(int id)
        {
            var rule = await _context.BlockingRules.FindAsync(id);
            if (rule != null)
            {
                _context.BlockingRules.Remove(rule);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<BlockingRule>> GetActiveRulesByTypeAsync(string ruleType)
        {
            return await _context.BlockingRules
                .Where(r => r.RuleType == ruleType && r.IsActive)
                .ToListAsync();
        }
    }
}
