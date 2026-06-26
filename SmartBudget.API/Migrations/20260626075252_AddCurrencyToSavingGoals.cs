using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartBudget.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddCurrencyToSavingGoals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Currency",
                table: "SavingGoals",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Currency",
                table: "SavingGoals");
        }
    }
}
