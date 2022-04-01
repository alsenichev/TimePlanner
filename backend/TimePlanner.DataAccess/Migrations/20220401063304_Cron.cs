using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimePlanner.DataAccess.Migrations
{
    public partial class Cron : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DaysCustom",
                table: "WorkItems");

            migrationBuilder.DropColumn(
                name: "DaysEveryN",
                table: "WorkItems");

            migrationBuilder.DropColumn(
                name: "IsRecurrent",
                table: "WorkItems");

            migrationBuilder.DropColumn(
                name: "MonthsCustom",
                table: "WorkItems");

            migrationBuilder.DropColumn(
                name: "MonthsEveryN",
                table: "WorkItems");

            migrationBuilder.DropColumn(
                name: "WeekDaysCustom",
                table: "WorkItems");

            migrationBuilder.DropColumn(
                name: "WeeksCustom",
                table: "WorkItems");

            migrationBuilder.DropColumn(
                name: "WeeksEveryN",
                table: "WorkItems");

            migrationBuilder.DropColumn(
                name: "YearsEveryN",
                table: "WorkItems");

            migrationBuilder.RenameColumn(
                name: "YearsCustom",
                table: "WorkItems",
                newName: "CronExpression");

            migrationBuilder.AddColumn<DateTime>(
                name: "RecurrenceStartsFrom",
                table: "WorkItems",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RecurrenceStartsFrom",
                table: "WorkItems");

            migrationBuilder.RenameColumn(
                name: "CronExpression",
                table: "WorkItems",
                newName: "YearsCustom");

            migrationBuilder.AddColumn<string>(
                name: "DaysCustom",
                table: "WorkItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DaysEveryN",
                table: "WorkItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRecurrent",
                table: "WorkItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MonthsCustom",
                table: "WorkItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MonthsEveryN",
                table: "WorkItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WeekDaysCustom",
                table: "WorkItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WeeksCustom",
                table: "WorkItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WeeksEveryN",
                table: "WorkItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "YearsEveryN",
                table: "WorkItems",
                type: "int",
                nullable: true);
        }
    }
}
