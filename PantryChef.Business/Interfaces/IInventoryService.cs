using PantryChef.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PantryChef.Business.Interfaces
{
    public interface IInventoryService
    {
        Task AddOrUpdateIngredientAsync(int userId, int ingredientId, double quantity);
        Task<IEnumerable<UserIngredient>> GetUserInventoryAsync(int userId);
    }
}