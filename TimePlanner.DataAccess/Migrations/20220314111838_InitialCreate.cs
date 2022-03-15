using System;
using Microsoft.EntityFrameworkCore.Migrations;

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
                    StatusID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BreakStartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Deposit = table.Column<TimeSpan>(type: "time", nullable: false),
                    Pause = table.Column<TimeSpan>(type: "time", nullable: false),
                    UndistributedTime = table.Column<TimeSpan>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Statuses", x => x.StatusID);
                });

            migrationBuilder.CreateTable(
                name: "WorkItems",
                columns: table => new
                {
                    WorkItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Duration = table.Column<TimeSpan>(type: "time", nullable: false),
                    StatusEntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkItems", x => x.WorkItemId);
                    table.ForeignKey(
                        name: "FK_WorkItems_Statuses_StatusEntityId",
                        column: x => x.StatusEntityId,
                        principalTable: "Statuses",
                        principalColumn: "StatusID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkItems_StatusEntityId",
                table: "WorkItems",
                column: "StatusEntityId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkItems");

            migrationBuilder.DropTable(
                name: "Statuses");
        }
    }
}
