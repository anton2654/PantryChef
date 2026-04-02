using Microsoft.Extensions.Logging;
using PantryChef.Business.Interfaces;
using PantryChef.Business.Models;
using PantryChef.Data.Entities;
using PantryChef.Data.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace PantryChef.Business.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly IUserIngredientRepository _inventoryRepo;
        private readonly IIngredientRepository _ingredientRepo;
        private readonly ILogger<InventoryService> _logger;

        public InventoryService(
            IUserIngredientRepository inventoryRepo,
            IIngredientRepository ingredientRepo,
            ILogger<InventoryService> logger)
        {
            _inventoryRepo = inventoryRepo;
            _ingredientRepo = ingredientRepo;
            _logger = logger;
        }

        public async Task<IEnumerable<UserIngredient>> GetUserInventoryAsync(int userId, string category = null, string searchQuery = null)
        {
            _logger.LogInformation("Отримання списку запасів для користувача {UserId} з фільтром: {Category}, пошук: {SearchQuery}", userId, category ?? "Всі", searchQuery ?? "Немає");
            
            var inventory = await _inventoryRepo.GetUserInventoryAsync(userId);

            if (!string.IsNullOrWhiteSpace(category))
            {
                inventory = inventory.Where(i => i.Ingredient.Category.Equals(category, System.StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                inventory = inventory.Where(i => i.Ingredient.Name.Contains(searchQuery, System.StringComparison.OrdinalIgnoreCase));
            }

            return inventory;
        }

        public async Task<IEnumerable<Ingredient>> GetAvailableIngredientsAsync()
        {
            var ingredients = await _ingredientRepo.GetAllAsync();

            return ingredients
                .OrderBy(i => i.Name)
                .ToList();
        }

        public async Task<IEnumerable<string>> GetUserInventoryCategoriesAsync(int userId)
        {
            var inventory = await _inventoryRepo.GetUserInventoryAsync(userId);
            
            return inventory
                .Select(i => i.Ingredient.Category)
                .Distinct()
                .OrderBy(c => c);
        }

        public async Task<Result> AddOrUpdateIngredientAsync(int userId, int ingredientId, double quantity)
        {
            _logger.LogInformation("Користувач {UserId} намагається додати інгредієнт {IngredientId} у кількості {Quantity}", userId, ingredientId, quantity);

            if (quantity <= 0)
            {
                return new Error("Кількість має бути більшою за нуль.");
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
            return new Success();
        }

        public async Task<Result> UpdateIngredientQuantityAsync(int userId, int ingredientId, double newQuantity)
        {
            _logger.LogInformation("Оновлення кількості інгредієнта {IngredientId} для користувача {UserId} на {NewQuantity}", ingredientId, userId, newQuantity);

            if (newQuantity <= 0)
            {
                return Result.Failure("Кількість має бути більшою за нуль. Якщо хочете видалити продукт, скористайтеся відповідною кнопкою.");
            }

            var item = await _inventoryRepo.GetUserIngredientAsync(userId, ingredientId);

            if (item == null)
            {
                return Result.Failure("Інгредієнт не знайдено у холодильнику.");
            }

            item.Quantity = newQuantity;
            _inventoryRepo.Update(item);
            await _inventoryRepo.SaveChangesAsync();

            return Result.Success();
        }

        public async Task<Result> RemoveIngredientAsync(int userId, int ingredientId)
        {
            _logger.LogInformation("Видалення інгредієнта {IngredientId} у користувача {UserId}", ingredientId, userId);

            var item = await _inventoryRepo.GetUserIngredientAsync(userId, ingredientId);

            if (item == null)
            {
                return Result.Failure("Інгредієнт не знайдено у холодильнику.");
            }

            _inventoryRepo.Delete(item); 
            await _inventoryRepo.SaveChangesAsync();

            return Result.Success();
        }
    }
}