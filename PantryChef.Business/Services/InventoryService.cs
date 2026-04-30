using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PantryChef.Business.Interfaces;
using PantryChef.Business.Models;
using PantryChef.Data.Entities;
using PantryChef.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace PantryChef.Business.Services
{
    public class InventoryService : IInventoryService
    {
        private const string AvailableIngredientsCacheKey = "inventory:available-ingredients";

        private readonly IUserIngredientRepository _inventoryRepo;
        private readonly IIngredientRepository _ingredientRepo;
        private readonly IRecipeRepository _recipeRepo;
        private readonly IShoppingListRepository _shoppingListRepo;
        private readonly INutritionService _nutritionService;
        private readonly ILogger<InventoryService> _logger;
        private readonly IMemoryCache _cache;
        private readonly PantryChefSettings _settings;

        public InventoryService(
            IUserIngredientRepository inventoryRepo,
            IIngredientRepository ingredientRepo,
            IRecipeRepository recipeRepo,
            IShoppingListRepository shoppingListRepo,
            INutritionService nutritionService,
            ILogger<InventoryService> logger,
            IMemoryCache cache,
            IOptions<PantryChefSettings> options)
        {
            _inventoryRepo = inventoryRepo;
            _ingredientRepo = ingredientRepo;
            _recipeRepo = recipeRepo;
            _shoppingListRepo = shoppingListRepo;
            _nutritionService = nutritionService;
            _logger = logger;
            _cache = cache;
            _settings = options?.Value ?? new PantryChefSettings();
        }

        public async Task<IEnumerable<UserIngredient>> GetUserInventoryAsync(int userId, string category = null, string searchQuery = null)
        {
            _logger.LogInformation("Отримання списку запасів для користувача {UserId} з фільтром: {Category}, пошук: {SearchQuery}", userId, category ?? "Всі", searchQuery ?? "Немає");
            
            var inventory = await _inventoryRepo.GetUserInventoryAsync(userId);
            var minSearchLength = _settings.Inventory.MinSearchLength;

            if (!string.IsNullOrWhiteSpace(category))
            {
                inventory = inventory.Where(i => i.Ingredient.Category.Equals(category, System.StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var normalizedSearchQuery = searchQuery.Trim();

                if (normalizedSearchQuery.Length < minSearchLength)
                {
                    return inventory;
                }

                inventory = inventory.Where(i => i.Ingredient.Name.Contains(normalizedSearchQuery, System.StringComparison.OrdinalIgnoreCase));
            }

            return inventory;
        }

        public async Task<IEnumerable<Ingredient>> GetAvailableIngredientsAsync()
        {
            if (_cache.TryGetValue(AvailableIngredientsCacheKey, out List<Ingredient> cachedIngredients))
            {
                return cachedIngredients;
            }

            var ingredients = (await _ingredientRepo.GetAllAsync())
                .OrderBy(i => i.Name)
                .ToList();

            var ttlMinutes = _settings.Caching.AvailableIngredientsTtlMinutes > 0
                ? _settings.Caching.AvailableIngredientsTtlMinutes
                : 30;

            _cache.Set(
                AvailableIngredientsCacheKey,
                ingredients,
                TimeSpan.FromMinutes(ttlMinutes));

            return ingredients;
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

        public async Task<Result> AddMissingIngredientsToShoppingListAsync(int userId, int recipeId)
        {
            if (userId <= 0)
            {
                _logger.LogWarning("Некоректний ідентифікатор користувача для списку покупок: {UserId}", userId);
                return new Error("Некоректний ідентифікатор користувача.");
            }

            if (recipeId <= 0)
            {
                _logger.LogWarning("Некоректний ідентифікатор страви для списку покупок: {RecipeId}", recipeId);
                return new Error("Некоректний ідентифікатор страви.");
            }

            _logger.LogInformation("Додавання відсутніх інгредієнтів у список покупок для користувача {UserId}, рецепт {RecipeId}", userId, recipeId);

            var recipe = await _recipeRepo.GetRecipeWithIngredientsByIdAsync(recipeId);
            if (recipe == null)
            {
                _logger.LogWarning("Страву {RecipeId} не знайдено для списку покупок", recipeId);
                return Result.Failure($"Страву з ID {recipeId} не знайдено.");
            }

            var inventory = (await _inventoryRepo.GetUserInventoryAsync(userId)).ToList();
            var inventoryMap = inventory.ToDictionary(item => item.IngredientId, item => item.Quantity);

            var deficits = BuildIngredientDeficits(recipe, inventoryMap);
            if (deficits.Count == 0)
            {
                _logger.LogInformation("У користувача {UserId} немає дефіциту для рецепта {RecipeId}", userId, recipeId);
                return Result.Failure("Усі інгредієнти вже є у достатній кількості.");
            }

            foreach (var deficit in deficits)
            {
                if (deficit.MissingQuantity <= 0)
                {
                    continue;
                }

                var existingItem = await _shoppingListRepo.GetItemAsync(userId, deficit.IngredientId);
                if (existingItem != null)
                {
                    existingItem.Quantity += deficit.MissingQuantity;
                    _shoppingListRepo.Update(existingItem);
                }
                else
                {
                    await _shoppingListRepo.AddAsync(new ShoppingListItem
                    {
                        UserId = userId,
                        IngredientId = deficit.IngredientId,
                        Quantity = deficit.MissingQuantity,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            await _shoppingListRepo.SaveChangesAsync();

            _logger.LogInformation("Список покупок користувача {UserId} оновлено для рецепта {RecipeId}", userId, recipeId);
            return Result.Success();
        }

        public async Task<Result> CookRecipeAsync(int userId, int recipeId)
        {
            if (userId <= 0)
            {
                _logger.LogWarning("Некоректний ідентифікатор користувача для приготування: {UserId}", userId);
                return new Error("Некоректний ідентифікатор користувача.");
            }

            if (recipeId <= 0)
            {
                _logger.LogWarning("Некоректний ідентифікатор страви для приготування: {RecipeId}", recipeId);
                return new Error("Некоректний ідентифікатор страви.");
            }

            _logger.LogInformation("Списання інгредієнтів для користувача {UserId}, рецепт {RecipeId}", userId, recipeId);

            var recipe = await _recipeRepo.GetRecipeWithIngredientsByIdAsync(recipeId);
            if (recipe == null)
            {
                _logger.LogWarning("Страву {RecipeId} не знайдено для приготування", recipeId);
                return Result.Failure($"Страву з ID {recipeId} не знайдено.");
            }

            var inventory = (await _inventoryRepo.GetUserInventoryAsync(userId)).ToList();
            var inventoryMap = inventory.ToDictionary(item => item.IngredientId, item => item);

            foreach (var ingredient in recipe.RecipeIngredients)
            {
                if (ingredient.Quantity <= 0)
                {
                    _logger.LogWarning("Некоректна кількість інгредієнта у рецепті {RecipeId}", recipeId);
                    return Result.Failure("Некоректна кількість інгредієнта у рецепті.");
                }

                if (!inventoryMap.TryGetValue(ingredient.IngredientId, out var userItem))
                {
                    _logger.LogWarning("Недостатньо інгредієнта {IngredientId} для рецепта {RecipeId}", ingredient.IngredientId, recipeId);
                    return Result.Failure($"Недостатньо інгредієнта: {ingredient.Ingredient?.Name ?? "Невідомий"}.");
                }

                if (userItem.Quantity < ingredient.Quantity)
                {
                    _logger.LogWarning("Недостатньо інгредієнта {IngredientId} для рецепта {RecipeId}", ingredient.IngredientId, recipeId);
                    return Result.Failure($"Недостатньо інгредієнта: {ingredient.Ingredient?.Name ?? "Невідомий"}.");
                }
            }

            foreach (var ingredient in recipe.RecipeIngredients)
            {
                var userItem = inventoryMap[ingredient.IngredientId];
                userItem.Quantity -= ingredient.Quantity;

                if (userItem.Quantity <= 0)
                {
                    _inventoryRepo.Delete(userItem);
                }
                else
                {
                    _inventoryRepo.Update(userItem);
                }
            }

            await _inventoryRepo.SaveChangesAsync();

            var nutrition = _nutritionService.CalculateNutrition(recipe);
            var nutritionResult = await _nutritionService.AddConsumedNutritionAsync(
                userId,
                nutrition.Calories,
                nutrition.Proteins,
                nutrition.Fats,
                nutrition.Carbohydrates);

            if (!nutritionResult.IsSuccess)
            {
                _logger.LogWarning("Не вдалося зберегти спожиту поживну цінність для користувача {UserId}: {Error}", userId, nutritionResult.ErrorMessage);
                return Result.Failure(nutritionResult.ErrorMessage);
            }

            _logger.LogInformation("Інгредієнти успішно списано для користувача {UserId}, рецепт {RecipeId}", userId, recipeId);
            return Result.Success();
        }

        public async Task<Result<IReadOnlyList<ShoppingListItem>>> GetShoppingListAsync(int userId)
        {
            if (userId <= 0)
            {
                _logger.LogWarning("Некоректний ідентифікатор користувача для перегляду списку покупок: {UserId}", userId);
                return new Error("Некоректний ідентифікатор користувача.");
            }

            var items = (await _shoppingListRepo.GetByUserAsync(userId)).ToList();
            return items;
        }

        public async Task<Result> RemoveShoppingListItemAsync(int userId, int ingredientId)
        {
            if (userId <= 0)
            {
                _logger.LogWarning("Некоректний ідентифікатор користувача для видалення з списку покупок: {UserId}", userId);
                return new Error("Некоректний ідентифікатор користувача.");
            }

            var item = await _shoppingListRepo.GetItemAsync(userId, ingredientId);
            if (item == null)
            {
                return Result.Failure("Позицію не знайдено у списку покупок.");
            }

            _shoppingListRepo.Delete(item);
            await _shoppingListRepo.SaveChangesAsync();

            _logger.LogInformation("Позицію {IngredientId} видалено зі списку покупок для користувача {UserId}", ingredientId, userId);
            return Result.Success();
        }

        private static List<IngredientDeficit> BuildIngredientDeficits(
            Recipe recipe,
            IReadOnlyDictionary<int, double> inventoryMap)
        {
            var deficits = new List<IngredientDeficit>();

            if (recipe?.RecipeIngredients == null)
            {
                return deficits;
            }

            foreach (var ingredient in recipe.RecipeIngredients)
            {
                var required = ingredient.Quantity;
                if (required <= 0)
                {
                    continue;
                }

                var available = inventoryMap.TryGetValue(ingredient.IngredientId, out var availableQuantity)
                    ? availableQuantity
                    : 0;

                var missing = required - available;
                if (missing > 0)
                {
                    deficits.Add(new IngredientDeficit
                    {
                        IngredientId = ingredient.IngredientId,
                        IngredientName = ingredient.Ingredient?.Name ?? string.Empty,
                        RequiredQuantity = required,
                        AvailableQuantity = available,
                        MissingQuantity = missing
                    });
                }
            }

            return deficits;
        }
    }
}