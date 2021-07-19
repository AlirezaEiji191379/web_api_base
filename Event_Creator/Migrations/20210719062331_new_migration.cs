using Microsoft.EntityFrameworkCore.Migrations;

namespace Event_Creator.Migrations
{
    public partial class new_migration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_exchanges_books_bookToExchangeBookId",
                table: "exchanges");

            migrationBuilder.DropForeignKey(
                name: "FK_exchanges_Users_UserId",
                table: "exchanges");

            migrationBuilder.DropIndex(
                name: "IX_exchanges_bookToExchangeBookId",
                table: "exchanges");

            migrationBuilder.DropIndex(
                name: "IX_exchanges_UserId",
                table: "exchanges");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "exchanges");

            migrationBuilder.DropColumn(
                name: "bookToExchangeBookId",
                table: "exchanges");

            migrationBuilder.AddColumn<long>(
                name: "bookToExchangeId",
                table: "exchanges",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_exchanges_bookToExchangeId",
                table: "exchanges",
                column: "bookToExchangeId");

            migrationBuilder.AddForeignKey(
                name: "FK_exchanges_books_bookToExchangeId",
                table: "exchanges",
                column: "bookToExchangeId",
                principalTable: "books",
                principalColumn: "BookId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_exchanges_books_bookToExchangeId",
                table: "exchanges");

            migrationBuilder.DropIndex(
                name: "IX_exchanges_bookToExchangeId",
                table: "exchanges");

            migrationBuilder.DropColumn(
                name: "bookToExchangeId",
                table: "exchanges");

            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "exchanges",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "bookToExchangeBookId",
                table: "exchanges",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_exchanges_bookToExchangeBookId",
                table: "exchanges",
                column: "bookToExchangeBookId");

            migrationBuilder.CreateIndex(
                name: "IX_exchanges_UserId",
                table: "exchanges",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_exchanges_books_bookToExchangeBookId",
                table: "exchanges",
                column: "bookToExchangeBookId",
                principalTable: "books",
                principalColumn: "BookId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_exchanges_Users_UserId",
                table: "exchanges",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
