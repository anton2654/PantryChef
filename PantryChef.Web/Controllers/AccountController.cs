using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PantryChef.Data.Context;
using PantryChef.Data.Entities;
using PantryChef.Web.Models;

namespace PantryChef.Web.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly PantryChefDbContext _dbContext;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            PantryChefDbContext dbContext,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _dbContext = dbContext;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var identityUser = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email
            };

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var result = await _userManager.CreateAsync(identityUser, model.Password);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }

                    await transaction.RollbackAsync();
                    return View(model);
                }

                await EnsureDomainUserLinkedAsync(identityUser, model.FullName);

                await transaction.CommitAsync();
                _logger.LogInformation("New user registered with email {Email}", model.Email);
                await _signInManager.SignInAsync(identityUser, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to register user with email {Email}", model.Email);
                ModelState.AddModelError(string.Empty, "Unable to complete registration right now. Please try again.");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            var model = new LoginViewModel
            {
                ReturnUrl = returnUrl ?? string.Empty
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user is null || string.IsNullOrWhiteSpace(user.UserName))
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                user.UserName,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: true);

            if (result.Succeeded)
            {
                try
                {
                    await EnsureDomainUserLinkedAsync(user);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to sync domain user for identity account {IdentityUserId}", user.Id);
                    await _signInManager.SignOutAsync();
                    ModelState.AddModelError(string.Empty, "Unable to load your profile. Please try again.");
                    return View(model);
                }

                _logger.LogInformation("User logged in with email {Email}", model.Email);
                return RedirectToLocal(model.ReturnUrl);
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "Account is locked due to multiple failed attempts.");
                return View(model);
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user is not null)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var callbackUrl = Url.Action(
                    action: nameof(ResetPassword),
                    controller: "Account",
                    values: new { token, email = user.Email },
                    protocol: Request.Scheme);

                if (!string.IsNullOrWhiteSpace(callbackUrl))
                {
                    TempData["ResetPasswordLink"] = callbackUrl;
                }
            }

            return RedirectToAction(nameof(ForgotPasswordConfirmation));
        }

        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            ViewData["ResetPasswordLink"] = TempData["ResetPasswordLink"];
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(email))
            {
                return BadRequest("A token and email must be supplied for password reset.");
            }

            return View(new ResetPasswordViewModel
            {
                Token = token,
                Email = email
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user is null)
            {
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
            if (result.Succeeded)
            {
                _logger.LogInformation("User reset password for email {Email}", model.Email);
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        private async Task EnsureDomainUserLinkedAsync(ApplicationUser identityUser, string preferredName = null)
        {
            var existingByIdentityId = await _dbContext.Users
                .FirstOrDefaultAsync(user => user.IdentityUserId == identityUser.Id);

            if (existingByIdentityId is not null)
            {
                return;
            }

            var identityEmail = identityUser.Email ?? identityUser.UserName ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(identityEmail))
            {
                var existingByEmail = await _dbContext.Users
                    .FirstOrDefaultAsync(user => user.Email == identityEmail);

                if (existingByEmail is not null)
                {
                    existingByEmail.IdentityUserId = identityUser.Id;

                    if (string.IsNullOrWhiteSpace(existingByEmail.Name))
                    {
                        existingByEmail.Name = ResolveDisplayName(preferredName, identityEmail);
                    }

                    _dbContext.Users.Update(existingByEmail);
                    await _dbContext.SaveChangesAsync();
                    return;
                }
            }

            var domainUser = new User
            {
                Email = identityEmail,
                Password = "IDENTITY_MANAGED",
                Name = ResolveDisplayName(preferredName, identityEmail),
                CalorieGoals = 2000,
                Allergies = "none",
                IdentityUserId = identityUser.Id
            };

            await _dbContext.Users.AddAsync(domainUser);
            await _dbContext.SaveChangesAsync();
        }

        private static string ResolveDisplayName(string preferredName, string email)
        {
            if (!string.IsNullOrWhiteSpace(preferredName))
            {
                return preferredName.Trim();
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                return "PantryChef User";
            }

            var atSignIndex = email.IndexOf('@');
            return atSignIndex > 0
                ? email[..atSignIndex]
                : email;
        }
    }
}
