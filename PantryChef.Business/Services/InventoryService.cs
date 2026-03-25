using Microsoft.Extensions.Logging;
using PantryChef.Business.Interfaces;
using PantryChef.Business.Models;
using PantryChef.Data.Entities;
using PantryChef.Data.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PantryChef.Business.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly IUserIngredientRepository _inventoryRepo;
        private readonly ILogger<InventoryService> _logger;

        public InventoryService(
            IUserIngredientRepository inventoryRepo,
            ILogger<InventoryService> logger)
        {
            _inventoryRepo = inventoryRepo;
            _logger = logger;
        }

        public async Task<IEnumerable<UserIngredient>> GetUserInventoryAsync(int userId)
        {
            _logger.LogInformation("Отримання списку запасів для користувача {UserId}", userId);
            return await _inventoryRepo.GetUserInventoryAsync(userId);
        }

        public async Task<Result> AddOrUpdateIngredientAsync(int userId, int ingredientId, double quantity)
        {
            _logger.LogInformation("Користувач {UserId} намагається додати інгредієнт {IngredientId} у кількості {Quantity}", userId, ingredientId, quantity);

            if (quantity <= 0)
            {
                return Result.Failure("Кількість має бути більшою за нуль.");
            }

            var existingItem = await _inventoryRepo.GetUserIngredientAsync(userId, ingredientId);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
                _inventoryRepo.Update(existingItem);
            }
            else
            {
                var newItem = new UserIngredient
                {
                    UserId = userId,
                    IngredientId = ingredientId,
                    Quantity = quantity
                };
                await _inventoryRepo.AddAsync(newItem);
            }

            await _inventoryRepo.SaveChangesAsync();
            return Result.Success();
        }
    }
}