using PantryChef.Data.Entities;
using System.Collections.Generic;

namespace PantryChef.Web.Models
{
    public class RecipeIndexViewModel
    {
        public IEnumerable<Recipe> Recipes { get; set; } = new List<Recipe>();

        public IEnumerable<RecipeCategoryOptionViewModel> Categories { get; set; } = new List<RecipeCategoryOptionViewModel>();

        public string SelectedCategory { get; set; } = string.Empty;
    }

    public class RecipeCategoryOptionViewModel
    {
        public string Value { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;

        public bool IsSelected { get; set; }
    }
}
