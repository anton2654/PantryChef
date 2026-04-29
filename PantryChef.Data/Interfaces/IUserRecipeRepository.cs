using PantryChef.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PantryChef.Data.Interfaces
{
    public interface IUserRecipeRepository : IRepository<UserRecipe>
    {
        Task<UserRecipe> GetAsync(int userId, int recipeId);
        Task<IReadOnlyList<int>> GetHiddenRecipeIdsAsync(int userId);
    }
}
