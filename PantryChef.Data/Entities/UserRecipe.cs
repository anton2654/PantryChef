using System;

namespace PantryChef.Data.Entities
{
    public class UserRecipe
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int RecipeId { get; set; }
        public bool IsSaved { get; set; } = true;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public virtual User User { get; set; } = null!;
        public virtual Recipe Recipe { get; set; } = null!;
    }
}
