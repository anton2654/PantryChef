using System.ComponentModel.DataAnnotations.Schema; 

namespace PantryChef.Data.Entities
{
    [Table("recipe_ingredient")]
    public class RecipeIngredient
    {

        [NotMapped]
        public int Id { get; set; }

        [Column("recipe_id")] 
        public int RecipeId { get; set; }

        [Column("ingredient_id")]
        public int IngredientId { get; set; }

        [Column("quantity")] 
        public double Quantity { get; set; }

        public virtual Recipe Recipe { get; set; }
        public virtual Ingredient Ingredient { get; set; }
    }
}