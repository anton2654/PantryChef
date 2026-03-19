using Microsoft.AspNetCore.Identity;

namespace PantryChef.Data.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public virtual User DomainUser { get; set; }
    }
}
