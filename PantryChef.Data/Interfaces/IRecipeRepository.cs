using PantryChef.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PantryChef.Data.Interfaces
{
    public interface IRecipeRepository : IRepository<Recipe>
    {
        Task AddRecipeAsync(Recipe recipe);

        void UpdateRecipe(Recipe recipe);

        void DeleteRecipe(Recipe recipe);

        Task<Recipe> GetRecipeByIdAsync(int id);

        Task<IEnumerable<string>> GetAvailableCategoriesAsync();

        Task<IEnumerable<Recipe>> GetAllRecipesWithIngredientsAsync();

        Task<Recipe> GetRecipeWithIngredientsByIdAsync(int id);

        Task<IEnumerable<Recipe>> GetRecipesByCategoryAsync(string category);
    }
}