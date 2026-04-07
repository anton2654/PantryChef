using System.Collections.Generic;

namespace PantryChef.Data.Entities
{
    public class User
    {
        public int Id { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string Name { get; set; }
        public int CalorieGoals { get; set; }
        public double? CurrentWeightKg { get; set; }
        public double? TargetWeightKg { get; set; }
        public double? HeightCm { get; set; }
        public int? Age { get; set; }
        public bool IsCalorieGoalManuallySet { get; set; }
        public required string Allergies { get; set; }
        public string IdentityUserId { get; set; }

        public virtual ApplicationUser IdentityUser { get; set; }

        public virtual ICollection<UserIngredient> UserIngredients { get; set; } = new List<UserIngredient>();
    }
}