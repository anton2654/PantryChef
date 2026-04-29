using PantryChef.Business.Models;
using PantryChef.Data.Entities;
using System.Collections.Generic;

namespace PantryChef.Web.Models
{
    public class RecipePlannerViewModel
    {
        public IReadOnlyList<RecipeMatchResult> FullMatches { get; set; } = new List<RecipeMatchResult>();
        public IReadOnlyList<RecipeMatchResult> PartialMatches { get; set; } = new List<RecipeMatchResult>();
        public IReadOnlyList<ShoppingListItem> ShoppingList { get; set; } = new List<ShoppingListItem>();
    }
}
