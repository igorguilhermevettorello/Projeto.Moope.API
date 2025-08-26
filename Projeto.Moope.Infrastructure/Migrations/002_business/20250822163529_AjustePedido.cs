using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Projeto.Moope.Infrastructure.Migrations._002_business
{
    /// <inheritdoc />
    public partial class AjustePedido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CodigoPlano",
                table: "Pedido",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "DescricaoPlano",
                table: "Pedido",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<Guid>(
                name: "PlanoId",
                table: "Pedido",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<int>(
                name: "Quantidade",
                table: "Pedido",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorUnitarioPlano",
                table: "Pedido",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_Pedido_PlanoId",
                table: "Pedido",
                column: "PlanoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Pedido_Plano_PlanoId",
                table: "Pedido",
                column: "PlanoId",
                principalTable: "Plano",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pedido_Plano_PlanoId",
                table: "Pedido");

            migrationBuilder.DropIndex(
                name: "IX_Pedido_PlanoId",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "CodigoPlano",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "DescricaoPlano",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "PlanoId",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "Quantidade",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "ValorUnitarioPlano",
                table: "Pedido");
        }
    }
}
