using Microsoft.Extensions.Logging;
using PantryChef.Business.Interfaces;
using PantryChef.Business.Models;
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
        private readonly IUserNutritionLogRepository _nutritionLogRepo;
        private readonly ILogger<NutritionService> _logger;

        public NutritionService(
            IRecipeRepository recipeRepo,
            IUserNutritionLogRepository nutritionLogRepo,
            ILogger<NutritionService> logger)
        {
            _recipeRepo = recipeRepo;
            _nutritionLogRepo = nutritionLogRepo;
            _logger = logger;
        }

        public async Task<Result> UpdateRecipeNutritionAsync(int recipeId)
        {
            _logger.LogInformation("Початок розрахунку КБЖВ для рецепта {RecipeId}", recipeId);

            var recipe = await _recipeRepo.GetRecipeWithIngredientsByIdAsync(recipeId);

            if (recipe == null)
            {
                return new Error($"Рецепт з ID {recipeId} не існує.");
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

            _logger.LogInformation("Успішно оновлено КБЖВ для рецепта {RecipeId}.", recipeId);
            return new Success();
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

        public async Task<Result> AddConsumedNutritionAsync(
            int userId,
            double calories,
            double proteins,
            double fats,
            double carbohydrates,
            DateTime? consumedOn = null)
        {
            if (userId <= 0)
            {
                _logger.LogWarning("Некоректний ідентифікатор користувача для фіксації харчування: {UserId}", userId);
                return new Error("Некоректний ідентифікатор користувача.");
            }

            if (calories < 0 || proteins < 0 || fats < 0 || carbohydrates < 0)
            {
                _logger.LogWarning("Негативні поживні значення для користувача {UserId}", userId);
                return new Error("Поживні значення не можуть бути від'ємними.");
            }

            var logDate = (consumedOn ?? DateTime.UtcNow).Date;

            var existingLog = await _nutritionLogRepo.GetByUserAndDateAsync(userId, logDate);

            if (existingLog == null)
            {
                await _nutritionLogRepo.AddAsync(new UserNutritionLog
                {
                    UserId = userId,
                    LogDate = logDate,
                    Calories = calories,
                    Proteins = proteins,
                    Fats = fats,
                    Carbohydrates = carbohydrates
                });
            }
            else
            {
                existingLog.Calories += calories;
                existingLog.Proteins += proteins;
                existingLog.Fats += fats;
                existingLog.Carbohydrates += carbohydrates;
                _nutritionLogRepo.Update(existingLog);
            }

            await _nutritionLogRepo.SaveChangesAsync();

            _logger.LogInformation("Оновлено добову поживну статистику для користувача {UserId} на дату {LogDate}", userId, logDate);
            return Result.Success();
        }
    }
}