namespace PantryChef.Web.Models
{
    public class ProfileIndexViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public double? CurrentWeightKg { get; set; }
        public double? TargetWeightKg { get; set; }
        public double? HeightCm { get; set; }
        public int? Age { get; set; }

        public int DailyCalorieGoal { get; set; }
        public bool IsCalorieGoalManuallySet { get; set; }
        public int ManualDailyCaloriesInput { get; set; }

        public double? AutoCalculationWeightKg { get; set; }
        public double? AutoCalculationHeightCm { get; set; }
        public int? AutoCalculationAge { get; set; }
        public int? AutoCalculatedCalories { get; set; }

        public int? ConsumedCaloriesToday { get; set; }
        public int? CalorieDifference { get; set; }
        public int? CalorieRemaining { get; set; }
        public int? CalorieExceeded { get; set; }

        public double? WeightDifferenceKg { get; set; }
        public double? WeightRemainingKg { get; set; }
    }
}
