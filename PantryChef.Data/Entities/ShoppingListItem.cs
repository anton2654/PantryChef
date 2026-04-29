using System;

namespace PantryChef.Data.Entities
{
    public class ShoppingListItem
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int IngredientId { get; set; }
        public double Quantity { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual User User { get; set; } = null!;
        public virtual Ingredient Ingredient { get; set; } = null!;
    }
}
