using AulaSegura.Core.Entities;

namespace AulaSegura.Core.Interfaces;

/// <summary>
/// Servicio de gestión de categorías
/// </summary>
public interface ICategoryService
{
    Task<Category> CreateCategoryAsync(string name, string description, string color);
    Task UpdateCategoryAsync(Category category);
    Task DeleteCategoryAsync(int id);
    Task<Category?> GetCategoryByIdAsync(int id);
    Task<IEnumerable<Category>> GetAllCategoriesAsync();
    Task<IEnumerable<Category>> GetActiveCategoriesAsync();
}
