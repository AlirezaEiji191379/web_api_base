using Microsoft.EntityFrameworkCore.Migrations;

namespace Event_Creator.Migrations
{
    public partial class added_image_count : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "imageCount",
                table: "books",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "imageCount",
                table: "books");
        }
    }
}
