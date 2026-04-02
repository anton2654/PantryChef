using Microsoft.AspNetCore.Mvc;

namespace PantryChef.Web.Controllers
{
    public abstract class BaseController : Controller
    {
        protected int CurrentUserId => 1;

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