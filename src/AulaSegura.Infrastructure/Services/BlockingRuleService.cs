using System.Globalization;
using AulaSegura.Core.Entities;
using AulaSegura.Core.Interfaces;
using AulaSegura.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AulaSegura.Infrastructure.Services;

public class BlockingRuleService : IBlockingRuleService
{
    private const string DefaultRuleType = "CATEGORY";
    private const string DefaultAction = "Block";

    private readonly AulaSeguraDbContext _context;

    public BlockingRuleService(AulaSeguraDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<BlockingRule>> GetAllRulesAsync()
    {
        return await _context.BlockingRules
            .Include(r => r.Category)
            .Where(r => r.IsActive)
            .OrderBy(r => r.Name)
            .ToListAsync();
    }

    public async Task<BlockingRule?> GetRuleByIdAsync(int id)
    {
        return await _context.BlockingRules
            .Include(r => r.Category)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<BlockingRule> CreateRuleAsync(BlockingRule rule)
    {
        await NormalizeAndValidateAsync(rule);

        await _context.BlockingRules.AddAsync(rule);
        await _context.SaveChangesAsync();
        return rule;
    }

    public async Task AddRuleAsync(BlockingRule rule)
    {
        await CreateRuleAsync(rule);
    }

    public async Task UpdateRuleAsync(BlockingRule rule)
    {
        var existing = await _context.BlockingRules.FindAsync(rule.Id);
        if (existing == null)
            throw new InvalidOperationException("Regla de bloqueo no encontrada");

        await NormalizeAndValidateAsync(rule);

        existing.Name = rule.Name;
        existing.RuleType = rule.RuleType;
        existing.Value = rule.Value;
        existing.Action = rule.Action;
        existing.CategoryId = rule.CategoryId;
        existing.MaxViolations = rule.MaxViolations;
        existing.BlockDurationMinutes = rule.BlockDurationMinutes;
        existing.IsActive = rule.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteRuleAsync(int id)
    {
        var rule = await _context.BlockingRules.FindAsync(id);
        if (rule == null)
            return;

        rule.IsActive = false;
        rule.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<BlockingRule>> GetActiveRulesByTypeAsync(string ruleType)
    {
        var normalizedType = NormalizeRuleType(ruleType);

        return await _context.BlockingRules
            .Include(r => r.Category)
            .Where(r => r.RuleType == normalizedType && r.IsActive)
            .OrderBy(r => r.Name)
            .ToListAsync();
    }

    private async Task NormalizeAndValidateAsync(BlockingRule rule)
    {
        if (string.IsNullOrWhiteSpace(rule.Name))
            throw new ArgumentException("El nombre de la regla es obligatorio", nameof(rule));

        rule.Name = rule.Name.Trim();
        rule.RuleType = NormalizeRuleType(rule.RuleType);
        rule.Action = string.IsNullOrWhiteSpace(rule.Action)
            ? DefaultAction
            : rule.Action.Trim();
        rule.MaxViolations = Math.Max(1, rule.MaxViolations);
        rule.BlockDurationMinutes = Math.Max(1, rule.BlockDurationMinutes);

        if (rule.CategoryId is <= 0)
            rule.CategoryId = null;

        if (rule.RuleType == DefaultRuleType && rule.CategoryId == null)
            throw new ArgumentException("Las reglas por categoria requieren una categoria activa", nameof(rule));

        if (rule.CategoryId.HasValue)
        {
            var categoryExists = await _context.Categories
                .AnyAsync(c => c.Id == rule.CategoryId.Value && c.IsActive);

            if (!categoryExists)
                throw new InvalidOperationException("La categoria seleccionada no existe o esta inactiva");
        }

        if (string.IsNullOrWhiteSpace(rule.Value))
        {
            rule.Value = rule.CategoryId?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        }
        else
        {
            rule.Value = rule.Value.Trim();
        }

        rule.IsActive = true;
    }

    private static string NormalizeRuleType(string? ruleType)
    {
        return string.IsNullOrWhiteSpace(ruleType)
            ? DefaultRuleType
            : ruleType.Trim().ToUpperInvariant();
    }
}
