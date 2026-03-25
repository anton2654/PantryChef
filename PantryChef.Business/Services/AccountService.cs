using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PantryChef.Business.Interfaces;
using PantryChef.Data.Context;
using PantryChef.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PantryChef.Business.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly PantryChefDbContext _dbContext;
        private readonly ILogger<AccountService> _logger;

        public AccountService(
            UserManager<ApplicationUser> userManager,
            PantryChefDbContext dbContext,
            ILogger<AccountService> logger)
        {
            _userManager = userManager;
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<(bool Succeeded, IEnumerable<string> Errors, ApplicationUser User)> RegisterUserAsync(string email, string password, string fullName)
        {
            var identityUser = new ApplicationUser
            {
                UserName = email,
                Email = email
            };

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var result = await _userManager.CreateAsync(identityUser, password);
                if (!result.Succeeded)
                {
                    await transaction.RollbackAsync();
                    return (false, result.Errors.Select(e => e.Description), null);
                }

                await EnsureDomainUserLinkedAsync(identityUser, fullName);

                await transaction.CommitAsync();
                return (true, Enumerable.Empty<string>(), identityUser);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to register user with email {Email}", email);
                throw;
            }
        }

        public async Task EnsureDomainUserLinkedAsync(ApplicationUser identityUser, string preferredName = null)
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