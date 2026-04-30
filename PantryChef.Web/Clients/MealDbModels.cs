using System.Collections.Generic;

namespace PantryChef.Web.Clients
{
    public class MealDbAreaItem
    {
        public string StrArea { get; set; } = string.Empty;

        public string StrCountry { get; set; } = string.Empty;
    }

    public class MealDbMealItem
    {
        public string IdMeal { get; set; } = string.Empty;

        public string StrMeal { get; set; } = string.Empty;

        public string StrMealThumb { get; set; } = string.Empty;

        public string StrArea { get; set; } = string.Empty;

        public string StrCountry { get; set; } = string.Empty;
    }

    internal class MealDbAreasResponse
    {
        public List<MealDbAreaItem> Meals { get; set; } = new();
    }

    internal class MealDbMealsResponse
    {
        public List<MealDbMealItem> Meals { get; set; } = new();
    }

    internal class MealDbLookupMealItem
    {
        public string StrInstructions { get; set; } = string.Empty;
    }

    internal class MealDbLookupResponse
    {
        public List<MealDbLookupMealItem> Meals { get; set; } = new();
    }
}
