using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PantryChef.Data.Entities
{
    public class Ingredient
    {
        [Column("id")]
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Category { get; set; }
        public double Calories { get; set; }

        [Column("proteins")]
        public double Proteins { get; set; }

        [Column("fats")]
        public double Fats { get; set; }

        [Column("carbohydrates")]
        public double Carbohydrates { get; set; }

        [Column("photo")]
        public string Photo { get; set; }

        public virtual ICollection<UserIngredient> UserIngredients { get; set; } = new List<UserIngredient>();
        public virtual ICollection<RecipeIngredient> RecipeIngredients { get; set; } = new List<RecipeIngredient>();
    }
}