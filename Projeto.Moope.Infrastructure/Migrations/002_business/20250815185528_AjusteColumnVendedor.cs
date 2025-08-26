using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Projeto.Moope.Infrastructure.Migrations._002_business
{
    /// <inheritdoc />
    public partial class AjusteColumnVendedor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vendedor_Vendedor_VendedorPaiId",
                table: "Vendedor");

            migrationBuilder.DropIndex(
                name: "IX_Vendedor_VendedorPaiId",
                table: "Vendedor");

            migrationBuilder.DropColumn(
                name: "VendedorPaiId",
                table: "Vendedor");

            migrationBuilder.RenameColumn(
                name: "RevendedorId",
                table: "Vendedor",
                newName: "VendedorId");

            migrationBuilder.CreateIndex(
                name: "IX_Vendedor_VendedorId",
                table: "Vendedor",
                column: "VendedorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Vendedor_Vendedor_VendedorId",
                table: "Vendedor",
                column: "VendedorId",
                principalTable: "Vendedor",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vendedor_Vendedor_VendedorId",
                table: "Vendedor");

            migrationBuilder.DropIndex(
                name: "IX_Vendedor_VendedorId",
                table: "Vendedor");

            migrationBuilder.RenameColumn(
                name: "VendedorId",
                table: "Vendedor",
                newName: "RevendedorId");

            migrationBuilder.AddColumn<Guid>(
                name: "VendedorPaiId",
                table: "Vendedor",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_Vendedor_VendedorPaiId",
                table: "Vendedor",
                column: "VendedorPaiId");

            migrationBuilder.AddForeignKey(
                name: "FK_Vendedor_Vendedor_VendedorPaiId",
                table: "Vendedor",
                column: "VendedorPaiId",
                principalTable: "Vendedor",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
