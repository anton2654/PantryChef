using Microsoft.EntityFrameworkCore;
using PantryChef.Data.Context;
using PantryChef.Data.Entities;
using PantryChef.Data.Interfaces;
using System.Threading.Tasks;

namespace PantryChef.Data.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(PantryChefDbContext context) : base(context)
        {
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User> GetByIdentityUserIdAsync(string identityUserId)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.IdentityUserId == identityUserId);
        }
    }
}