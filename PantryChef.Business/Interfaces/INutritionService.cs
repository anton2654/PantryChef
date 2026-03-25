using PantryChef.Business.Models;
using PantryChef.Data.Entities;
using System.Threading.Tasks;

namespace PantryChef.Business.Interfaces
{
    public interface INutritionService
    {
        Task<Result> UpdateRecipeNutritionAsync(int recipeId);
        (double Calories, double Proteins, double Fats, double Carbohydrates) CalculateNutrition(Recipe recipe);
    }
}