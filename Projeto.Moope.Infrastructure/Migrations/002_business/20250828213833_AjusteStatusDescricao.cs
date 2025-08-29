using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Projeto.Moope.Infrastructure.Migrations._002_business
{
    /// <inheritdoc />
    public partial class AjusteStatusDescricao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StatusPagamento",
                table: "Transacao",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StatusAssinatura",
                table: "Pedido",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StatusPagamento",
                table: "Transacao");

            migrationBuilder.DropColumn(
                name: "StatusAssinatura",
                table: "Pedido");
        }
    }
}
