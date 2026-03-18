using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Flourish.Migrations
{
    /// <inheritdoc />
    public partial class AddSharedGoals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SharedGoals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReviewPeriodId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SharedGoals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SharedGoals_ReviewPeriods_ReviewPeriodId",
                        column: x => x.ReviewPeriodId,
                        principalTable: "ReviewPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SharedGoals_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SharedGoalMembers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SharedGoalId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SharedGoalMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SharedGoalMembers_SharedGoals_SharedGoalId",
                        column: x => x.SharedGoalId,
                        principalTable: "SharedGoals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SharedGoalMembers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SharedGoalMembers_SharedGoalId_UserId",
                table: "SharedGoalMembers",
                columns: new[] { "SharedGoalId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SharedGoalMembers_UserId",
                table: "SharedGoalMembers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SharedGoals_CreatedById",
                table: "SharedGoals",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_SharedGoals_ReviewPeriodId",
                table: "SharedGoals",
                column: "ReviewPeriodId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SharedGoalMembers");

            migrationBuilder.DropTable(
                name: "SharedGoals");
        }
    }
}
