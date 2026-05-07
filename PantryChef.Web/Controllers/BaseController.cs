using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using PantryChef.Data.Context;
using System.Linq;
using System.Security.Claims;

namespace PantryChef.Web.Controllers
{
    public abstract class BaseController : Controller
    {
        protected int CurrentUserId
        {
            get
            {
                var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrWhiteSpace(identityUserId))
                {
                    return 1;
                }

                var dbContext = HttpContext?.RequestServices.GetService<PantryChefDbContext>();
                var domainUserId = dbContext?.Users
                    .Where(user => user.IdentityUserId == identityUserId)
                    .Select(user => (int?)user.Id)
                    .FirstOrDefault();

                return domainUserId ?? 1;
            }
        }

        protected void SetSuccessMessage(string message)
        {
            TempData["SuccessMessage"] = message;
        }

        protected void SetErrorMessage(string message)
        {
            TempData["ErrorMessage"] = message;
        }
    }
}
