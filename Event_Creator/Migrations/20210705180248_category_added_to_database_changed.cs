using Microsoft.EntityFrameworkCore.Migrations;

namespace Event_Creator.Migrations
{
    public partial class category_added_to_database_changed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "parentCategoryId",
                table: "categories",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_categories_parentCategoryId",
                table: "categories",
                column: "parentCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_categories_categories_parentCategoryId",
                table: "categories",
                column: "parentCategoryId",
                principalTable: "categories",
                principalColumn: "CategoryId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_categories_categories_parentCategoryId",
                table: "categories");

            migrationBuilder.DropIndex(
                name: "IX_categories_parentCategoryId",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "parentCategoryId",
                table: "categories");
        }
    }
}
