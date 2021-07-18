using Microsoft.EntityFrameworkCore.Migrations;

namespace Event_Creator.Migrations
{
    public partial class pornhub12 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "BookName",
                table: "exchanges",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "books",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_books_UserId",
                table: "books",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_books_Users_UserId",
                table: "books",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_books_Users_UserId",
                table: "books");

            migrationBuilder.DropIndex(
                name: "IX_books_UserId",
                table: "books");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "books");

            migrationBuilder.AlterColumn<string>(
                name: "BookName",
                table: "exchanges",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
