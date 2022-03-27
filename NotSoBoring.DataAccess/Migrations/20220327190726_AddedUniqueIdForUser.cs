using Microsoft.EntityFrameworkCore.Migrations;

namespace NotSoBoring.DataAccess.Migrations
{
    public partial class AddedUniqueIdForUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UniqueId",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UniqueId",
                table: "Users");
        }
    }
}
