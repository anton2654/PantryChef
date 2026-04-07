using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PantryChef.Business.Interfaces;
using PantryChef.Business.Models;
using PantryChef.Data.Entities;
using PantryChef.Data.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PantryChef.Business.Services
{
    public class RecipeService : IRecipeService
    {
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
            var recipes = await _recipeRepo.GetAllRecipesWithIngredientsAsync();
            return ApplyDefaultPageSize(recipes);
        }

        public async Task<IEnumerable<Recipe>> GetRecipesByCategoryAsync(string category)
        {
            _logger.LogInformation("Отримання рецептів за категорією: {Category}", category);
            var recipes = await _recipeRepo.GetRecipesByCategoryAsync(category);
            return ApplyDefaultPageSize(recipes);
        }

        public async Task<Recipe> GetRecipeWithIngredientsByIdAsync(int id)
        {
            _logger.LogInformation("Отримання рецепта з інгредієнтами за ID: {Id}", id);
            return await _recipeRepo.GetRecipeWithIngredientsByIdAsync(id);
        }

        private IEnumerable<Recipe> ApplyDefaultPageSize(IEnumerable<Recipe> recipes)
        {
            var pageSize = _settings.Pagination.DefaultPageSize;
            return pageSize > 0 ? recipes.Take(pageSize) : recipes;
        }
    }
}