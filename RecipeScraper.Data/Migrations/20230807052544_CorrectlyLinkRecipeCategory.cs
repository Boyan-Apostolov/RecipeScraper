using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecipeScraper.Data.Migrations
{
    /// <inheritdoc />
    public partial class CorrectlyLinkRecipeCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductRecipes_RecipeCategories_RecipeCategoryId",
                table: "ProductRecipes");

            migrationBuilder.DropIndex(
                name: "IX_ProductRecipes_RecipeCategoryId",
                table: "ProductRecipes");

            migrationBuilder.DropColumn(
                name: "RecipeCategoryId",
                table: "ProductRecipes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RecipeCategoryId",
                table: "ProductRecipes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ProductRecipes_RecipeCategoryId",
                table: "ProductRecipes",
                column: "RecipeCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductRecipes_RecipeCategories_RecipeCategoryId",
                table: "ProductRecipes",
                column: "RecipeCategoryId",
                principalTable: "RecipeCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
