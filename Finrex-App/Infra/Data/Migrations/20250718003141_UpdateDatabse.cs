using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Finrex_App.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDatabse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Mes",
                table: "MSpending");

            migrationBuilder.DropColumn(
                name: "Mes",
                table: "MIncome");

            migrationBuilder.AddColumn<DateTime>(
                name: "Date",
                table: "MSpending",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "Date",
                table: "MIncome",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Date",
                table: "MSpending");

            migrationBuilder.DropColumn(
                name: "Date",
                table: "MIncome");

            migrationBuilder.AddColumn<int>(
                name: "Mes",
                table: "MSpending",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Mes",
                table: "MIncome",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
