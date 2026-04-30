namespace PantryChef.Business.Models
{
    public class RecipeCreateModel
    {
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string Photo { get; set; } = string.Empty;

        public double Calories { get; set; }

        public double Proteins { get; set; }

        public double Fats { get; set; }

        public double Carbohydrates { get; set; }
    }

    public class RecipeEditModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string Photo { get; set; } = string.Empty;

        public double Calories { get; set; }

        public double Proteins { get; set; }

        public double Fats { get; set; }

        public double Carbohydrates { get; set; }
    }

    public class RecipeDeleteModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;
    }
}
