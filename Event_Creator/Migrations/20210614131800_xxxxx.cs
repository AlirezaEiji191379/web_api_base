using Microsoft.EntityFrameworkCore.Migrations;

namespace Event_Creator.Migrations
{
    public partial class xxxxx : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "UserId",
                table: "changePassword",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_changePassword_UserId",
                table: "changePassword",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_changePassword_Users_UserId",
                table: "changePassword",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_changePassword_Users_UserId",
                table: "changePassword");

            migrationBuilder.DropIndex(
                name: "IX_changePassword_UserId",
                table: "changePassword");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "changePassword");
        }
    }
}
