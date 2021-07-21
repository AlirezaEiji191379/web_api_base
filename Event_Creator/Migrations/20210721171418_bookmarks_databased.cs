using Microsoft.EntityFrameworkCore.Migrations;

namespace Event_Creator.Migrations
{
    public partial class bookmarks_databased : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_bookmarks_Users_UserId",
                table: "bookmarks");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "bookmarks",
                newName: "userId");

            migrationBuilder.RenameIndex(
                name: "IX_bookmarks_UserId",
                table: "bookmarks",
                newName: "IX_bookmarks_userId");

            migrationBuilder.AlterColumn<long>(
                name: "userId",
                table: "bookmarks",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_bookmarks_Users_userId",
                table: "bookmarks",
                column: "userId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_bookmarks_Users_userId",
                table: "bookmarks");

            migrationBuilder.RenameColumn(
                name: "userId",
                table: "bookmarks",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_bookmarks_userId",
                table: "bookmarks",
                newName: "IX_bookmarks_UserId");

            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "bookmarks",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddForeignKey(
                name: "FK_bookmarks_Users_UserId",
                table: "bookmarks",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
