using Microsoft.EntityFrameworkCore.Migrations;

namespace Event_Creator.Migrations
{
    public partial class ip_address_addedd : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ipAddress",
                table: "refreshTokens",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ipAddress",
                table: "refreshTokens");
        }
    }
}
