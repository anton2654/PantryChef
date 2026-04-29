using Microsoft.EntityFrameworkCore;
using PantryChef.Data.Context;
using PantryChef.Data.Entities;
using PantryChef.Data.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PantryChef.Data.Repositories
{
    public class UserRecipeRepository : Repository<UserRecipe>, IUserRecipeRepository
    {
        public UserRecipeRepository(PantryChefDbContext context) : base(context)
        {
        }

        public async Task<UserRecipe> GetAsync(int userId, int recipeId)
        {
            return await _dbSet.FirstOrDefaultAsync(link => link.UserId == userId && link.RecipeId == recipeId);
        }

        public async Task<IReadOnlyList<int>> GetHiddenRecipeIdsAsync(int userId)
        {
            return await _dbSet
                .Where(link => link.UserId == userId && !link.IsSaved)
                .Select(link => link.RecipeId)
                .ToListAsync();
        }
    }
}
