using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Finrex_App.Data.Migrations
{
    /// <inheritdoc />
    public partial class AtualizandoNomesDeColunas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Email",
                table: "Users",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "Senha",
                table: "Users",
                newName: "password");

            migrationBuilder.RenameIndex(
                name: "IX_Users_Email",
                table: "Users",
                newName: "IX_Users_email");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "email",
                table: "Users",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "password",
                table: "Users",
                newName: "Senha");

            migrationBuilder.RenameIndex(
                name: "IX_Users_email",
                table: "Users",
                newName: "IX_Users_Email");
        }
    }
}
