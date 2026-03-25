using PantryChef.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PantryChef.Business.Interfaces
{
    public interface IRecipeService
    {
        Task<IEnumerable<Recipe>> GetAllRecipesWithIngredientsAsync();
        Task<IEnumerable<Recipe>> GetRecipesByCategoryAsync(DishCategory category);
        Task<Recipe> GetRecipeWithIngredientsByIdAsync(int id);
    }
}