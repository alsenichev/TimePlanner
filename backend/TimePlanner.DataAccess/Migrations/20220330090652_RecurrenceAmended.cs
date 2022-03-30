using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimePlanner.DataAccess.Migrations
{
    public partial class RecurrenceAmended : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxRepetitionCount",
                table: "WorkItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RepetitionCount",
                table: "WorkItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WeekDaysCustom",
                table: "WorkItems",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxRepetitionCount",
                table: "WorkItems");

            migrationBuilder.DropColumn(
                name: "RepetitionCount",
                table: "WorkItems");

            migrationBuilder.DropColumn(
                name: "WeekDaysCustom",
                table: "WorkItems");
        }
    }
}
