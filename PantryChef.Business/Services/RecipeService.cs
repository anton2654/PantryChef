using Microsoft.Extensions.Logging;
using PantryChef.Business.Interfaces;
using PantryChef.Data.Entities;
using PantryChef.Data.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PantryChef.Business.Services
{
    public class RecipeService : IRecipeService
    {
        private readonly IRecipeRepository _recipeRepo;
        private readonly ILogger<RecipeService> _logger;

        public RecipeService(IRecipeRepository recipeRepo, ILogger<RecipeService> logger)
        {
            _recipeRepo = recipeRepo;
            _logger = logger;
        }

        public async Task<IEnumerable<Recipe>> GetAllRecipesWithIngredientsAsync()
        {
            _logger.LogInformation("Отримання всіх рецептів з інгредієнтами.");
            return await _recipeRepo.GetAllRecipesWithIngredientsAsync();
        }

        public async Task<IEnumerable<Recipe>> GetRecipesByCategoryAsync(string category)
        {
            _logger.LogInformation("Отримання рецептів за категорією: {Category}", category);
            return await _recipeRepo.GetRecipesByCategoryAsync(category);
        }

        public async Task<Recipe> GetRecipeWithIngredientsByIdAsync(int id)
        {
            _logger.LogInformation("Отримання рецепта з інгредієнтами за ID: {Id}", id);
            return await _recipeRepo.GetRecipeWithIngredientsByIdAsync(id);
        }
    }
}