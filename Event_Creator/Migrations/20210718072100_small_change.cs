using Microsoft.EntityFrameworkCore.Migrations;

namespace Event_Creator.Migrations
{
    public partial class small_change : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_categories_parentName",
                table: "categories");

            migrationBuilder.RenameColumn(
                name: "parentName",
                table: "categories",
                newName: "ParentName");

            migrationBuilder.AlterColumn<string>(
                name: "ParentName",
                table: "categories",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_categories_ParentName",
                table: "categories",
                column: "ParentName",
                unique: true,
                filter: "[ParentName] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_categories_ParentName",
                table: "categories");

            migrationBuilder.RenameColumn(
                name: "ParentName",
                table: "categories",
                newName: "parentName");

            migrationBuilder.AlterColumn<string>(
                name: "parentName",
                table: "categories",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_categories_parentName",
                table: "categories",
                column: "parentName",
                unique: true);
        }
    }
}
