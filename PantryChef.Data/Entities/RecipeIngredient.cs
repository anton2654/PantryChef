namespace PantryChef.Data.Entities
{
    public class RecipeIngredient
    {
        public int Id { get; set; }
        public int RecipeId { get; set; }
        public int IngredientId { get; set; }
        public double Quantity { get; set; }

        public virtual Recipe Recipe { get; set; } = null!;
        public virtual Ingredient Ingredient { get; set; } = null!;
    }
}