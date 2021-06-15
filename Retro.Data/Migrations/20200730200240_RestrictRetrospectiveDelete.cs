using Microsoft.EntityFrameworkCore.Migrations;

namespace Retro.Data.Migrations
{
    public partial class RestrictRetrospectiveDelete : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Retrospectives_Teams_TeamId",
                table: "Retrospectives");

            migrationBuilder.AddForeignKey(
                name: "FK_Retrospectives_Teams_TeamId",
                table: "Retrospectives",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Retrospectives_Teams_TeamId",
                table: "Retrospectives");

            migrationBuilder.AddForeignKey(
                name: "FK_Retrospectives_Teams_TeamId",
                table: "Retrospectives",
                column: "TeamId",
                principalTable: "Teams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
