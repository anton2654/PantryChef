using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PantryChef.Data.Entities
{
    public class Ingredient
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("category")]
        public string Category { get; set; }

        [Column("calories")]
        public double Calories { get; set; }

        [Column("proteins")]
        public double Proteins { get; set; }

        [Column("fats")]
        public double Fats { get; set; }

        [Column("carbohydrates")]
        public double Carbohydrates { get; set; }

        [Column("photo")]
        public string Photo { get; set; }

        public virtual ICollection<UserIngredient> UserIngredients { get; set; }
        public virtual ICollection<RecipeIngredient> RecipeIngredients { get; set; }
    }
}