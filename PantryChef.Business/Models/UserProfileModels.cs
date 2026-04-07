namespace PantryChef.Business.Models
{
    public class UserProfileData
    {
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public double? CurrentWeightKg { get; set; }
        public double? TargetWeightKg { get; set; }
        public double? HeightCm { get; set; }
        public int? Age { get; set; }
        public int DailyCalorieGoal { get; set; }
        public bool IsCalorieGoalManuallySet { get; set; }
    }

    public class UserGoalProgress
    {
        public double? WeightDifferenceKg { get; set; }
        public double? WeightRemainingKg { get; set; }
        public int? CalorieDifference { get; set; }
        public int? CalorieRemaining { get; set; }
        public int? CalorieExceeded { get; set; }
    }
}
