using Microsoft.EntityFrameworkCore.Migrations;

namespace Event_Creator.Migrations
{
    public partial class expiration_time_added_to_system : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "expirationTime",
                table: "verifications",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "expirationTime",
                table: "verifications");
        }
    }
}
