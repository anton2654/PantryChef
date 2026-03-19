using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PantryChef.Data.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeRecipeIngredientQuantitiesToGrams : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "recipe_ingredient",
                keyColumns: new[] { "IngredientId", "RecipeId" },
                keyValues: new object[] { 2, 1 },
                column: "Quantity",
                value: 150.0);

            migrationBuilder.UpdateData(
                table: "recipe_ingredient",
                keyColumns: new[] { "IngredientId", "RecipeId" },
                keyValues: new object[] { 2, 7 },
                column: "Quantity",
                value: 100.0);

            migrationBuilder.UpdateData(
                table: "recipe_ingredient",
                keyColumns: new[] { "IngredientId", "RecipeId" },
                keyValues: new object[] { 2, 8 },
                column: "Quantity",
                value: 150.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "recipe_ingredient",
                keyColumns: new[] { "IngredientId", "RecipeId" },
                keyValues: new object[] { 2, 1 },
                column: "Quantity",
                value: 3.0);

            migrationBuilder.UpdateData(
                table: "recipe_ingredient",
                keyColumns: new[] { "IngredientId", "RecipeId" },
                keyValues: new object[] { 2, 7 },
                column: "Quantity",
                value: 2.0);

            migrationBuilder.UpdateData(
                table: "recipe_ingredient",
                keyColumns: new[] { "IngredientId", "RecipeId" },
                keyValues: new object[] { 2, 8 },
                column: "Quantity",
                value: 3.0);
        }
    }
}
