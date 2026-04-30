using PantryChef.Data.Entities;
using System.Collections.Generic;

namespace PantryChef.Business.Models
{
    public class RecipeMatchResult
    {
        public Recipe Recipe { get; set; } = null!;
        public IReadOnlyList<IngredientDeficit> MissingIngredients { get; set; } = new List<IngredientDeficit>();
        public bool IsFullMatch => MissingIngredients.Count == 0;
    }

    public class IngredientDeficit
    {
        public int IngredientId { get; set; }
        public string IngredientName { get; set; } = string.Empty;
        public double RequiredQuantity { get; set; }
        public double AvailableQuantity { get; set; }
        public double MissingQuantity { get; set; }
    }
}
