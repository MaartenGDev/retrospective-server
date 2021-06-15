using Microsoft.EntityFrameworkCore.Migrations;

namespace Retro.Data.Migrations
{
    public partial class AddTimeUsageCategoryOrder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "TimeUsageCategories",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Order",
                table: "TimeUsageCategories");
        }
    }
}
