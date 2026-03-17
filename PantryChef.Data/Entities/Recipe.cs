using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PantryChef.Data.Entities
{
    public class Recipe
    {
        [Column("id")]
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public double Calories { get; set; }

        [Column("proteins")]
        public double Proteins { get; set; }

        [Column("fats")]
        public double Fats { get; set; }

        [Column("carbohydrates")]
        public double Carbohydrates { get; set; }
        public string Photo { get; set; }
        public DishCategory Category { get; set; }

        public virtual ICollection<RecipeIngredient> RecipeIngredients { get; set; } = new List<RecipeIngredient>();
    }
}