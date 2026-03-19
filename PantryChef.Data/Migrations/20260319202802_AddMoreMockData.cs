using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PantryChef.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMoreMockData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ingredient",
                columns: new[] { "Id", "Calories", "Carbohydrates", "Category", "Fats", "Name", "Photo", "Proteins" },
                values: new object[,]
                {
                    { 9, 208.0, 0.0, "Fish", 13.0, "Salmon Fillet", "salmon_fillet.jpg", 20.0 },
                    { 10, 160.0, 9.0, "Fruit", 15.0, "Avocado", "avocado.jpg", 2.0 },
                    { 11, 89.0, 23.0, "Fruit", 0.29999999999999999, "Banana", "banana.jpg", 1.1000000000000001 },
                    { 12, 59.0, 3.6000000000000001, "Dairy", 0.40000000000000002, "Greek Yogurt", "greek_yogurt.jpg", 10.0 },
                    { 13, 23.0, 3.6000000000000001, "Vegetable", 0.40000000000000002, "Spinach", "spinach.jpg", 2.8999999999999999 },
                    { 14, 120.0, 21.300000000000001, "Grain", 1.8999999999999999, "Quinoa", "quinoa.jpg", 4.4000000000000004 }
                });

            migrationBuilder.InsertData(
                table: "recipe",
                columns: new[] { "Id", "Calories", "Carbohydrates", "Category", "Description", "Fats", "Name", "Photo", "Proteins" },
                values: new object[,]
                {
                    { 5, 590.0, 44.0, 2, "Baked salmon with quinoa, spinach, and avocado.", 29.0, "Salmon Quinoa Bowl", "salmon_quinoa_bowl.jpg", 36.0 },
                    { 6, 340.0, 51.0, 0, "Greek yogurt layered with oats and banana slices.", 6.0, "Greek Yogurt Parfait", "greek_yogurt_parfait.jpg", 22.0 },
                    { 7, 410.0, 34.0, 1, "Creamy avocado and eggs served over toasted bread.", 23.0, "Avocado Egg Toast", "avocado_egg_toast.jpg", 16.0 },
                    { 8, 280.0, 6.0, 3, "Protein-rich omelette with spinach and garlic.", 19.0, "Spinach Omelette", "spinach_omelette.jpg", 20.0 }
                });

            migrationBuilder.InsertData(
                table: "recipe_ingredient",
                columns: new[] { "IngredientId", "RecipeId", "Id", "Quantity" },
                values: new object[,]
                {
                    { 5, 5, 0, 8.0 },
                    { 9, 5, 0, 180.0 },
                    { 10, 5, 0, 80.0 },
                    { 13, 5, 0, 70.0 },
                    { 14, 5, 0, 120.0 },
                    { 8, 6, 0, 40.0 },
                    { 11, 6, 0, 100.0 },
                    { 12, 6, 0, 200.0 },
                    { 2, 7, 0, 2.0 },
                    { 5, 7, 0, 5.0 },
                    { 10, 7, 0, 100.0 },
                    { 2, 8, 0, 3.0 },
                    { 5, 8, 0, 5.0 },
                    { 6, 8, 0, 5.0 },
                    { 13, 8, 0, 60.0 }
                });

            migrationBuilder.InsertData(
                table: "user_ingredient",
                columns: new[] { "Id", "IngredientId", "Quantity", "UserId" },
                values: new object[,]
                {
                    { 10, 10, 3.0, 1 },
                    { 11, 13, 200.0, 1 },
                    { 12, 11, 6.0, 2 },
                    { 13, 12, 400.0, 2 },
                    { 14, 9, 350.0, 3 },
                    { 15, 14, 250.0, 3 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "recipe_ingredient",
                keyColumns: new[] { "IngredientId", "RecipeId" },
                keyValues: new object[] { 5, 5 });

            migrationBuilder.DeleteData(
                table: "recipe_ingredient",
                keyColumns: new[] { "IngredientId", "RecipeId" },
                keyValues: new object[] { 9, 5 });

            migrationBuilder.DeleteData(
                table: "recipe_ingredient",
                keyColumns: new[] { "IngredientId", "RecipeId" },
                keyValues: new object[] { 10, 5 });

            migrationBuilder.DeleteData(
                table: "recipe_ingredient",
                keyColumns: new[] { "IngredientId", "RecipeId" },
                keyValues: new object[] { 13, 5 });

            migrationBuilder.DeleteData(
                table: "recipe_ingredient",
                keyColumns: new[] { "IngredientId", "RecipeId" },
                keyValues: new object[] { 14, 5 });

            migrationBuilder.DeleteData(
                table: "recipe_ingredient",
                keyColumns: new[] { "IngredientId", "RecipeId" },
                keyValues: new object[] { 8, 6 });

            migrationBuilder.DeleteData(
                table: "recipe_ingredient",
                keyColumns: new[] { "IngredientId", "RecipeId" },
                keyValues: new object[] { 11, 6 });

            migrationBuilder.DeleteData(
                table: "recipe_ingredient",
                keyColumns: new[] { "IngredientId", "RecipeId" },
                keyValues: new object[] { 12, 6 });

            migrationBuilder.DeleteData(
                table: "recipe_ingredient",
                keyColumns: new[] { "IngredientId", "RecipeId" },
                keyValues: new object[] { 2, 7 });

            migrationBuilder.DeleteData(
                table: "recipe_ingredient",
                keyColumns: new[] { "IngredientId", "RecipeId" },
                keyValues: new object[] { 5, 7 });

            migrationBuilder.DeleteData(
                table: "recipe_ingredient",
                keyColumns: new[] { "IngredientId", "RecipeId" },
                keyValues: new object[] { 10, 7 });

            migrationBuilder.DeleteData(
                table: "recipe_ingredient",
                keyColumns: new[] { "IngredientId", "RecipeId" },
                keyValues: new object[] { 2, 8 });

            migrationBuilder.DeleteData(
                table: "recipe_ingredient",
                keyColumns: new[] { "IngredientId", "RecipeId" },
                keyValues: new object[] { 5, 8 });

            migrationBuilder.DeleteData(
                table: "recipe_ingredient",
                keyColumns: new[] { "IngredientId", "RecipeId" },
                keyValues: new object[] { 6, 8 });

            migrationBuilder.DeleteData(
                table: "recipe_ingredient",
                keyColumns: new[] { "IngredientId", "RecipeId" },
                keyValues: new object[] { 13, 8 });

            migrationBuilder.DeleteData(
                table: "user_ingredient",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "user_ingredient",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "user_ingredient",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "user_ingredient",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "user_ingredient",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "user_ingredient",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "ingredient",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "ingredient",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "ingredient",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "ingredient",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "ingredient",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "ingredient",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "recipe",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "recipe",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "recipe",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "recipe",
                keyColumn: "Id",
                keyValue: 8);
        }
    }
}
