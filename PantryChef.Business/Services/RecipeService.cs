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
        private readonly ILogger<RecipeService> _logger;
        private readonly PantryChefSettings _settings;

        public RecipeService(
            IRecipeRepository recipeRepo,
            ILogger<RecipeService> logger,
            IOptions<PantryChefSettings> options)
        {
            _recipeRepo = recipeRepo;
            _logger = logger;
            _settings = options?.Value ?? new PantryChefSettings();
        }

        public async Task<IEnumerable<Recipe>> GetAllRecipesWithIngredientsAsync()
        {
            _logger.LogInformation("Отримання всіх рецептів з інгредієнтами.");
            var recipes = (await _recipeRepo.GetAllRecipesWithIngredientsAsync()).ToList();
            await ResolveAndPersistPhotoLinksAsync(recipes);
            return ApplyDefaultPageSize(recipes);
        }

        public async Task<IEnumerable<Recipe>> GetRecipesByCategoryAsync(string category)
        {
            _logger.LogInformation("Отримання рецептів за категорією: {Category}", category);
            var recipes = (await _recipeRepo.GetRecipesByCategoryAsync(category)).ToList();
            await ResolveAndPersistPhotoLinksAsync(recipes);
            return ApplyDefaultPageSize(recipes);
        }

        public async Task<IEnumerable<string>> GetAvailableCategoriesAsync()
        {
            _logger.LogInformation("Отримання доступних категорій страв.");
            return await _recipeRepo.GetAvailableCategoriesAsync();
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

                _logger.LogInformation("Користувач оновив страву {RecipeName} (ID: {RecipeId})", recipe.Name, recipe.Id);
                return Result.Success();
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Помилка під час редагування страви {RecipeId}", model.Id);
                return Result.Failure("Не вдалося оновити страву. Спробуйте пізніше.");
            }
        }

        public async Task<Result> DeleteRecipeAsync(int recipeId)
        {
            if (recipeId <= 0)
            {
                const string error = "Некоректний ідентифікатор страви.";
                _logger.LogError(error);
                return Result.Failure(error);
            }

            try
            {
                var recipe = await _recipeRepo.GetRecipeByIdAsync(recipeId);

                if (recipe == null)
                {
                    var error = $"Страву з ID {recipeId} не знайдено.";
                    _logger.LogError(error);
                    return Result.Failure(error);
                }

                _recipeRepo.DeleteRecipe(recipe);
                await _recipeRepo.SaveChangesAsync();

                _logger.LogInformation("Користувач видалив страву {RecipeName} (ID: {RecipeId})", recipe.Name, recipeId);
                return Result.Success();
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Помилка під час видалення страви {RecipeId}", recipeId);
                return Result.Failure("Не вдалося видалити страву. Спробуйте пізніше.");
            }
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

        private IEnumerable<Recipe> ApplyDefaultPageSize(IEnumerable<Recipe> recipes)
        {
            var pageSize = _settings.Pagination.DefaultPageSize;
            return pageSize > 0 ? recipes.Take(pageSize) : recipes;
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