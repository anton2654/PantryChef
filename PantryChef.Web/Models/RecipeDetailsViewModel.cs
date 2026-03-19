using PantryChef.Data.Entities;

namespace PantryChef.Web.Models
{
    public class RecipeDetailsViewModel
    {
        public Recipe Recipe { get; set; } = null!;

        public double Calories { get; set; }

        public double Proteins { get; set; }

        public double Fats { get; set; }

        public double Carbohydrates { get; set; }
    }
}
