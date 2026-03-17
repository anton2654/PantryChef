using Microsoft.EntityFrameworkCore;
using PantryChef.Data.Context;
using PantryChef.Data.Entities;
using PantryChef.Data.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PantryChef.Data.Repositories
{
    public class UserIngredientRepository : Repository<UserIngredient>, IUserIngredientRepository
    {
        public UserIngredientRepository(PantryChefDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<UserIngredient>> GetUserInventoryAsync(int userId)
        {
            return await _dbSet
                .Include(ui => ui.Ingredient)
                .Where(ui => ui.UserId == userId)
                .ToListAsync();
        }

        public async Task<UserIngredient> GetUserIngredientAsync(int userId, int ingredientId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(ui => ui.UserId == userId && ui.IngredientId == ingredientId);
        }
    }
}