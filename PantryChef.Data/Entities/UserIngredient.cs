namespace PantryChef.Data.Entities
{
    public class UserIngredient
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int IngredientId { get; set; }
        public double Quantity { get; set; }
        
        public virtual User User { get; set; }
        public virtual Ingredient Ingredient { get; set; }
    }
}