using PantryChef.Data.Entities;
using System.Threading.Tasks;

namespace PantryChef.Business.Interfaces
{
    public interface INutritionService
    {
        Task UpdateRecipeNutritionAsync(int recipeId);
        
        (double Calories, double Proteins, double Fats, double Carbohydrates) CalculateNutrition(Recipe recipe);
    }
}