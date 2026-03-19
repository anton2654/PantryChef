using PantryChef.Data.Entities;
using System.Threading.Tasks;

namespace PantryChef.Data.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User> GetByEmailAsync(string email);
        Task<User> GetByIdentityUserIdAsync(string identityUserId);
    }
}