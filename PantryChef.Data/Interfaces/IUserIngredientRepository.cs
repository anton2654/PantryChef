using PantryChef.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PantryChef.Data.Interfaces
{
    public interface IUserIngredientRepository : IRepository<UserIngredient>
    {
        Task<IEnumerable<UserIngredient>> GetUserInventoryAsync(int userId);
        Task<UserIngredient> GetUserIngredientAsync(int userId, int ingredientId);
    }
}   