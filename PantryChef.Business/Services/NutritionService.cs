using Microsoft.Extensions.Logging;
using PantryChef.Business.Interfaces;
using PantryChef.Data.Interfaces;
using System;
using System.Threading.Tasks;

namespace PantryChef.Business.Services
{
    public class NutritionService : INutritionService
    {
        private readonly IRecipeRepository _recipeRepo;
        private readonly ILogger<NutritionService> _logger;

        public NutritionService(
            IRecipeRepository recipeRepo,
            ILogger<NutritionService> logger)
        {
            _recipeRepo = recipeRepo;
            _logger = logger;
        }

        public async Task UpdateRecipeNutritionAsync(int recipeId)
        {
            _logger.LogInformation("Початок розрахунку КБЖВ для рецепта {RecipeId}", recipeId);

            try
            {
                var recipe = await _recipeRepo.GetRecipeWithIngredientsByIdAsync(recipeId);

                if (recipe == null)
                {
                    _logger.LogWarning("Рецепт з ID {RecipeId} не знайдено в базі даних.", recipeId);
                    throw new ArgumentException($"Рецепт з ID {recipeId} не існує.");
                }

                double totalCalories = 0;
                double totalProteins = 0;
                double totalFats = 0;
                double totalCarbs = 0;

                foreach (var item in recipe.RecipeIngredients)
                {

                    double multiplier = item.Quantity / 100.0;

                    totalCalories += item.Ingredient.Calories * multiplier;
                    totalProteins += item.Ingredient.Proteins * multiplier;
                    totalFats += item.Ingredient.Fats * multiplier;
                    totalCarbs += item.Ingredient.Carbohydrates * multiplier;
                }

                recipe.Calories = Math.Round(totalCalories, 1);
                recipe.Proteins = Math.Round(totalProteins, 1);
                recipe.Fats = Math.Round(totalFats, 1);
                recipe.Carbohydrates = Math.Round(totalCarbs, 1);

                _recipeRepo.Update(recipe);
                await _recipeRepo.SaveChangesAsync();

                _logger.LogInformation(
                    "Успішно оновлено КБЖВ для рецепта {RecipeId}. Калорії: {Calories}, Білки: {Proteins}, Жири: {Fats}, Вуглеводи: {Carbs}",
                    recipeId, recipe.Calories, recipe.Proteins, recipe.Fats, recipe.Carbohydrates);
            }
            catch (Exception ex)
            {

                throw new InvalidOperationException($"Критична помилка при розрахунку харчової цінності для рецепта {recipeId}.", ex);
            }
        }
    }
}