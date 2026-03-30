using PantryChef.Data.Entities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PantryChef.Web.Models
{
    public class InventoryIndexViewModel
    {
        public IEnumerable<UserIngredient> Inventory { get; set; }
        public string SelectedCategory { get; set; }
        public IEnumerable<string> AvailableCategories { get; set; }

        [Required]
        [Display(Name = "Інгредієнт")]
        public int? AddIngredientId { get; set; }

        [Range(0.1, 100000, ErrorMessage = "Кількість має бути більшою за нуль.")]
        [Display(Name = "Кількість")]
        public double AddQuantity { get; set; }

        public IEnumerable<Ingredient> AvailableIngredients { get; set; }
    }
}