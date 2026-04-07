using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PantryChef.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserProfileGoals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Age",
                table: "User",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "CurrentWeightKg",
                table: "User",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "HeightCm",
                table: "User",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCalorieGoalManuallySet",
                table: "User",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "TargetWeightKg",
                table: "User",
                type: "double precision",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Age", "CurrentWeightKg", "HeightCm", "TargetWeightKg" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Age", "CurrentWeightKg", "HeightCm", "TargetWeightKg" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "User",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Age", "CurrentWeightKg", "HeightCm", "TargetWeightKg" },
                values: new object[] { null, null, null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Age",
                table: "User");

            migrationBuilder.DropColumn(
                name: "CurrentWeightKg",
                table: "User");

            migrationBuilder.DropColumn(
                name: "HeightCm",
                table: "User");

            migrationBuilder.DropColumn(
                name: "IsCalorieGoalManuallySet",
                table: "User");

            migrationBuilder.DropColumn(
                name: "TargetWeightKg",
                table: "User");
        }
    }
}
