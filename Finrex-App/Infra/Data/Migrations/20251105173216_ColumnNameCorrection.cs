using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Finrex_App.Data.Migrations
{
    /// <inheritdoc />
    public partial class ColumnNameCorrection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BussinesProfit",
                table: "MIncome",
                newName: "BusinessProfit");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BusinessProfit",
                table: "MIncome",
                newName: "BussinesProfit");
        }
    }
}
