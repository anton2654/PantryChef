using Microsoft.EntityFrameworkCore;
using PantryChef.Data.Context;
using PantryChef.Data.Entities;
using PantryChef.Data.Interfaces;
using System;
using System.Threading.Tasks;

namespace PantryChef.Data.Repositories
{
    public class UserNutritionLogRepository : Repository<UserNutritionLog>, IUserNutritionLogRepository
    {
        public UserNutritionLogRepository(PantryChefDbContext context) : base(context)
        {
        }

        public async Task<UserNutritionLog> GetByUserAndDateAsync(int userId, DateTime logDate)
        {
            var normalizedDate = logDate.Date;

            return await _dbSet.FirstOrDefaultAsync(log =>
                log.UserId == userId && log.LogDate == normalizedDate);
        }
    }
}
