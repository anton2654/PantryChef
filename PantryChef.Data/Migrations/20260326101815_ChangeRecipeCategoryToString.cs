using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PantryChef.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeRecipeCategoryToString : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "recipe",
                type: "text",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.UpdateData(
                table: "recipe",
                keyColumn: "Id",
                keyValue: 1,
                column: "Category",
                value: "Сніданки");

            migrationBuilder.UpdateData(
                table: "recipe",
                keyColumn: "Id",
                keyValue: 2,
                column: "Category",
                value: "Обіди");

            migrationBuilder.UpdateData(
                table: "recipe",
                keyColumn: "Id",
                keyValue: 3,
                column: "Category",
                value: "Вечері");

            migrationBuilder.UpdateData(
                table: "recipe",
                keyColumn: "Id",
                keyValue: 4,
                column: "Category",
                value: "Сніданки");

            migrationBuilder.UpdateData(
                table: "recipe",
                keyColumn: "Id",
                keyValue: 5,
                column: "Category",
                value: "Вечері");

            migrationBuilder.UpdateData(
                table: "recipe",
                keyColumn: "Id",
                keyValue: 6,
                column: "Category",
                value: "Сніданки");

            migrationBuilder.UpdateData(
                table: "recipe",
                keyColumn: "Id",
                keyValue: 7,
                column: "Category",
                value: "Обіди");

            migrationBuilder.UpdateData(
                table: "recipe",
                keyColumn: "Id",
                keyValue: 8,
                column: "Category",
                value: "Снеки");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Category",
                table: "recipe",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "recipe",
                keyColumn: "Id",
                keyValue: 1,
                column: "Category",
                value: 0);

            migrationBuilder.UpdateData(
                table: "recipe",
                keyColumn: "Id",
                keyValue: 2,
                column: "Category",
                value: 1);

            migrationBuilder.UpdateData(
                table: "recipe",
                keyColumn: "Id",
                keyValue: 3,
                column: "Category",
                value: 2);

            migrationBuilder.UpdateData(
                table: "recipe",
                keyColumn: "Id",
                keyValue: 4,
                column: "Category",
                value: 0);

            migrationBuilder.UpdateData(
                table: "recipe",
                keyColumn: "Id",
                keyValue: 5,
                column: "Category",
                value: 2);

            migrationBuilder.UpdateData(
                table: "recipe",
                keyColumn: "Id",
                keyValue: 6,
                column: "Category",
                value: 0);

            migrationBuilder.UpdateData(
                table: "recipe",
                keyColumn: "Id",
                keyValue: 7,
                column: "Category",
                value: 1);

            migrationBuilder.UpdateData(
                table: "recipe",
                keyColumn: "Id",
                keyValue: 8,
                column: "Category",
                value: 3);
        }
    }
}
