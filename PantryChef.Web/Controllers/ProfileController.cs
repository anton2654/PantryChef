using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PantryChef.Business.Interfaces;
using PantryChef.Web.Models;
using System.Threading.Tasks;

namespace PantryChef.Web.Controllers
{
    [Authorize]
    public class ProfileController : BaseController
    {
        private readonly IProfileService _profileService;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(
            IProfileService profileService,
            ILogger<ProfileController> logger)
        {
            _profileService = profileService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            int? consumedCaloriesToday = null,
            double? autoWeightKg = null,
            double? autoHeightCm = null,
            int? autoAge = null,
            int? autoCalculatedCalories = null)
        {
            var profileResult = await _profileService.GetProfileAsync(CurrentUserId);
            if (!profileResult.IsSuccess)
            {
                SetErrorMessage(profileResult.ErrorMessage);
                return View(new ProfileIndexViewModel());
            }

            var progressResult = await _profileService.GetGoalProgressAsync(CurrentUserId, consumedCaloriesToday);
            if (!progressResult.IsSuccess)
            {
                SetErrorMessage(progressResult.ErrorMessage);
                return View(MapToViewModel(
                    profileResult.Data,
                    null,
                    consumedCaloriesToday,
                    autoWeightKg,
                    autoHeightCm,
                    autoAge,
                    autoCalculatedCalories));
            }

            return View(MapToViewModel(
                profileResult.Data,
                progressResult.Data,
                consumedCaloriesToday,
                autoWeightKg,
                autoHeightCm,
                autoAge,
                autoCalculatedCalories));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetWeightGoals(double currentWeightKg, double targetWeightKg)
        {
            if (!ModelState.IsValid)
            {
                SetErrorMessage("Некоректний формат ваги. Спробуйте значення у форматі 60,5 або 60.5.");
                return RedirectToAction(nameof(Index));
            }

            var result = await _profileService.SetWeightGoalAsync(CurrentUserId, currentWeightKg, targetWeightKg);
            if (!result.IsSuccess)
            {
                SetErrorMessage(result.ErrorMessage);
                return RedirectToAction(nameof(Index));
            }

            SetSuccessMessage("Цілі по вазі успішно збережено.");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetGoals()
        {
            var result = await _profileService.ResetGoalsAsync(CurrentUserId);
            if (!result.IsSuccess)
            {
                SetErrorMessage(result.ErrorMessage);
                return RedirectToAction(nameof(Index));
            }

            SetSuccessMessage("Цілі скинуто до стандартних значень.");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CalculateDailyCalories(double weightKg, double heightCm, int age)
        {
            if (!ModelState.IsValid)
            {
                SetErrorMessage("Некоректний формат параметрів. Перевірте вагу, зріст і вік.");
                return RedirectToAction(nameof(Index));
            }

            var result = await _profileService.CalculateDailyCaloriesAsync(CurrentUserId, weightKg, heightCm, age);
            if (!result.IsSuccess)
            {
                SetErrorMessage(result.ErrorMessage);
                return RedirectToAction(nameof(Index));
            }

            SetSuccessMessage($"Авто-розрахунок: {result.Data} ккал. Щоб застосувати це значення, натисніть \"Зберегти\" у блоці ручного редагування калорій.");

            return RedirectToAction(nameof(Index), new
            {
                autoWeightKg = weightKg,
                autoHeightCm = heightCm,
                autoAge = age,
                autoCalculatedCalories = result.Data
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateDailyCalories(int dailyCalories)
        {
            var result = await _profileService.UpdateManualCalorieGoalAsync(CurrentUserId, dailyCalories);
            if (!result.IsSuccess)
            {
                SetErrorMessage(result.ErrorMessage);
                return RedirectToAction(nameof(Index));
            }

            SetSuccessMessage("Добову норму калорій збережено вручну.");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CalculateGoalProgress(int consumedCaloriesToday)
        {
            if (consumedCaloriesToday < 0)
            {
                SetErrorMessage("Спожиті калорії не можуть бути від'ємними.");
                return RedirectToAction(nameof(Index));
            }

            _logger.LogInformation(
                "Користувач {UserId} виконав розрахунок залишку калорій за день: {ConsumedCaloriesToday}",
                CurrentUserId,
                consumedCaloriesToday);

            return RedirectToAction(nameof(Index), new { consumedCaloriesToday });
        }

        private static ProfileIndexViewModel MapToViewModel(
            PantryChef.Business.Models.UserProfileData profile,
            PantryChef.Business.Models.UserGoalProgress progress,
            int? consumedCaloriesToday,
            double? autoWeightKg,
            double? autoHeightCm,
            int? autoAge,
            int? autoCalculatedCalories)
        {
            return new ProfileIndexViewModel
            {
                Name = profile.Name,
                Email = profile.Email,
                CurrentWeightKg = profile.CurrentWeightKg,
                TargetWeightKg = profile.TargetWeightKg,
                HeightCm = profile.HeightCm,
                Age = profile.Age,
                DailyCalorieGoal = profile.DailyCalorieGoal,
                ManualDailyCaloriesInput = autoCalculatedCalories ?? profile.DailyCalorieGoal,
                AutoCalculationWeightKg = autoWeightKg,
                AutoCalculationHeightCm = autoHeightCm ?? profile.HeightCm,
                AutoCalculationAge = autoAge ?? profile.Age,
                AutoCalculatedCalories = autoCalculatedCalories,
                IsCalorieGoalManuallySet = profile.IsCalorieGoalManuallySet,
                ConsumedCaloriesToday = consumedCaloriesToday,
                CalorieDifference = progress?.CalorieDifference,
                CalorieRemaining = progress?.CalorieRemaining,
                CalorieExceeded = progress?.CalorieExceeded,
                WeightDifferenceKg = progress?.WeightDifferenceKg,
                WeightRemainingKg = progress?.WeightRemainingKg
            };
        }
    }
}
