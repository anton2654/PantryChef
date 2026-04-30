using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PantryChef.Data.Entities
{
    public class Recipe
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public double Calories { get; set; }

        public double Proteins { get; set; }

        public double Fats { get; set; }

        public double Carbohydrates { get; set; }
        public string Photo { get; set; }
        public string Category { get; set; }

        // Total estimated weight of the recipe in grams (sum of ingredient quantities)
        [NotMapped]
        public double WeightGrams { get; set; }

        public virtual ICollection<RecipeIngredient> RecipeIngredients { get; set; } = new List<RecipeIngredient>();
    }
}