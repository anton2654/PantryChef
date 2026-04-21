using PantryChef.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using PantryChef.Business.Models;

namespace PantryChef.Business.Interfaces
{
    public interface IRecipeService
    {
        Task<IEnumerable<Recipe>> GetAllRecipesWithIngredientsAsync();

        Task<IEnumerable<Recipe>> GetRecipesByCategoryAsync(string category);

        Task<IEnumerable<string>> GetAvailableCategoriesAsync();

        Task<Recipe> GetRecipeWithIngredientsByIdAsync(int id);

        Task<Result<int>> AddRecipeAsync(RecipeCreateModel model);

        Task<Result> EditRecipeAsync(RecipeEditModel model);

        Task<Result> DeleteRecipeAsync(int recipeId);

        Task<Result<RecipeEditModel>> GetRecipeForEditAsync(int recipeId);

        Task<Result<RecipeDeleteModel>> GetRecipeForDeleteAsync(int recipeId);
    }
}