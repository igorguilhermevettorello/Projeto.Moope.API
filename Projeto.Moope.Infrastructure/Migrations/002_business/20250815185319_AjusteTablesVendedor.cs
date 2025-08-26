using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Projeto.Moope.Infrastructure.Migrations._002_business
{
    /// <inheritdoc />
    public partial class AjusteTablesVendedor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pedido_Revendedor_RevendedorId",
                table: "Pedido");

            migrationBuilder.DropTable(
                name: "Revendedor");

            migrationBuilder.DropIndex(
                name: "IX_Pedido_RevendedorId",
                table: "Pedido");

            migrationBuilder.AddColumn<Guid>(
                name: "VendedorId",
                table: "Pedido",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.CreateTable(
                name: "Vendedor",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PercentualComissao = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Updated = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    RevendedorId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    VendedorPaiId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vendedor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vendedor_Vendedor_VendedorPaiId",
                        column: x => x.VendedorPaiId,
                        principalTable: "Vendedor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Pedido_VendedorId",
                table: "Pedido",
                column: "VendedorId");

            migrationBuilder.CreateIndex(
                name: "IX_Vendedor_VendedorPaiId",
                table: "Vendedor",
                column: "VendedorPaiId");

            migrationBuilder.AddForeignKey(
                name: "FK_Pedido_Vendedor_VendedorId",
                table: "Pedido",
                column: "VendedorId",
                principalTable: "Vendedor",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pedido_Vendedor_VendedorId",
                table: "Pedido");

            migrationBuilder.DropTable(
                name: "Vendedor");

            migrationBuilder.DropIndex(
                name: "IX_Pedido_VendedorId",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "VendedorId",
                table: "Pedido");

            migrationBuilder.CreateTable(
                name: "Revendedor",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    RevendedorId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    UsuarioId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Created = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    PercentualComissao = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Updated = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Revendedor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Revendedor_Revendedor_RevendedorId",
                        column: x => x.RevendedorId,
                        principalTable: "Revendedor",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Revendedor_Usuario_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Pedido_RevendedorId",
                table: "Pedido",
                column: "RevendedorId");

            migrationBuilder.CreateIndex(
                name: "IX_Revendedor_RevendedorId",
                table: "Revendedor",
                column: "RevendedorId");

            migrationBuilder.CreateIndex(
                name: "IX_Revendedor_UsuarioId",
                table: "Revendedor",
                column: "UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Pedido_Revendedor_RevendedorId",
                table: "Pedido",
                column: "RevendedorId",
                principalTable: "Revendedor",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
