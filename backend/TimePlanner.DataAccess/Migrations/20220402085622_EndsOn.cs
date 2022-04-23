using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimePlanner.DataAccess.Migrations
{
    public partial class EndsOn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RepetitionCount",
                table: "WorkItems",
                newName: "RepetitionsCount");

            migrationBuilder.RenameColumn(
                name: "RecurrenceStartsFrom",
                table: "WorkItems",
                newName: "RecurrenceStartsOn");

            migrationBuilder.RenameColumn(
                name: "MaxRepetitionCount",
                table: "WorkItems",
                newName: "MaxRepetitionsCount");

            migrationBuilder.RenameColumn(
                name: "IsAfterPreviousCompleted",
                table: "WorkItems",
                newName: "IsOnPause");

            migrationBuilder.AddColumn<bool>(
                name: "IsIfPreviousCompleted",
                table: "WorkItems",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RecurrenceEndsOn",
                table: "WorkItems",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsIfPreviousCompleted",
                table: "WorkItems");

            migrationBuilder.DropColumn(
                name: "RecurrenceEndsOn",
                table: "WorkItems");

            migrationBuilder.RenameColumn(
                name: "RepetitionsCount",
                table: "WorkItems",
                newName: "RepetitionCount");

            migrationBuilder.RenameColumn(
                name: "RecurrenceStartsOn",
                table: "WorkItems",
                newName: "RecurrenceStartsFrom");

            migrationBuilder.RenameColumn(
                name: "MaxRepetitionsCount",
                table: "WorkItems",
                newName: "MaxRepetitionCount");

            migrationBuilder.RenameColumn(
                name: "IsOnPause",
                table: "WorkItems",
                newName: "IsAfterPreviousCompleted");
        }
    }
}
