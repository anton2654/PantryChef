using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PantryChef.Business.Interfaces;
using PantryChef.Business.Models;
using PantryChef.Data.Entities;
using PantryChef.Data.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PantryChef.Business.Services
{
    public class RecipeService : IRecipeService
    {
        private const string AvailableCategoriesCacheKey = "recipe:available-categories";

        private static readonly HttpClient _photoResolverClient = CreatePhotoResolverClient();
        private static readonly ConcurrentDictionary<string, byte> _failedPhotoResolveCache = new(StringComparer.OrdinalIgnoreCase);

        private static readonly Regex OgImageRegex = new(
            "<meta[^>]+(?:property|name)=[\"'](?:og:image|twitter:image)[\"'][^>]+content=[\"'](?<url>[^\"']+)[\"'][^>]*>|<meta[^>]+content=[\"'](?<url>[^\"']+)[\"'][^>]+(?:property|name)=[\"'](?:og:image|twitter:image)[\"'][^>]*>",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex AbsoluteImageRegex = new(
            "(?<url>https?://[^\"'\\s>]+\\.(?:jpg|jpeg|png|webp|gif|bmp|svg|avif))(?:\\?[^\"'\\s>]*)?",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex RelativeImageRegex = new(
            "(?:src|href)=[\"'](?<url>/[^\"']+\\.(?:jpg|jpeg|png|webp|gif|bmp|svg|avif)(?:\\?[^\"']*)?)[\"']",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private readonly IRecipeRepository _recipeRepo;
        private readonly IUserIngredientRepository _userIngredientRepo;
        private readonly IUserRecipeRepository _userRecipeRepo;
        private readonly ILogger<RecipeService> _logger;
        private readonly IMemoryCache _cache;
        private readonly PantryChefSettings _settings;
        private readonly IInventoryService _inventoryService;
        private readonly INutritionService _nutritionService;

        public RecipeService(
            IRecipeRepository recipeRepo,
            IUserIngredientRepository userIngredientRepo,
            IUserRecipeRepository userRecipeRepo,
            ILogger<RecipeService> logger,
            IMemoryCache cache,
            IOptions<PantryChefSettings> options,
            IInventoryService inventoryService,
            INutritionService nutritionService)
        {
            _recipeRepo = recipeRepo;
            _userIngredientRepo = userIngredientRepo;
            _userRecipeRepo = userRecipeRepo;
            _logger = logger;
            _cache = cache;
            _settings = options?.Value ?? new PantryChefSettings();
            _inventoryService = inventoryService;
            _nutritionService = nutritionService;
        }

        public async Task<IEnumerable<Recipe>> GetAllRecipesWithIngredientsAsync()
        {
            _logger.LogInformation("Отримання всіх рецептів з інгредієнтами.");
            var recipes = (await _recipeRepo.GetAllRecipesWithIngredientsAsync()).ToList();
            await ResolveAndPersistPhotoLinksAsync(recipes);
            return recipes;
        }

        public async Task<IEnumerable<Recipe>> GetRecipesByCategoryAsync(string category)
        {
            _logger.LogInformation("Отримання рецептів за категорією: {Category}", category);
            var recipes = (await _recipeRepo.GetRecipesByCategoryAsync(category)).ToList();
            await ResolveAndPersistPhotoLinksAsync(recipes);
            return recipes;
        }

        public async Task<IEnumerable<string>> GetAvailableCategoriesAsync()
        {
            _logger.LogInformation("Отримання доступних категорій страв.");

            if (_cache.TryGetValue(AvailableCategoriesCacheKey, out List<string> cachedCategories))
            {
                return cachedCategories;
            }

            var categories = (await _recipeRepo.GetAvailableCategoriesAsync()).ToList();
            var ttlMinutes = _settings.Caching.AvailableRecipeCategoriesTtlMinutes > 0
                ? _settings.Caching.AvailableRecipeCategoriesTtlMinutes
                : 30;

            _cache.Set(
                AvailableCategoriesCacheKey,
                categories,
                TimeSpan.FromMinutes(ttlMinutes));

            return categories;
        }

        public async Task<Recipe> GetRecipeWithIngredientsByIdAsync(int id)
        {
            _logger.LogInformation("Отримання рецепта з інгредієнтами за ID: {Id}", id);
            var recipe = await _recipeRepo.GetRecipeWithIngredientsByIdAsync(id);

            if (recipe != null)
            {
                await ResolveAndPersistPhotoLinksAsync([recipe]);
            }

            return recipe;
        }

        public async Task<Result<int>> AddRecipeAsync(RecipeCreateModel model)
        {
            if (model == null)
            {
                const string error = "Дані для створення страви не передані.";
                _logger.LogError(error);
                return Result<int>.Failure(error);
            }

            var validationError = ValidateRecipeData(
                model.Name,
                model.Description,
                model.Category,
                model.Photo,
                model.Calories,
                model.Proteins,
                model.Fats,
                model.Carbohydrates);

            if (!string.IsNullOrWhiteSpace(validationError))
            {
                _logger.LogError("Не вдалося створити страву: {ValidationError}", validationError);
                return Result<int>.Failure(validationError);
            }

            try
            {
                var resolvedPhoto = await ResolvePhotoUrlAsync(model.Photo, model.Name);

                var recipe = new Recipe
                {
                    Name = model.Name.Trim(),
                    Description = model.Description.Trim(),
                    Category = model.Category.Trim(),
                    Photo = resolvedPhoto,
                    Calories = model.Calories,
                    Proteins = model.Proteins,
                    Fats = model.Fats,
                    Carbohydrates = model.Carbohydrates
                };

                await _recipeRepo.AddRecipeAsync(recipe);
                await _recipeRepo.SaveChangesAsync();
                InvalidateAvailableCategoriesCache();

                _logger.LogInformation("Користувач додав страву {RecipeName}", recipe.Name);
                return Result<int>.Success(recipe.Id);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Помилка під час створення страви {RecipeName}", model.Name);
                return Result<int>.Failure("Не вдалося створити страву. Спробуйте пізніше.");
            }
        }

        public async Task<Result> EditRecipeAsync(RecipeEditModel model)
        {
            if (model == null)
            {
                const string error = "Дані для редагування страви не передані.";
                _logger.LogError(error);
                return Result.Failure(error);
            }

            if (model.Id <= 0)
            {
                const string error = "Некоректний ідентифікатор страви.";
                _logger.LogError(error);
                return Result.Failure(error);
            }

            var validationError = ValidateRecipeData(
                model.Name,
                model.Description,
                model.Category,
                model.Photo,
                model.Calories,
                model.Proteins,
                model.Fats,
                model.Carbohydrates);

            if (!string.IsNullOrWhiteSpace(validationError))
            {
                _logger.LogError("Не вдалося оновити страву {RecipeId}: {ValidationError}", model.Id, validationError);
                return Result.Failure(validationError);
            }

            try
            {
                var recipe = await _recipeRepo.GetRecipeByIdAsync(model.Id);

                if (recipe == null)
                {
                    var error = $"Страву з ID {model.Id} не знайдено.";
                    _logger.LogError(error);
                    return Result.Failure(error);
                }

                recipe.Name = model.Name.Trim();
                recipe.Description = model.Description.Trim();
                recipe.Category = model.Category.Trim();
                recipe.Photo = await ResolvePhotoUrlAsync(model.Photo, model.Name);
                recipe.Calories = model.Calories;
                recipe.Proteins = model.Proteins;
                recipe.Fats = model.Fats;
                recipe.Carbohydrates = model.Carbohydrates;

                _recipeRepo.UpdateRecipe(recipe);
                await _recipeRepo.SaveChangesAsync();
                InvalidateAvailableCategoriesCache();

                _logger.LogInformation("Користувач оновив страву {RecipeName} (ID: {RecipeId})", recipe.Name, recipe.Id);
                return Result.Success();
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Помилка під час редагування страви {RecipeId}", model.Id);
                return Result.Failure("Не вдалося оновити страву. Спробуйте пізніше.");
            }
        }

        public async Task<Result> RemoveRecipeForUserAsync(int userId, int recipeId)
        {
            if (userId <= 0)
            {
                const string error = "Некоректний ідентифікатор користувача.";
                _logger.LogError(error);
                return Result.Failure(error);
            }

            if (recipeId <= 0)
            {
                const string error = "Некоректний ідентифікатор страви.";
                _logger.LogError(error);
                return Result.Failure(error);
            }

            var recipe = await _recipeRepo.GetRecipeByIdAsync(recipeId);

            if (recipe == null)
            {
                var error = $"Страву з ID {recipeId} не знайдено.";
                _logger.LogError(error);
                return Result.Failure(error);
            }

            var link = await _userRecipeRepo.GetAsync(userId, recipeId);

            if (link == null)
            {
                await _userRecipeRepo.AddAsync(new UserRecipe
                {
                    UserId = userId,
                    RecipeId = recipeId,
                    IsSaved = false,
                    UpdatedAt = DateTime.UtcNow
                });
            }
            else if (link.IsSaved)
            {
                link.IsSaved = false;
                link.UpdatedAt = DateTime.UtcNow;
                _userRecipeRepo.Update(link);
            }

            await _userRecipeRepo.SaveChangesAsync();

            _logger.LogInformation("Користувач {UserId} прибрав страву {RecipeId} зі списку", userId, recipeId);
            return Result.Success();
        }

        public async Task<Result> SaveRecipeForUserAsync(int userId, int recipeId)
        {
            if (userId <= 0)
            {
                const string error = "Некоректний ідентифікатор користувача.";
                _logger.LogError(error);
                return Result.Failure(error);
            }

            if (recipeId <= 0)
            {
                const string error = "Некоректний ідентифікатор страви.";
                _logger.LogError(error);
                return Result.Failure(error);
            }

            var recipe = await _recipeRepo.GetRecipeByIdAsync(recipeId);

            if (recipe == null)
            {
                var error = $"Страву з ID {recipeId} не знайдено.";
                _logger.LogError(error);
                return Result.Failure(error);
            }

            var link = await _userRecipeRepo.GetAsync(userId, recipeId);

            if (link == null)
            {
                await _userRecipeRepo.AddAsync(new UserRecipe
                {
                    UserId = userId,
                    RecipeId = recipeId,
                    IsSaved = true,
                    UpdatedAt = DateTime.UtcNow
                });
            }
            else if (!link.IsSaved)
            {
                link.IsSaved = true;
                link.UpdatedAt = DateTime.UtcNow;
                _userRecipeRepo.Update(link);
            }

            await _userRecipeRepo.SaveChangesAsync();

            _logger.LogInformation("Користувач {UserId} додав/позначив страву {RecipeId} як збережену", userId, recipeId);
            return Result.Success();
        }

        public async Task<Result<RecipeEditModel>> GetRecipeForEditAsync(int recipeId)
        {
            if (recipeId <= 0)
            {
                const string error = "Некоректний ідентифікатор страви.";
                _logger.LogError(error);
                return Result<RecipeEditModel>.Failure(error);
            }

            try
            {
                var recipe = await _recipeRepo.GetRecipeByIdAsync(recipeId);

                if (recipe == null)
                {
                    var error = $"Страву з ID {recipeId} не знайдено.";
                    _logger.LogError(error);
                    return Result<RecipeEditModel>.Failure(error);
                }

                return Result<RecipeEditModel>.Success(new RecipeEditModel
                {
                    Id = recipe.Id,
                    Name = recipe.Name,
                    Description = recipe.Description,
                    Category = recipe.Category,
                    Photo = recipe.Photo,
                    Calories = recipe.Calories,
                    Proteins = recipe.Proteins,
                    Fats = recipe.Fats,
                    Carbohydrates = recipe.Carbohydrates
                });
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Помилка під час отримання страви {RecipeId} для редагування", recipeId);
                return Result<RecipeEditModel>.Failure("Не вдалося завантажити страву для редагування.");
            }
        }

        public async Task<Result<RecipeDeleteModel>> GetRecipeForDeleteAsync(int recipeId)
        {
            if (recipeId <= 0)
            {
                const string error = "Некоректний ідентифікатор страви.";
                _logger.LogError(error);
                return Result<RecipeDeleteModel>.Failure(error);
            }

            try
            {
                var recipe = await _recipeRepo.GetRecipeByIdAsync(recipeId);

                if (recipe == null)
                {
                    var error = $"Страву з ID {recipeId} не знайдено.";
                    _logger.LogError(error);
                    return Result<RecipeDeleteModel>.Failure(error);
                }

                return Result<RecipeDeleteModel>.Success(new RecipeDeleteModel
                {
                    Id = recipe.Id,
                    Name = recipe.Name,
                    Category = recipe.Category
                });
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Помилка під час отримання страви {RecipeId} для видалення", recipeId);
                return Result<RecipeDeleteModel>.Failure("Не вдалося завантажити страву для видалення.");
            }
        }

        public async Task<Result<IReadOnlyList<Recipe>>> GetUserRecipesAsync(int userId, string category = null)
        {
            if (userId <= 0)
            {
                const string error = "Некоректний ідентифікатор користувача.";
                _logger.LogError(error);
                return Result<IReadOnlyList<Recipe>>.Failure(error);
            }

            var hiddenRecipeIds = await _userRecipeRepo.GetHiddenRecipeIdsAsync(userId);

            var recipes = string.IsNullOrWhiteSpace(category)
                ? await _recipeRepo.GetAllRecipesWithIngredientsAsync()
                : await _recipeRepo.GetRecipesByCategoryAsync(category);

            var filtered = (recipes ?? Enumerable.Empty<Recipe>())
                .Where(recipe => !hiddenRecipeIds.Contains(recipe.Id))
                .ToList();

            await ResolveAndPersistPhotoLinksAsync(filtered);
            return filtered;
        }

        public async Task<Result<IReadOnlyList<RecipeMatchResult>>> GetFullMatchRecipesAsync(int userId)
        {
            if (userId <= 0)
            {
                _logger.LogWarning("Некоректний ідентифікатор користувача для генерації повного меню: {UserId}", userId);
                return new Error("Некоректний ідентифікатор користувача.");
            }

            _logger.LogInformation("Генерація повного меню для користувача {UserId}", userId);

            var recipes = (await _recipeRepo.GetAllRecipesWithIngredientsAsync()).ToList();
            var inventory = (await _userIngredientRepo.GetUserInventoryAsync(userId)).ToList();
            var inventoryMap = inventory.ToDictionary(item => item.IngredientId, item => item.Quantity);

            var matches = new List<RecipeMatchResult>();

            foreach (var recipe in recipes)
            {
                var deficits = BuildIngredientDeficits(recipe, inventoryMap);
                if (deficits.Count == 0)
                {
                    matches.Add(new RecipeMatchResult
                    {
                        Recipe = recipe,
                        MissingIngredients = deficits
                    });
                }
            }

            _logger.LogInformation("Знайдено {MatchCount} повних збігів для користувача {UserId}", matches.Count, userId);
            return matches;
        }

        public async Task<Result<IReadOnlyList<RecipeMatchResult>>> GetPartialMatchRecipesAsync(int userId)
        {
            if (userId <= 0)
            {
                _logger.LogWarning("Некоректний ідентифікатор користувача для генерації часткового меню: {UserId}", userId);
                return new Error("Некоректний ідентифікатор користувача.");
            }

            _logger.LogInformation("Генерація часткового меню для користувача {UserId}", userId);

            var recipes = (await _recipeRepo.GetAllRecipesWithIngredientsAsync()).ToList();
            var inventory = (await _userIngredientRepo.GetUserInventoryAsync(userId)).ToList();
            var inventoryMap = inventory.ToDictionary(item => item.IngredientId, item => item.Quantity);

            var matches = new List<RecipeMatchResult>();

            foreach (var recipe in recipes)
            {
                var deficits = BuildIngredientDeficits(recipe, inventoryMap);
                if (deficits.Count > 0)
                {
                    matches.Add(new RecipeMatchResult
                    {
                        Recipe = recipe,
                        MissingIngredients = deficits
                    });
                }
            }

            _logger.LogInformation("Знайдено {MatchCount} часткових збігів для користувача {UserId}", matches.Count, userId);
            return matches;
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

        private static string ValidateRecipeData(
            string name,
            string description,
            string category,
            string photo,
            double calories,
            double proteins,
            double fats,
            double carbohydrates)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return "Назва страви є обов'язковою.";
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                return "Опис страви є обов'язковим.";
            }

            if (string.IsNullOrWhiteSpace(category))
            {
                return "Категорія страви є обов'язковою.";
            }

            if (string.IsNullOrWhiteSpace(photo))
            {
                return "Посилання на фото є обов'язковим.";
            }

            if (calories < 0 || proteins < 0 || fats < 0 || carbohydrates < 0)
            {
                return "Поживні значення не можуть бути від'ємними.";
            }

            return string.Empty;
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

            var recipe = await _recipeRepo.GetRecipeWithIngredientsByIdAsync(recipeId);
            if (recipe == null)
            {
                _logger.LogWarning("Страву {RecipeId} не знайдено для приготування", recipeId);
                return Result.Failure($"Страву з ID {recipeId} не знайдено.");
            }

            // Ensure ingredients are consumed via InventoryService
            var consumeResult = await _inventoryService.CookRecipeAsync(userId, recipeId);
            if (!consumeResult.IsSuccess)
            {
                _logger.LogWarning("Не вдалося списати інгредієнти для користувача {UserId}, рецепт {RecipeId}: {Error}", userId, recipeId, consumeResult.ErrorMessage);
                return Result.Failure(consumeResult.ErrorMessage);
            }

            // Keep the recipe payload normalized after cooking.
            recipe.WeightGrams = recipe.RecipeIngredients?.Sum(ri => ri.Quantity) ?? 0;

            try
            {
                _recipeRepo.Update(recipe);
                await _recipeRepo.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Не вдалося зберегти інформацію про приготування для рецепта {RecipeId}", recipeId);
                return Result.Failure("Не вдалося оновити інформацію про страву після приготування.");
            }

            // Record nutrition consumption
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

            _logger.LogInformation("Рецепт {RecipeId} приготовано для користувача {UserId}", recipeId, userId);
            return Result.Success();
        }

        private async Task<string> ResolvePhotoUrlAsync(string rawPhotoUrl, string recipeName)
        {
            var normalized = rawPhotoUrl?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(normalized) || LooksLikeImageUrl(normalized))
            {
                return normalized;
            }

            if (_failedPhotoResolveCache.ContainsKey(normalized))
            {
                return normalized;
            }

            if (!Uri.TryCreate(normalized, UriKind.Absolute, out var pageUri) ||
                (pageUri.Scheme != Uri.UriSchemeHttp && pageUri.Scheme != Uri.UriSchemeHttps))
            {
                return normalized;
            }

            try
            {
                var html = await _photoResolverClient.GetStringAsync(pageUri);
                var extractedImageUrl = ExtractImageUrlFromHtml(html, pageUri);

                if (!string.IsNullOrWhiteSpace(extractedImageUrl))
                {
                    _failedPhotoResolveCache.TryRemove(normalized, out _);
                    _logger.LogInformation("Знайдено пряме зображення для страви {RecipeName} із URL сторінки.", recipeName);
                    return extractedImageUrl;
                }

                _failedPhotoResolveCache.TryAdd(normalized, 0);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Не вдалося автоматично отримати фото зі сторінки для страви {RecipeName}", recipeName);
                _failedPhotoResolveCache.TryAdd(normalized, 0);
            }

            return normalized;
        }

        private async Task ResolveAndPersistPhotoLinksAsync(IEnumerable<Recipe> recipes)
        {
            var hasChanges = false;

            foreach (var recipe in recipes)
            {
                if (recipe == null)
                {
                    continue;
                }

                var resolvedPhoto = await ResolvePhotoUrlAsync(recipe.Photo, recipe.Name);

                if (string.Equals(recipe.Photo, resolvedPhoto, StringComparison.Ordinal))
                {
                    continue;
                }

                recipe.Photo = resolvedPhoto;
                _recipeRepo.UpdateRecipe(recipe);
                hasChanges = true;
            }

            if (hasChanges)
            {
                await _recipeRepo.SaveChangesAsync();
            }
        }

        private static string ExtractImageUrlFromHtml(string html, Uri pageUri)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                return string.Empty;
            }

            var ogImageMatch = OgImageRegex.Match(html);
            if (ogImageMatch.Success)
            {
                var resolvedOgImageUrl = ResolveToAbsoluteUrl(ogImageMatch.Groups["url"].Value, pageUri);
                if (LooksLikeImageUrl(resolvedOgImageUrl))
                {
                    return resolvedOgImageUrl;
                }
            }

            var absoluteImageMatch = AbsoluteImageRegex.Match(html);
            if (absoluteImageMatch.Success)
            {
                return ResolveToAbsoluteUrl(absoluteImageMatch.Groups["url"].Value, pageUri);
            }

            var relativeImageMatch = RelativeImageRegex.Match(html);
            if (relativeImageMatch.Success)
            {
                return ResolveToAbsoluteUrl(relativeImageMatch.Groups["url"].Value, pageUri);
            }

            return string.Empty;
        }

        private static string ResolveToAbsoluteUrl(string urlCandidate, Uri pageUri)
        {
            if (string.IsNullOrWhiteSpace(urlCandidate))
            {
                return string.Empty;
            }

            var decodedUrl = WebUtility.HtmlDecode(urlCandidate).Trim();

            if (Uri.TryCreate(decodedUrl, UriKind.Absolute, out var absoluteUri))
            {
                return absoluteUri.ToString();
            }

            if (Uri.TryCreate(pageUri, decodedUrl, out var relativeUri))
            {
                return relativeUri.ToString();
            }

            return string.Empty;
        }

        private static bool LooksLikeImageUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return false;
            }

            var normalizedUrl = url.ToLowerInvariant();
            return normalizedUrl.Contains(".jpg")
                   || normalizedUrl.Contains(".jpeg")
                   || normalizedUrl.Contains(".png")
                   || normalizedUrl.Contains(".webp")
                   || normalizedUrl.Contains(".gif")
                   || normalizedUrl.Contains(".bmp")
                   || normalizedUrl.Contains(".svg")
                   || normalizedUrl.Contains(".avif");
        }

        private void InvalidateAvailableCategoriesCache()
        {
            _cache.Remove(AvailableCategoriesCacheKey);
        }

        private static HttpClient CreatePhotoResolverClient()
        {
            var client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(8)
            };

            client.DefaultRequestHeaders.UserAgent.ParseAdd("PantryChefBot/1.0");
            return client;
        }
    }
}