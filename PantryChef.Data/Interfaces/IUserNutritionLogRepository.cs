using PantryChef.Data.Entities;
using System;
using System.Threading.Tasks;

namespace PantryChef.Data.Interfaces
{
    public interface IUserNutritionLogRepository : IRepository<UserNutritionLog>
    {
        Task<UserNutritionLog> GetByUserAndDateAsync(int userId, DateTime logDate);
    }
}
