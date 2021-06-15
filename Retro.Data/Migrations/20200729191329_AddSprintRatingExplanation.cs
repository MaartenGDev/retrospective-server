using Microsoft.EntityFrameworkCore.Migrations;

namespace Retro.Data.Migrations
{
    public partial class AddSprintRatingExplanation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SprintRatingExplanation",
                table: "Evaluations",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SprintRatingExplanation",
                table: "Evaluations");
        }
    }
}
