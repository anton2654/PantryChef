using PantryChef.Business.Models;
using PantryChef.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PantryChef.Business.Interfaces
{
    public interface IInventoryService
    {
        Task<Result> AddOrUpdateIngredientAsync(int userId, int ingredientId, double quantity);
        Task<IEnumerable<UserIngredient>> GetUserInventoryAsync(int userId);
    }
}