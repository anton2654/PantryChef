using Microsoft.EntityFrameworkCore;
using PantryChef.Data.Context;
using PantryChef.Data.Entities;
using PantryChef.Data.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PantryChef.Data.Repositories
{
    public class ShoppingListRepository : Repository<ShoppingListItem>, IShoppingListRepository
    {
        public ShoppingListRepository(PantryChefDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<ShoppingListItem>> GetByUserAsync(int userId)
        {
            return await _dbSet
                .Include(item => item.Ingredient)
                .Where(item => item.UserId == userId)
                .OrderBy(item => item.Ingredient.Name)
                .ToListAsync();
        }

        public async Task<ShoppingListItem> GetItemAsync(int userId, int ingredientId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(item => item.UserId == userId && item.IngredientId == ingredientId);
        }
    }
}
