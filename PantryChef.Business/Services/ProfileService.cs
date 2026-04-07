using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PantryChef.Business.Interfaces;
using PantryChef.Business.Models;
using PantryChef.Data.Interfaces;
using System;
using System.Threading.Tasks;

namespace PantryChef.Business.Services
{
    public class ProfileService : IProfileService
    {
        private const int MinAge = 10;
        private const int MaxAge = 100;
        private const double MinHeightCm = 100;
        private const double MaxHeightCm = 250;
        private const double MinWeightKg = 20;
        private const double MaxWeightKg = 350;
        private const int MinManualCalories = 800;
        private const int MaxManualCalories = 6000;

        private readonly IUserRepository _userRepository;
        private readonly ILogger<ProfileService> _logger;
        private readonly PantryChefSettings _settings;

        public ProfileService(
            IUserRepository userRepository,
            ILogger<ProfileService> logger,
            IOptions<PantryChefSettings> options)
        {
            _userRepository = userRepository;
            _logger = logger;
            _settings = options?.Value ?? new PantryChefSettings();
        }

        public async Task<Result<UserProfileData>> GetProfileAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return new Error("Користувача не знайдено.");
            }

            return new UserProfileData
            {
                UserId = user.Id,
                Name = user.Name,
                Email = user.Email,
                CurrentWeightKg = user.CurrentWeightKg,
                TargetWeightKg = user.TargetWeightKg,
                HeightCm = user.HeightCm,
                Age = user.Age,
                DailyCalorieGoal = user.CalorieGoals,
                IsCalorieGoalManuallySet = user.IsCalorieGoalManuallySet
            };
        }

        public async Task<Result> SetWeightGoalAsync(int userId, double currentWeightKg, double targetWeightKg)
        {
            if (!IsValidWeight(currentWeightKg) || !IsValidWeight(targetWeightKg))
            {
                return Result.Failure("Вага має бути в діапазоні від 20 до 350 кг.");
            }

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return Result.Failure("Користувача не знайдено.");
            }

            user.CurrentWeightKg = Math.Round(currentWeightKg, 1);
            user.TargetWeightKg = Math.Round(targetWeightKg, 1);

            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();

            _logger.LogInformation(
                "Користувач {UserId} встановив/оновив вагу: поточна {CurrentWeightKg}, цільова {TargetWeightKg}",
                userId,
                user.CurrentWeightKg,
                user.TargetWeightKg);

            return Result.Success();
        }

        public async Task<Result> ResetGoalsAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return Result.Failure("Користувача не знайдено.");
            }

            user.CurrentWeightKg = null;
            user.TargetWeightKg = null;
            user.HeightCm = null;
            user.Age = null;
            user.CalorieGoals = _settings.DefaultCalorieGoals;
            user.IsCalorieGoalManuallySet = false;

            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();

            _logger.LogInformation("Користувач {UserId} скинув цілі по вазі та калоріях", userId);

            return Result.Success();
        }

        public async Task<Result<int>> CalculateDailyCaloriesAsync(int userId, double weightKg, double heightCm, int age)
        {
            if (!IsValidWeight(weightKg))
            {
                return Result<int>.Failure("Вага має бути в діапазоні від 20 до 350 кг.");
            }

            if (!IsValidHeight(heightCm))
            {
                return Result<int>.Failure("Зріст має бути в діапазоні від 100 до 250 см.");
            }

            if (!IsValidAge(age))
            {
                return Result<int>.Failure("Вік має бути в діапазоні від 10 до 100 років.");
            }

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return Result<int>.Failure("Користувача не знайдено.");
            }

            // Mifflin-St Jeor (Формула Міффліна-Сан Жеора)
            var calculatedCalories = (int)Math.Round(
                (10 * weightKg) + (6.25 * heightCm) - (5 * age) + 5,
                MidpointRounding.AwayFromZero);

            calculatedCalories = Math.Clamp(calculatedCalories, 1200, 4500);

            _logger.LogInformation(
                "Користувач {UserId} виконав авто-розрахунок добової норми калорій: {Calories}",
                userId,
                calculatedCalories);

            return Result<int>.Success(calculatedCalories);
        }

        public async Task<Result> UpdateManualCalorieGoalAsync(int userId, int dailyCalories)
        {
            if (dailyCalories < MinManualCalories || dailyCalories > MaxManualCalories)
            {
                return Result.Failure("Норма калорій має бути в діапазоні від 800 до 6000 ккал.");
            }

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return Result.Failure("Користувача не знайдено.");
            }

            user.CalorieGoals = dailyCalories;
            user.IsCalorieGoalManuallySet = true;

            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();

            _logger.LogInformation(
                "Користувач {UserId} вручну оновив добову норму калорій: {Calories}",
                userId,
                dailyCalories);

            return Result.Success();
        }

        public async Task<Result<UserGoalProgress>> GetGoalProgressAsync(int userId, int? consumedCaloriesToday = null)
        {
            if (consumedCaloriesToday.HasValue && consumedCaloriesToday.Value < 0)
            {
                return Result<UserGoalProgress>.Failure("Спожиті калорії не можуть бути від'ємними.");
            }

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return Result<UserGoalProgress>.Failure("Користувача не знайдено.");
            }

            double? weightDifference = null;
            double? weightRemaining = null;

            if (user.CurrentWeightKg.HasValue && user.TargetWeightKg.HasValue)
            {
                weightDifference = Math.Round(user.TargetWeightKg.Value - user.CurrentWeightKg.Value, 1);
                weightRemaining = Math.Round(Math.Abs(weightDifference.Value), 1);
            }

            int? calorieDifference = consumedCaloriesToday.HasValue
                ? user.CalorieGoals - consumedCaloriesToday.Value
                : null;

            int? calorieRemaining = calorieDifference.HasValue
                ? Math.Max(calorieDifference.Value, 0)
                : null;

            int? calorieExceeded = calorieDifference.HasValue
                ? Math.Max(-calorieDifference.Value, 0)
                : null;

            return new UserGoalProgress
            {
                WeightDifferenceKg = weightDifference,
                WeightRemainingKg = weightRemaining,
                CalorieDifference = calorieDifference,
                CalorieRemaining = calorieRemaining,
                CalorieExceeded = calorieExceeded
            };
        }

        private static bool IsValidWeight(double weightKg)
        {
            return weightKg >= MinWeightKg && weightKg <= MaxWeightKg;
        }

        private static bool IsValidHeight(double heightCm)
        {
            return heightCm >= MinHeightCm && heightCm <= MaxHeightCm;
        }

        private static bool IsValidAge(int age)
        {
            return age >= MinAge && age <= MaxAge;
        }
    }
}
