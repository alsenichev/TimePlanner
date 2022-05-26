using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TimePlanner.DataAccess.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Statuses",
                columns: table => new
                {
                    StatusId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BreakStartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Deposit = table.Column<TimeSpan>(type: "interval", nullable: false),
                    Pause = table.Column<TimeSpan>(type: "interval", nullable: false),
                    UndistributedTime = table.Column<TimeSpan>(type: "interval", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Statuses", x => x.StatusId);
                });

            migrationBuilder.CreateTable(
                name: "WorkItems",
                columns: table => new
                {
                    WorkItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CronExpression = table.Column<string>(type: "text", nullable: true),
                    RecurrenceStartsOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RecurrenceEndsOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RepetitionCount = table.Column<int>(type: "integer", nullable: true),
                    MaxRepetitionCount = table.Column<int>(type: "integer", nullable: true),
                    IsIfPreviousCompleted = table.Column<bool>(type: "boolean", nullable: true),
                    IsOnPause = table.Column<bool>(type: "boolean", nullable: true),
                    NextTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkItems", x => x.WorkItemId);
                });

            migrationBuilder.CreateTable(
                name: "Durations",
                columns: table => new
                {
                    DurationEntityId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorkItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Value = table.Column<TimeSpan>(type: "interval", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Durations", x => x.DurationEntityId);
                    table.ForeignKey(
                        name: "FK_Durations_WorkItems_WorkItemId",
                        column: x => x.WorkItemId,
                        principalTable: "WorkItems",
                        principalColumn: "WorkItemId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Durations_WorkItemId",
                table: "Durations",
                column: "WorkItemId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Durations");

            migrationBuilder.DropTable(
                name: "Statuses");

            migrationBuilder.DropTable(
                name: "WorkItems");
        }
    }
}
