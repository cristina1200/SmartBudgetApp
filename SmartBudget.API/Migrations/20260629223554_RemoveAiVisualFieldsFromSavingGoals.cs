using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartBudget.Server.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAiVisualFieldsFromSavingGoals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AiImageDarkDataUrl",
                table: "SavingGoals");

            migrationBuilder.DropColumn(
                name: "AiImageGeneratedAt",
                table: "SavingGoals");

            migrationBuilder.DropColumn(
                name: "AiImageLightDataUrl",
                table: "SavingGoals");

            migrationBuilder.DropColumn(
                name: "AiImagePrompt",
                table: "SavingGoals");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AiImageDarkDataUrl",
                table: "SavingGoals",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AiImageGeneratedAt",
                table: "SavingGoals",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AiImageLightDataUrl",
                table: "SavingGoals",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AiImagePrompt",
                table: "SavingGoals",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
