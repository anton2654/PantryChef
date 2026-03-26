using PantryChef.Data.Entities;
using System.Collections.Generic;

namespace PantryChef.Web.Models
{
    public class InventoryIndexViewModel
    {
        public IEnumerable<UserIngredient> Inventory { get; set; }
        public string SelectedCategory { get; set; }
        public IEnumerable<string> AvailableCategories { get; set; }
    }
}