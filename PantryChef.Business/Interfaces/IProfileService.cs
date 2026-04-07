using PantryChef.Business.Models;
using System.Threading.Tasks;

namespace PantryChef.Business.Interfaces
{
    public interface IProfileService
    {
        Task<Result<UserProfileData>> GetProfileAsync(int userId);
        Task<Result> SetWeightGoalAsync(int userId, double currentWeightKg, double targetWeightKg);
        Task<Result> ResetGoalsAsync(int userId);
        Task<Result<int>> CalculateDailyCaloriesAsync(int userId, double weightKg, double heightCm, int age);
        Task<Result> UpdateManualCalorieGoalAsync(int userId, int dailyCalories);
        Task<Result<UserGoalProgress>> GetGoalProgressAsync(int userId, int? consumedCaloriesToday = null);
    }
}
