using PantryChef.Business.Models;
using PantryChef.Data.Entities;
using System;
using System.Threading.Tasks;

namespace PantryChef.Business.Interfaces
{
    public interface INutritionService
    {
        Task<Result> UpdateRecipeNutritionAsync(int recipeId);
        (double Calories, double Proteins, double Fats, double Carbohydrates) CalculateNutrition(Recipe recipe);

        Task<Result> AddConsumedNutritionAsync(int userId, double calories, double proteins, double fats, double carbohydrates, DateTime? consumedOn = null);
    }
}