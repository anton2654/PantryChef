using PantryChef.Data.Entities;
using System.Collections.Generic;

namespace PantryChef.Web.Models
{
    public class RecipeIndexViewModel
    {
        public IEnumerable<Recipe> Recipes { get; set; } = new List<Recipe>();

        public IEnumerable<RecipeCategoryOptionViewModel> Categories { get; set; } = new List<RecipeCategoryOptionViewModel>();

        public string SelectedCategory { get; set; } = string.Empty;

        public int CurrentPage { get; set; } = 1;

        public int TotalPages { get; set; }

        public int PageSize { get; set; }

        public int TotalItems { get; set; }

        public bool HasPreviousPage => CurrentPage > 1;

        public bool HasNextPage => CurrentPage < TotalPages;
    }

    public class RecipeCategoryOptionViewModel
    {
        public string Value { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;

        public bool IsSelected { get; set; }
    }
}
