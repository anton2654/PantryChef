using PantryChef.Business.Models;
using PantryChef.Data.Entities;
using System.Threading.Tasks;

namespace PantryChef.Business.Interfaces
{
    public interface IAccountService
    {
        Task<Result<ApplicationUser>> RegisterUserAsync(string email, string password, string fullName);
        Task EnsureDomainUserLinkedAsync(ApplicationUser identityUser, string preferredName = null);
    }
}