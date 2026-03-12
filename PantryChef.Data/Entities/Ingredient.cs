using System.Collections.Generic;

namespace PantryChef.Data.Entities
{
    public class Ingredient
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Category { get; set; }
        public double Calories { get; set; }
        public double Proteins { get; set; }
        public double Fats { get; set; }
        public double Carbohydrates { get; set; }
        public required string Photo { get; set; }

        public virtual ICollection<UserIngredient> UserIngredients { get; set; } = new List<UserIngredient>();
        public virtual ICollection<RecipeIngredient> RecipeIngredients { get; set; } = new List<RecipeIngredient>();
    }
}