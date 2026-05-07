using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PantryChef.Data.Context;
using PantryChef.Web.Models;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PantryChef.Web.Controllers
{
    [Authorize]
    [Route("notifications")]
    public class NotificationController : Controller
    {
        private readonly PantryChefDbContext _dbContext;

        public NotificationController(PantryChefDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var userId = await ResolveCurrentUserIdAsync();
            if (userId == null)
            {
                return Unauthorized();
            }

            var notifications = await _dbContext.SystemNotifications
                .AsNoTracking()
                .Where(notification => notification.UserId == userId.Value)
                .OrderByDescending(notification => notification.CreatedAt)
                .Take(10)
                .Select(notification => new NotificationViewModel
                {
                    Id = notification.Id,
                    Type = notification.Type,
                    Title = notification.Title,
                    Message = notification.Message,
                    IsRead = notification.IsRead,
                    CreatedAt = notification.CreatedAt
                })
                .ToListAsync();

            var unreadCount = await _dbContext.SystemNotifications
                .AsNoTracking()
                .CountAsync(notification => notification.UserId == userId.Value && !notification.IsRead);

            return Json(new
            {
                unreadCount,
                notifications
            });
        }

        [HttpPost("read/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = await ResolveCurrentUserIdAsync();
            if (userId == null)
            {
                return Unauthorized();
            }

            var notification = await _dbContext.SystemNotifications
                .FirstOrDefaultAsync(item => item.Id == id && item.UserId == userId.Value);

            if (notification == null)
            {
                return NotFound();
            }

            notification.IsRead = true;
            await _dbContext.SaveChangesAsync();

            return Ok();
        }

        private async Task<int?> ResolveCurrentUserIdAsync()
        {
            var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(identityUserId))
            {
                return null;
            }

            return await _dbContext.Users
                .Where(user => user.IdentityUserId == identityUserId)
                .Select(user => (int?)user.Id)
                .FirstOrDefaultAsync();
        }
    }
}
