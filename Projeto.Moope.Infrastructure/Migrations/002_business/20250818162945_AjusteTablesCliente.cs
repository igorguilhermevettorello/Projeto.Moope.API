using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Projeto.Moope.Infrastructure.Migrations._002_business
{
    /// <inheritdoc />
    public partial class AjusteTablesCliente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "VendedorId",
                table: "Cliente",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_Cliente_VendedorId",
                table: "Cliente",
                column: "VendedorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Cliente_Vendedor_VendedorId",
                table: "Cliente",
                column: "VendedorId",
                principalTable: "Vendedor",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cliente_Vendedor_VendedorId",
                table: "Cliente");

            migrationBuilder.DropIndex(
                name: "IX_Cliente_VendedorId",
                table: "Cliente");

            migrationBuilder.DropColumn(
                name: "VendedorId",
                table: "Cliente");
        }
    }
}
