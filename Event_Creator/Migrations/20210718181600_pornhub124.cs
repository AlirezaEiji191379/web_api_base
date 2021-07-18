using Microsoft.EntityFrameworkCore.Migrations;

namespace Event_Creator.Migrations
{
    public partial class pornhub124 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_books_Users_UserId",
                table: "books");

            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "books",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_books_Users_UserId",
                table: "books",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_books_Users_UserId",
                table: "books");

            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "books",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddForeignKey(
                name: "FK_books_Users_UserId",
                table: "books",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
