using PantryChef.Business.Models;
using PantryChef.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PantryChef.Business.Interfaces
{
    public interface IInventoryService
    {
        Task<Result> AddOrUpdateIngredientAsync(int userId, int ingredientId, double quantity);
        
        Task<IEnumerable<UserIngredient>> GetUserInventoryAsync(int userId, string category = null, string searchQuery = null);

        Task<IEnumerable<Ingredient>> GetAvailableIngredientsAsync();
        
        Task<IEnumerable<string>> GetUserInventoryCategoriesAsync(int userId);

        Task<Result> UpdateIngredientQuantityAsync(int userId, int ingredientId, double newQuantity);
        
        Task<Result> RemoveIngredientAsync(int userId, int ingredientId);

        Task<Result> AddMissingIngredientsToShoppingListAsync(int userId, int recipeId);

        Task<Result> CookRecipeAsync(int userId, int recipeId);

        Task<Result<IReadOnlyList<ShoppingListItem>>> GetShoppingListAsync(int userId);
    }
}