using Microsoft.Extensions.Logging;
using PantryChef.Business.Interfaces;
using PantryChef.Data.Entities;
using PantryChef.Data.Interfaces;
using System;
using System.Linq; 
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

        public (double Calories, double Proteins, double Fats, double Carbohydrates) CalculateNutrition(Recipe recipe)
        {
            if (recipe.RecipeIngredients == null || recipe.RecipeIngredients.Count == 0)
            {
                return (
                    Math.Round(recipe.Calories, 1),
                    Math.Round(recipe.Proteins, 1),
                    Math.Round(recipe.Fats, 1),
                    Math.Round(recipe.Carbohydrates, 1));
            }

            var calories = recipe.RecipeIngredients.Sum(item => item.Ingredient.Calories * (item.Quantity / 100.0));
            var proteins = recipe.RecipeIngredients.Sum(item => item.Ingredient.Proteins * (item.Quantity / 100.0));
            var fats = recipe.RecipeIngredients.Sum(item => item.Ingredient.Fats * (item.Quantity / 100.0));
            var carbohydrates = recipe.RecipeIngredients.Sum(item => item.Ingredient.Carbohydrates * (item.Quantity / 100.0));

            return (
                Math.Round(calories, 1),
                Math.Round(proteins, 1),
                Math.Round(fats, 1),
                Math.Round(carbohydrates, 1));
        }
    }
}