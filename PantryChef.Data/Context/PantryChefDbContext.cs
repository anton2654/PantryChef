using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PantryChef.Data.Entities;

namespace PantryChef.Data.Context
{
    public class PantryChefDbContext(DbContextOptions<PantryChefDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public new DbSet<User> Users { get; set; }
        public DbSet<Ingredient> Ingredients { get; set; }
        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<RecipeIngredient> RecipeIngredients { get; set; }
        public DbSet<UserIngredient> UserIngredients { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<RecipeIngredient>()
                .HasKey(ri => new { ri.RecipeId, ri.IngredientId });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("User");
                entity.Property(user => user.Email).IsRequired();
                entity.Property(user => user.Password).IsRequired();
                entity.Property(user => user.Name).IsRequired();
                entity.Property(user => user.Allergies).IsRequired();
                entity.Property(user => user.IsCalorieGoalManuallySet).HasDefaultValue(false);
                entity.HasIndex(user => user.IdentityUserId).IsUnique();
                entity
                    .HasOne(user => user.IdentityUser)
                    .WithOne(identityUser => identityUser.DomainUser)
                    .HasForeignKey<User>(user => user.IdentityUserId)
                    .HasPrincipalKey<ApplicationUser>(identityUser => identityUser.Id)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Ingredient>(entity =>
            {
                entity.ToTable("ingredient");
                entity.Property(ingredient => ingredient.Name).IsRequired();
                entity.Property(ingredient => ingredient.Category).IsRequired();
                entity.Property(ingredient => ingredient.Photo).IsRequired();
            });

            modelBuilder.Entity<Recipe>(entity =>
            {
                entity.ToTable("recipe");
                entity.Property(recipe => recipe.Name).IsRequired();
                entity.Property(recipe => recipe.Description).IsRequired();
                entity.Property(recipe => recipe.Photo).IsRequired();
            });

            modelBuilder.Entity<RecipeIngredient>().ToTable("recipe_ingredient");
            modelBuilder.Entity<UserIngredient>().ToTable("user_ingredient");

            // --- Seed Data ---

            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Name = "Alice Smith",   Email = "alice@example.com", Password = "hashed_pw_1", CalorieGoals = 2000, Allergies = "none" },
                new User { Id = 2, Name = "Bob Johnson",  Email = "bob@example.com",   Password = "hashed_pw_2", CalorieGoals = 2500, Allergies = "gluten" },
                new User { Id = 3, Name = "Carol White",  Email = "carol@example.com", Password = "hashed_pw_3", CalorieGoals = 1800, Allergies = "dairy, nuts" }
            );

            // modelBuilder.Entity<Ingredient>().HasData(
            //     new Ingredient { Id = 1,  Name = "Chicken Breast", Category = "Meat",    Calories = 165, Proteins = 31, Fats = 3.6, Carbohydrates = 0,    Photo = "chicken_breast.jpg" },
            //     new Ingredient { Id = 2,  Name = "Egg",            Category = "Dairy",   Calories = 68,  Proteins = 6,  Fats = 4.8, Carbohydrates = 0.6,  Photo = "egg.jpg" },
            //     new Ingredient { Id = 3,  Name = "Tomato",         Category = "Vegetable", Calories = 18, Proteins = 0.9, Fats = 0.2, Carbohydrates = 3.9, Photo = "tomato.jpg" },
            //     new Ingredient { Id = 4,  Name = "Pasta",          Category = "Grain",   Calories = 371, Proteins = 13, Fats = 1.5, Carbohydrates = 75,   Photo = "pasta.jpg" },
            //     new Ingredient { Id = 5,  Name = "Olive Oil",      Category = "Oil",     Calories = 884, Proteins = 0,  Fats = 100, Carbohydrates = 0,    Photo = "olive_oil.jpg" },
            //     new Ingredient { Id = 6,  Name = "Garlic",         Category = "Vegetable", Calories = 149, Proteins = 6.4, Fats = 0.5, Carbohydrates = 33, Photo = "garlic.jpg" },
            //     new Ingredient { Id = 7,  Name = "Milk",           Category = "Dairy",   Calories = 42,  Proteins = 3.4, Fats = 1,  Carbohydrates = 5,    Photo = "milk.jpg" },
            //     new Ingredient { Id = 8,  Name = "Oats",           Category = "Grain",   Calories = 389, Proteins = 17, Fats = 7,  Carbohydrates = 66,   Photo = "oats.jpg" },
            //     new Ingredient { Id = 9,  Name = "Salmon Fillet",  Category = "Fish",    Calories = 208, Proteins = 20, Fats = 13, Carbohydrates = 0,    Photo = "salmon_fillet.jpg" },
            //     new Ingredient { Id = 10, Name = "Avocado",        Category = "Fruit",   Calories = 160, Proteins = 2,  Fats = 15, Carbohydrates = 9,    Photo = "avocado.jpg" },
            //     new Ingredient { Id = 11, Name = "Banana",         Category = "Fruit",   Calories = 89,  Proteins = 1.1, Fats = 0.3, Carbohydrates = 23,  Photo = "banana.jpg" },
            //     new Ingredient { Id = 12, Name = "Greek Yogurt",   Category = "Dairy",   Calories = 59,  Proteins = 10, Fats = 0.4, Carbohydrates = 3.6,  Photo = "greek_yogurt.jpg" },
            //     new Ingredient { Id = 13, Name = "Spinach",        Category = "Vegetable", Calories = 23, Proteins = 2.9, Fats = 0.4, Carbohydrates = 3.6, Photo = "spinach.jpg" },
            //     new Ingredient { Id = 14, Name = "Quinoa",         Category = "Grain",   Calories = 120, Proteins = 4.4, Fats = 1.9, Carbohydrates = 21.3, Photo = "quinoa.jpg" }
            // );

            modelBuilder.Entity<Recipe>().HasData(
                new Recipe
                {
                    Id = 1, Name = "Scrambled Eggs", Description = "Quick and fluffy scrambled eggs with butter.",
                    Calories = 220, Proteins = 14, Fats = 16, Carbohydrates = 2,
                    Photo = "scrambled_eggs.jpg", Category = "Сніданки"
                },
                new Recipe
                {
                    Id = 2, Name = "Pasta Pomodoro", Description = "Classic Italian pasta with fresh tomato sauce and garlic.",
                    Calories = 480, Proteins = 15, Fats = 10, Carbohydrates = 82,
                    Photo = "pasta_pomodoro.jpg", Category = "Обіди"
                },
                new Recipe
                {
                    Id = 3, Name = "Grilled Chicken", Description = "Juicy grilled chicken breast with olive oil and herbs.",
                    Calories = 250, Proteins = 42, Fats = 8, Carbohydrates = 0,
                    Photo = "grilled_chicken.jpg", Category = "Вечері"
                },
                new Recipe
                {
                    Id = 4, Name = "Oatmeal", Description = "Warm oatmeal with milk, a simple and filling breakfast.",
                    Calories = 310, Proteins = 12, Fats = 6, Carbohydrates = 54,
                    Photo = "oatmeal.jpg", Category = "Сніданки"
                },
                new Recipe
                {
                    Id = 5, Name = "Salmon Quinoa Bowl", Description = "Baked salmon with quinoa, spinach, and avocado.",
                    Calories = 590, Proteins = 36, Fats = 29, Carbohydrates = 44,
                    Photo = "salmon_quinoa_bowl.jpg", Category = "Вечері"
                },
                new Recipe
                {
                    Id = 6, Name = "Greek Yogurt Parfait", Description = "Greek yogurt layered with oats and banana slices.",
                    Calories = 340, Proteins = 22, Fats = 6, Carbohydrates = 51,
                    Photo = "greek_yogurt_parfait.jpg", Category = "Сніданки"
                },
                new Recipe
                {
                    Id = 7, Name = "Avocado Egg Toast", Description = "Creamy avocado and eggs served over toasted bread.",
                    Calories = 410, Proteins = 16, Fats = 23, Carbohydrates = 34,
                    Photo = "avocado_egg_toast.jpg", Category = "Обіди"
                },
                new Recipe
                {
                    Id = 8, Name = "Spinach Omelette", Description = "Protein-rich omelette with spinach and garlic.",
                    Calories = 280, Proteins = 20, Fats = 19, Carbohydrates = 6,
                    Photo = "spinach_omelette.jpg", Category = "Снеки"
                }
            );

            modelBuilder.Entity<RecipeIngredient>().HasData(
                // Scrambled Eggs: eggs + olive oil
                new RecipeIngredient { RecipeId = 1, IngredientId = 2, Quantity = 150 },
                new RecipeIngredient { RecipeId = 1, IngredientId = 5, Quantity = 10 },
                // Pasta Pomodoro: pasta + tomato + garlic + olive oil
                new RecipeIngredient { RecipeId = 2, IngredientId = 4, Quantity = 100 },
                new RecipeIngredient { RecipeId = 2, IngredientId = 3, Quantity = 150 },
                new RecipeIngredient { RecipeId = 2, IngredientId = 6, Quantity = 10 },
                new RecipeIngredient { RecipeId = 2, IngredientId = 5, Quantity = 15 },
                // Grilled Chicken: chicken breast + olive oil + garlic
                new RecipeIngredient { RecipeId = 3, IngredientId = 1, Quantity = 200 },
                new RecipeIngredient { RecipeId = 3, IngredientId = 5, Quantity = 20 },
                new RecipeIngredient { RecipeId = 3, IngredientId = 6, Quantity = 5 },
                // Oatmeal: oats + milk
                new RecipeIngredient { RecipeId = 4, IngredientId = 8, Quantity = 80 },
                new RecipeIngredient { RecipeId = 4, IngredientId = 7, Quantity = 200 },
                // Salmon Quinoa Bowl: salmon + quinoa + spinach + avocado + olive oil
                new RecipeIngredient { RecipeId = 5, IngredientId = 9, Quantity = 180 },
                new RecipeIngredient { RecipeId = 5, IngredientId = 14, Quantity = 120 },
                new RecipeIngredient { RecipeId = 5, IngredientId = 13, Quantity = 70 },
                new RecipeIngredient { RecipeId = 5, IngredientId = 10, Quantity = 80 },
                new RecipeIngredient { RecipeId = 5, IngredientId = 5, Quantity = 8 },
                // Greek Yogurt Parfait: yogurt + oats + banana
                new RecipeIngredient { RecipeId = 6, IngredientId = 12, Quantity = 200 },
                new RecipeIngredient { RecipeId = 6, IngredientId = 8, Quantity = 40 },
                new RecipeIngredient { RecipeId = 6, IngredientId = 11, Quantity = 100 },
                // Avocado Egg Toast: avocado + eggs + olive oil
                new RecipeIngredient { RecipeId = 7, IngredientId = 10, Quantity = 100 },
                new RecipeIngredient { RecipeId = 7, IngredientId = 2, Quantity = 100 },
                new RecipeIngredient { RecipeId = 7, IngredientId = 5, Quantity = 5 },
                // Spinach Omelette: eggs + spinach + garlic + olive oil
                new RecipeIngredient { RecipeId = 8, IngredientId = 2, Quantity = 150 },
                new RecipeIngredient { RecipeId = 8, IngredientId = 13, Quantity = 60 },
                new RecipeIngredient { RecipeId = 8, IngredientId = 6, Quantity = 5 },
                new RecipeIngredient { RecipeId = 8, IngredientId = 5, Quantity = 5 }
            );

            // modelBuilder.Entity<UserIngredient>().HasData(
            //     // Alice's pantry
            //     new UserIngredient { Id = 1, UserId = 1, IngredientId = 1, Quantity = 500 },
            //     new UserIngredient { Id = 2, UserId = 1, IngredientId = 2, Quantity = 6 },
            //     new UserIngredient { Id = 3, UserId = 1, IngredientId = 3, Quantity = 300 },
            //     new UserIngredient { Id = 4, UserId = 1, IngredientId = 5, Quantity = 100 },
            //     // Bob's pantry
            //     new UserIngredient { Id = 5, UserId = 2, IngredientId = 4, Quantity = 400 },
            //     new UserIngredient { Id = 6, UserId = 2, IngredientId = 6, Quantity = 50 },
            //     new UserIngredient { Id = 7, UserId = 2, IngredientId = 7, Quantity = 1000 },
            //     // Carol's pantry
            //     new UserIngredient { Id = 8, UserId = 3, IngredientId = 8, Quantity = 500 },
            //     new UserIngredient { Id = 9, UserId = 3, IngredientId = 7, Quantity = 500 },
            //     // Additional pantry data
            //     new UserIngredient { Id = 10, UserId = 1, IngredientId = 10, Quantity = 3 },
            //     new UserIngredient { Id = 11, UserId = 1, IngredientId = 13, Quantity = 200 },
            //     new UserIngredient { Id = 12, UserId = 2, IngredientId = 11, Quantity = 6 },
            //     new UserIngredient { Id = 13, UserId = 2, IngredientId = 12, Quantity = 400 },
            //     new UserIngredient { Id = 14, UserId = 3, IngredientId = 9, Quantity = 350 },
            //     new UserIngredient { Id = 15, UserId = 3, IngredientId = 14, Quantity = 250 }
            // );
        }
    }
}