using Microsoft.EntityFrameworkCore.Migrations;

namespace Event_Creator.Migrations
{
    public partial class category_changes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AlterColumn<string>(
                name: "CategoryName",
                table: "categories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "parentName",
                table: "categories",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "publisherName",
                table: "books",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_categories_parentName",
                table: "categories",
                column: "parentName",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_categories_parentName",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "parentName",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "publisherName",
                table: "books");

            migrationBuilder.AlterColumn<string>(
                name: "CategoryName",
                table: "categories",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

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
    }
}
