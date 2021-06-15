using Microsoft.EntityFrameworkCore.Migrations;

namespace Retro.Data.Migrations
{
    public partial class DisableCascadeDeleteForEvaluations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Evaluations_Retrospectives_RetrospectiveId",
                table: "Evaluations");

            migrationBuilder.AddForeignKey(
                name: "FK_Evaluations_Retrospectives_RetrospectiveId",
                table: "Evaluations",
                column: "RetrospectiveId",
                principalTable: "Retrospectives",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Evaluations_Retrospectives_RetrospectiveId",
                table: "Evaluations");

            migrationBuilder.AddForeignKey(
                name: "FK_Evaluations_Retrospectives_RetrospectiveId",
                table: "Evaluations",
                column: "RetrospectiveId",
                principalTable: "Retrospectives",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
