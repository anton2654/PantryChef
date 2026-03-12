using System.Collections.Generic;

namespace PantryChef.Data.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public int CalorieGoals { get; set; }
        public string Allergies { get; set; }

        public virtual ICollection<UserIngredient> UserIngredients { get; set; }
    }
}