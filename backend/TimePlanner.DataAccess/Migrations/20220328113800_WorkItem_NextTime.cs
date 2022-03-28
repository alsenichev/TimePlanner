using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimePlanner.DataAccess.Migrations
{
    public partial class WorkItem_NextTime : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WakingUpWhere",
                table: "WorkItems");

            migrationBuilder.RenameColumn(
                name: "WakingUpWhen",
                table: "WorkItems",
                newName: "NextTime");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NextTime",
                table: "WorkItems",
                newName: "WakingUpWhen");

            migrationBuilder.AddColumn<string>(
                name: "WakingUpWhere",
                table: "WorkItems",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
