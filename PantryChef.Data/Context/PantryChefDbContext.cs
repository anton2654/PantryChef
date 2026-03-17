using Microsoft.EntityFrameworkCore;
using PantryChef.Data.Entities;

namespace PantryChef.Data.Context
{
    public class PantryChefDbContext(DbContextOptions<PantryChefDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Ingredient> Ingredients { get; set; }
        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<RecipeIngredient> RecipeIngredients { get; set; }
        public DbSet<UserIngredient> UserIngredients { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RecipeIngredient>()
                .HasKey(ri => new { ri.RecipeId, ri.IngredientId });

            modelBuilder.Entity<User>().ToTable("User");
            modelBuilder.Entity<Ingredient>().ToTable("ingredient");
            modelBuilder.Entity<Recipe>().ToTable("recipe");
            modelBuilder.Entity<RecipeIngredient>().ToTable("recipe_ingredient");
            modelBuilder.Entity<UserIngredient>().ToTable("user_ingredient");

            base.OnModelCreating(modelBuilder);
        }
    }
}