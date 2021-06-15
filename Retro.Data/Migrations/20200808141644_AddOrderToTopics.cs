using Microsoft.EntityFrameworkCore.Migrations;

namespace Retro.Data.Migrations
{
    public partial class AddOrderToTopics : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "Topics",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Order",
                table: "Topics");
        }
    }
}
