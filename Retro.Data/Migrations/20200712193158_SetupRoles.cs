using Microsoft.EntityFrameworkCore.Migrations;
using Retro.Data.Models;

namespace Retro.Data.Migrations
{
    public partial class SetupRoles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RoleId",
                table: "TeamMembers",
                nullable: false,
                defaultValue: TeamMemberRoleIdentifiers.Member);

            migrationBuilder.CreateTable(
                name: "TeamMemberRoles",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: false),
                    CanManageTeam = table.Column<bool>(nullable: false),
                    CanManageRetrospective = table.Column<bool>(nullable: false),
                    CanViewMemberInsights = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamMemberRoles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TeamMembers_RoleId",
                table: "TeamMembers",
                column: "RoleId");

            migrationBuilder.Sql("SET IDENTITY_INSERT dbo.TeamMemberRoles ON;");
            migrationBuilder.Sql(
                $@"
                    INSERT INTO TeamMemberRoles(Id, Name, CanManageTeam, CanManageRetrospective, CanViewMemberInsights) VALUES
                        ({TeamMemberRoleIdentifiers.Member}, 'Member', 0, 0, 0), 
                        ({TeamMemberRoleIdentifiers.Manager}, 'Manager', 0, 0, 1), 
                        ({TeamMemberRoleIdentifiers.ScrumMaster}, 'Scrum master', 0, 1, 1), 
                        ({TeamMemberRoleIdentifiers.Admin}, 'Admin', 1, 1, 1);
                ");
            migrationBuilder.Sql("SET IDENTITY_INSERT dbo.TeamMemberRoles OFF;");

            migrationBuilder.AddForeignKey(
                name: "FK_TeamMembers_TeamMemberRoles_RoleId",
                table: "TeamMembers",
                column: "RoleId",
                principalTable: "TeamMemberRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);


            migrationBuilder.Sql($"UPDATE TeamMembers SET RoleId={TeamMemberRoleIdentifiers.Admin} WHERE IsAdmin=1");
                
            migrationBuilder.DropColumn(
                name: "IsAdmin",
                table: "TeamMembers");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAdmin",
                table: "TeamMembers",
                type: "bit",
                nullable: false,
                defaultValue: false);
            
            migrationBuilder.Sql($"UPDATE TeamMembers SET IsAdmin=1 WHERE RoleId={TeamMemberRoleIdentifiers.Admin}");
            
            migrationBuilder.DropForeignKey(
                name: "FK_TeamMembers_TeamMemberRoles_RoleId",
                table: "TeamMembers");

            migrationBuilder.DropTable(
                name: "TeamMemberRoles");

            migrationBuilder.DropIndex(
                name: "IX_TeamMembers_RoleId",
                table: "TeamMembers");

            migrationBuilder.DropColumn(
                name: "RoleId",
                table: "TeamMembers");
        }
    }
}
