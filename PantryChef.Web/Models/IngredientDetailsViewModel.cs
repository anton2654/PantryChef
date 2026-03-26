using PantryChef.Data.Entities;

namespace PantryChef.Web.Models
{
    public class IngredientDetailsViewModel
    {
        public Ingredient Ingredient { get; set; }
        public double Quantity { get; set; }
    }
}