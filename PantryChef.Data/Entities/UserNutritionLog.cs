using System;

namespace PantryChef.Data.Entities
{
    public class UserNutritionLog
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime LogDate { get; set; }
        public double Calories { get; set; }
        public double Proteins { get; set; }
        public double Fats { get; set; }
        public double Carbohydrates { get; set; }

        public virtual User User { get; set; } = null!;
    }
}
