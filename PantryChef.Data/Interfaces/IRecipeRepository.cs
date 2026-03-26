using PantryChef.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PantryChef.Data.Interfaces
{
    public interface IRecipeRepository : IRepository<Recipe>
    {
        Task<IEnumerable<Recipe>> GetAllRecipesWithIngredientsAsync();

        Task<Recipe> GetRecipeWithIngredientsByIdAsync(int id);
        Task<IEnumerable<Recipe>> GetRecipesByCategoryAsync(string category);
    }
}