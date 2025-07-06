using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Finrex_App.Data.Migrations
{
    /// <inheritdoc />
    public partial class CreatingMonthlySpellingTablesAndMonthlyIncome : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Nome",
                table: "Users",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.CreateTable(
                name: "MIncome",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    Mes = table.Column<int>(type: "integer", nullable: false),
                    MainIncome = table.Column<decimal>(type: "numeric", nullable: false),
                    Freelance = table.Column<decimal>(type: "numeric", nullable: false),
                    Benefits = table.Column<decimal>(type: "numeric", nullable: false),
                    BussinesProfit = table.Column<decimal>(type: "numeric", nullable: false),
                    Other = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MIncome", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MIncome_Users_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MSpending",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    Mes = table.Column<int>(type: "integer", nullable: false),
                    Transportation = table.Column<decimal>(type: "numeric", nullable: false),
                    Entertainment = table.Column<decimal>(type: "numeric", nullable: false),
                    Rent = table.Column<decimal>(type: "numeric", nullable: false),
                    Groceries = table.Column<decimal>(type: "numeric", nullable: false),
                    Utilities = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MSpending", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MSpending_Users_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MIncome_UsuarioId",
                table: "MIncome",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_MSpending_UsuarioId",
                table: "MSpending",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MIncome");

            migrationBuilder.DropTable(
                name: "MSpending");

            migrationBuilder.AlterColumn<string>(
                name: "Nome",
                table: "Users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
