using Microsoft.EntityFrameworkCore.Migrations;

namespace Event_Creator.Migrations
{
    public partial class pornhub : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "publisherName",
                table: "books",
                newName: "PublisherName");

            migrationBuilder.RenameColumn(
                name: "price",
                table: "books",
                newName: "Price");

            migrationBuilder.RenameColumn(
                name: "bookName",
                table: "books",
                newName: "BookName");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PublisherName",
                table: "books",
                newName: "publisherName");

            migrationBuilder.RenameColumn(
                name: "Price",
                table: "books",
                newName: "price");

            migrationBuilder.RenameColumn(
                name: "BookName",
                table: "books",
                newName: "bookName");
        }
    }
}
