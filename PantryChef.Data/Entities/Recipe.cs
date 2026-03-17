using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PantryChef.Data.Entities
{
    public class Recipe
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("description")]
        public string Description { get; set; }

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

        [Column("category")]
        public DishCategory Category { get; set; }

        public virtual ICollection<RecipeIngredient> RecipeIngredients { get; set; }
    }
}