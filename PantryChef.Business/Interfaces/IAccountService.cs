using PantryChef.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PantryChef.Business.Interfaces
{
    public interface IAccountService
    {
        Task<(bool Succeeded, IEnumerable<string> Errors, ApplicationUser User)> RegisterUserAsync(string email, string password, string fullName);
        Task EnsureDomainUserLinkedAsync(ApplicationUser identityUser, string preferredName = null);
    }
}