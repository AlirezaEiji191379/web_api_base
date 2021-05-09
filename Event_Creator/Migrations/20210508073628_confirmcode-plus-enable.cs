using Microsoft.EntityFrameworkCore.Migrations;

namespace Event_Creator.Migrations
{
    public partial class confirmcodeplusenable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ConfrimCode",
                table: "Users",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Enable",
                table: "Users",
                type: "bit",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConfrimCode",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Enable",
                table: "Users");
        }
    }
}
