using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Flourish.Migrations
{
    /// <inheritdoc />
    public partial class AddGoalLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GoalLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GoalId1 = table.Column<Guid>(type: "uuid", nullable: false),
                    GoalId2 = table.Column<Guid>(type: "uuid", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: true),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoalLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GoalLinks_Goals_GoalId1",
                        column: x => x.GoalId1,
                        principalTable: "Goals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GoalLinks_Goals_GoalId2",
                        column: x => x.GoalId2,
                        principalTable: "Goals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GoalLinks_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GoalLinks_CreatedById",
                table: "GoalLinks",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_GoalLinks_GoalId1_GoalId2",
                table: "GoalLinks",
                columns: new[] { "GoalId1", "GoalId2" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GoalLinks_GoalId2",
                table: "GoalLinks",
                column: "GoalId2");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GoalLinks");
        }
    }
}
