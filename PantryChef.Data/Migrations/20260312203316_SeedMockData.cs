using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PantryChef.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedMockData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "User",
                columns: new[] { "Id", "Allergies", "CalorieGoals", "Email", "Name", "Password" },
                values: new object[,]
                {
                    { 1, "none", 2000, "alice@example.com", "Alice Smith", "hashed_pw_1" },
                    { 2, "gluten", 2500, "bob@example.com", "Bob Johnson", "hashed_pw_2" },
                    { 3, "dairy, nuts", 1800, "carol@example.com", "Carol White", "hashed_pw_3" }
                });

            migrationBuilder.InsertData(
                table: "ingredient",
                columns: new[] { "Id", "Calories", "Carbohydrates", "Category", "Fats", "Name", "Photo", "Proteins" },
                values: new object[,]
                {
                    { 1, 165.0, 0.0, "Meat", 3.6000000000000001, "Chicken Breast", "chicken_breast.jpg", 31.0 },
                    { 2, 68.0, 0.59999999999999998, "Dairy", 4.7999999999999998, "Egg", "egg.jpg", 6.0 },
                    { 3, 18.0, 3.8999999999999999, "Vegetable", 0.20000000000000001, "Tomato", "tomato.jpg", 0.90000000000000002 },
                    { 4, 371.0, 75.0, "Grain", 1.5, "Pasta", "pasta.jpg", 13.0 },
                    { 5, 884.0, 0.0, "Oil", 100.0, "Olive Oil", "olive_oil.jpg", 0.0 },
                    { 6, 149.0, 33.0, "Vegetable", 0.5, "Garlic", "garlic.jpg", 6.4000000000000004 },
                    { 7, 42.0, 5.0, "Dairy", 1.0, "Milk", "milk.jpg", 3.3999999999999999 },
                    { 8, 389.0, 66.0, "Grain", 7.0, "Oats", "oats.jpg", 17.0 }
                });

            migrationBuilder.InsertData(
                table: "recipe",
                columns: new[] { "Id", "Calories", "Carbohydrates", "Category", "Description", "Fats", "Name", "Photo", "Proteins" },
                values: new object[,]
                {
                    { 1, 220.0, 2.0, 0, "Quick and fluffy scrambled eggs with butter.", 16.0, "Scrambled Eggs", "scrambled_eggs.jpg", 14.0 },
                    { 2, 480.0, 82.0, 1, "Classic Italian pasta with fresh tomato sauce and garlic.", 10.0, "Pasta Pomodoro", "pasta_pomodoro.jpg", 15.0 },
                    { 3, 250.0, 0.0, 2, "Juicy grilled chicken breast with olive oil and herbs.", 8.0, "Grilled Chicken", "grilled_chicken.jpg", 42.0 },
                    { 4, 310.0, 54.0, 0, "Warm oatmeal with milk, a simple and filling breakfast.", 6.0, "Oatmeal", "oatmeal.jpg", 12.0 }
                });

            migrationBuilder.InsertData(
                table: "recipe_ingredient",
                columns: new[] { "IngredientId", "RecipeId", "Id", "Quantity" },
                values: new object[,]
                {
                    { 2, 1, 0, 3.0 },
                    { 5, 1, 0, 10.0 },
                    { 3, 2, 0, 150.0 },
                    { 4, 2, 0, 100.0 },
                    { 5, 2, 0, 15.0 },
                    { 6, 2, 0, 10.0 },
                    { 1, 3, 0, 200.0 },
                    { 5, 3, 0, 20.0 },
                    { 6, 3, 0, 5.0 },
                    { 7, 4, 0, 200.0 },
                    { 8, 4, 0, 80.0 }
                });

            migrationBuilder.InsertData(
                table: "user_ingredient",
                columns: new[] { "Id", "IngredientId", "Quantity", "UserId" },
                values: new object[,]
                {
                    { 1, 1, 500.0, 1 },
                    { 2, 2, 6.0, 1 },
                    { 3, 3, 300.0, 1 },
                    { 4, 5, 100.0, 1 },
                    { 5, 4, 400.0, 2 },
                    { 6, 6, 50.0, 2 },
                    { 7, 7, 1000.0, 2 },
                    { 8, 8, 500.0, 3 },
                    { 9, 7, 500.0, 3 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "recipe_ingredient",
                keyColumns: new[] { "IngredientId", "RecipeId" },
                keyValues: new object[] { 2, 1 });

            migrationBuilder.DeleteData(
                table: "recipe_ingredient",
                keyColumns: new[] { "IngredientId", "RecipeId" },
                keyValues: new object[] { 5, 1 });

            migrationBuilder.DeleteData(
                table: "recipe_ingredient",
                keyColumns: new[] { "IngredientId", "RecipeId" },
                keyValues: new object[] { 3, 2 });

            migrationBuilder.DeleteData(
                table: "recipe_ingredient",
                keyColumns: new[] { "IngredientId", "RecipeId" },
                keyValues: new object[] { 4, 2 });

            migrationBuilder.DeleteData(
                table: "recipe_ingredient",
                keyColumns: new[] { "IngredientId", "RecipeId" },
                keyValues: new object[] { 5, 2 });

            migrationBuilder.DeleteData(
                table: "recipe_ingredient",
                keyColumns: new[] { "IngredientId", "RecipeId" },
                keyValues: new object[] { 6, 2 });

            migrationBuilder.DeleteData(
                table: "recipe_ingredient",
                keyColumns: new[] { "IngredientId", "RecipeId" },
                keyValues: new object[] { 1, 3 });

            migrationBuilder.DeleteData(
                table: "recipe_ingredient",
                keyColumns: new[] { "IngredientId", "RecipeId" },
                keyValues: new object[] { 5, 3 });

            migrationBuilder.DeleteData(
                table: "recipe_ingredient",
                keyColumns: new[] { "IngredientId", "RecipeId" },
                keyValues: new object[] { 6, 3 });

            migrationBuilder.DeleteData(
                table: "recipe_ingredient",
                keyColumns: new[] { "IngredientId", "RecipeId" },
                keyValues: new object[] { 7, 4 });

            migrationBuilder.DeleteData(
                table: "recipe_ingredient",
                keyColumns: new[] { "IngredientId", "RecipeId" },
                keyValues: new object[] { 8, 4 });

            migrationBuilder.DeleteData(
                table: "user_ingredient",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "user_ingredient",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "user_ingredient",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "user_ingredient",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "user_ingredient",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "user_ingredient",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "user_ingredient",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "user_ingredient",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "user_ingredient",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "User",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "User",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "User",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "ingredient",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ingredient",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "ingredient",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "ingredient",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "ingredient",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "ingredient",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "ingredient",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "ingredient",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "recipe",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "recipe",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "recipe",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "recipe",
                keyColumn: "Id",
                keyValue: 4);
        }
    }
}
