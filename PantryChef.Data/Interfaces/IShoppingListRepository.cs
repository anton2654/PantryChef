using PantryChef.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PantryChef.Data.Interfaces
{
    public interface IShoppingListRepository : IRepository<ShoppingListItem>
    {
        Task<IEnumerable<ShoppingListItem>> GetByUserAsync(int userId);
        Task<ShoppingListItem> GetItemAsync(int userId, int ingredientId);
    }
}
