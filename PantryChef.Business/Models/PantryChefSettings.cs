using System.Collections.Generic;

namespace PantryChef.Business.Models
{
    public class PantryChefSettings
    {
        public int DefaultCalorieGoals { get; set; } = 2000;
        public PaginationSettings Pagination { get; set; } = new();
        public InventorySettings Inventory { get; set; } = new();
        public RecipeFilterSettings RecipeFilter { get; set; } = new();
        public CachingSettings Caching { get; set; } = new();
    }

    public class PaginationSettings
    {
        public int DefaultPageSize { get; set; } = 12;
    }

    public class InventorySettings
    {
        public double DefaultAddQuantity { get; set; } = 100.0;
        public int MinSearchLength { get; set; } = 2;
    }

    public class RecipeFilterSettings
    {
        public string AllCategoryLabel { get; set; } = "Всі страви";
        public List<string> Categories { get; set; } =
        [
            "Сніданки",
            "Обіди",
            "Вечері",
            "Десерти",
            "Салати",
            "Гарніри",
            "Закуски",
            "Снеки",
            "Пісні страви",
            "Перші страви",
            "Другі страви"
        ];
    }

    public class CachingSettings
    {
        public int AvailableIngredientsTtlMinutes { get; set; } = 30;
        public int AvailableRecipeCategoriesTtlMinutes { get; set; } = 30;
    }
}