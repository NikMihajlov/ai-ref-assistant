using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Flourish.Migrations
{
    /// <inheritdoc />
    public partial class AddGoalDates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "DueDate",
                table: "Goals",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "StartDate",
                table: "Goals",
                type: "date",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "Goals");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Goals");
        }
    }
}
