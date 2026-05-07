using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PantryChef.Business.Models;
using PantryChef.Data.Context;
using PantryChef.Data.Entities;
using PantryChef.Web.Hubs;
using PantryChef.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PantryChef.Web.Services
{
    public class NotificationBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<NotificationBackgroundService> _logger;
        private readonly PantryChefSettings _settings;

        public NotificationBackgroundService(
            IServiceProvider services,
            IHubContext<NotificationHub> hubContext,
            ILogger<NotificationBackgroundService> logger,
            IOptions<PantryChefSettings> options)
        {
            _services = services;
            _hubContext = hubContext;
            _logger = logger;
            _settings = options?.Value ?? new PantryChefSettings();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Notification background service started.");

            try
            {
                await CheckAndNotifyAsync(stoppingToken);

                var intervalSeconds = Math.Max(_settings.Notifications.CheckIntervalSeconds, 10);
                using var timer = new PeriodicTimer(TimeSpan.FromSeconds(intervalSeconds));

                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    await CheckAndNotifyAsync(stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Notification background service is stopping.");
            }
        }

        private async Task CheckAndNotifyAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _services.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<PantryChefDbContext>();

                var notifications = new List<PendingNotification>();
                notifications.AddRange(await BuildLowStockNotificationsAsync(dbContext, cancellationToken));
                notifications.AddRange(await BuildShoppingListNotificationsAsync(dbContext, cancellationToken));
                notifications.AddRange(await BuildCalorieGoalNotificationsAsync(dbContext, cancellationToken));

                if (notifications.Count == 0)
                {
                    return;
                }

                await dbContext.SystemNotifications.AddRangeAsync(
                    notifications.Select(notification => notification.Notification),
                    cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);

                foreach (var pending in notifications)
                {
                    if (string.IsNullOrWhiteSpace(pending.IdentityUserId))
                    {
                        continue;
                    }

                    await _hubContext.Clients
                        .User(pending.IdentityUserId)
                        .SendAsync("ReceiveNotification", Map(pending.Notification), cancellationToken);

                    pending.Notification.DeliveredAt = DateTime.UtcNow;
                }

                await dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check notification events.");
            }
        }

        private async Task<List<PendingNotification>> BuildLowStockNotificationsAsync(
            PantryChefDbContext dbContext,
            CancellationToken cancellationToken)
        {
            var threshold = _settings.Notifications.LowStockThreshold;

            var lowStockItems = await dbContext.UserIngredients
                .AsNoTracking()
                .Include(item => item.Ingredient)
                .Include(item => item.User)
                .Where(item => item.Quantity <= threshold && item.User.IdentityUserId != null)
                .ToListAsync(cancellationToken);

            var notifications = new List<PendingNotification>();

            foreach (var item in lowStockItems)
            {
                var eventKey = $"low-stock:{item.UserId}:{item.IngredientId}:{Math.Round(item.Quantity, 1)}";
                if (await HasNotificationAsync(dbContext, eventKey, cancellationToken))
                {
                    continue;
                }

                notifications.Add(new PendingNotification(
                    new SystemNotification
                    {
                        UserId = item.UserId,
                        EventKey = eventKey,
                        Type = "low-stock",
                        Title = "Закінчується продукт",
                        Message = $"{item.Ingredient.Name}: залишилось {item.Quantity:0.##} г/шт."
                    },
                    item.User.IdentityUserId));
            }

            return notifications;
        }

        private async Task<List<PendingNotification>> BuildShoppingListNotificationsAsync(
            PantryChefDbContext dbContext,
            CancellationToken cancellationToken)
        {
            var since = DateTime.UtcNow.AddDays(-Math.Max(_settings.Notifications.ShoppingListLookbackDays, 1));

            var items = await dbContext.ShoppingListItems
                .AsNoTracking()
                .Include(item => item.Ingredient)
                .Include(item => item.User)
                .Where(item => item.CreatedAt >= since && item.User.IdentityUserId != null)
                .ToListAsync(cancellationToken);

            var notifications = new List<PendingNotification>();

            foreach (var item in items)
            {
                var eventKey = $"shopping-list:{item.Id}";
                if (await HasNotificationAsync(dbContext, eventKey, cancellationToken))
                {
                    continue;
                }

                notifications.Add(new PendingNotification(
                    new SystemNotification
                    {
                        UserId = item.UserId,
                        EventKey = eventKey,
                        Type = "shopping-list",
                        Title = "Список покупок оновлено",
                        Message = $"{item.Ingredient.Name} додано до покупок: {item.Quantity:0.##} г/шт."
                    },
                    item.User.IdentityUserId));
            }

            return notifications;
        }

        private async Task<List<PendingNotification>> BuildCalorieGoalNotificationsAsync(
            PantryChefDbContext dbContext,
            CancellationToken cancellationToken)
        {
            var today = DateTime.UtcNow.Date;

            var exceededLogs = await dbContext.UserNutritionLogs
                .AsNoTracking()
                .Include(log => log.User)
                .Where(log => log.LogDate == today
                    && log.User.IdentityUserId != null
                    && log.User.CalorieGoals > 0
                    && log.Calories > log.User.CalorieGoals)
                .ToListAsync(cancellationToken);

            var notifications = new List<PendingNotification>();

            foreach (var log in exceededLogs)
            {
                var eventKey = $"calorie-goal:{log.UserId}:{log.LogDate:yyyyMMdd}";
                if (await HasNotificationAsync(dbContext, eventKey, cancellationToken))
                {
                    continue;
                }

                notifications.Add(new PendingNotification(
                    new SystemNotification
                    {
                        UserId = log.UserId,
                        EventKey = eventKey,
                        Type = "calorie-goal",
                        Title = "Перевищено денну норму",
                        Message = $"Сьогодні спожито {log.Calories:0} ккал з цілі {log.User.CalorieGoals} ккал."
                    },
                    log.User.IdentityUserId));
            }

            return notifications;
        }

        private static async Task<bool> HasNotificationAsync(
            PantryChefDbContext dbContext,
            string eventKey,
            CancellationToken cancellationToken)
        {
            return await dbContext.SystemNotifications
                .AnyAsync(notification => notification.EventKey == eventKey, cancellationToken);
        }

        private static NotificationViewModel Map(SystemNotification notification)
        {
            return new NotificationViewModel
            {
                Id = notification.Id,
                Type = notification.Type,
                Title = notification.Title,
                Message = notification.Message,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt
            };
        }

        private sealed record PendingNotification(SystemNotification Notification, string IdentityUserId);
    }
}
