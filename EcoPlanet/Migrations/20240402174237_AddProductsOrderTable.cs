using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcoPlanet.Migrations
{
    /// <inheritdoc />
    public partial class AddProductsOrderTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.CreateTable(
                name: "ProductsCartTable",
                columns: table => new
                {
                    productsCartId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    userId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductsCartTable", x => x.productsCartId);
                });

            migrationBuilder.CreateTable(
                name: "ProductsOrderTable",
                columns: table => new
                {
                    ProductsOrderId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProductsOrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Contact = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PaymentStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrderStatus = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductsOrderTable", x => x.ProductsOrderId);
                });

            migrationBuilder.CreateTable(
                name: "ProductsCartItemTable",
                columns: table => new
                {
                    productsCartItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    productsId = table.Column<int>(type: "int", nullable: false),
                    productsName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    productsQuantity = table.Column<int>(type: "int", nullable: false),
                    productsPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    productsImage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    productsCartId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductsCartItemTable", x => x.productsCartItemId);
                    table.ForeignKey(
                        name: "FK_ProductsCartItemTable_ProductsCartTable_productsCartId",
                        column: x => x.productsCartId,
                        principalTable: "ProductsCartTable",
                        principalColumn: "productsCartId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductsOrderItemTable",
                columns: table => new
                {
                    ProductsOrderItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    productsId = table.Column<int>(type: "int", nullable: false),
                    productsName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    productsQuantity = table.Column<int>(type: "int", nullable: false),
                    productsPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ProductsOrderId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductsOrderItemTable", x => x.ProductsOrderItemId);
                    table.ForeignKey(
                        name: "FK_ProductsOrderItemTable_ProductsOrderTable_ProductsOrderId",
                        column: x => x.ProductsOrderId,
                        principalTable: "ProductsOrderTable",
                        principalColumn: "ProductsOrderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductsCartItemTable_productsCartId",
                table: "ProductsCartItemTable",
                column: "productsCartId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductsOrderItemTable_ProductsOrderId",
                table: "ProductsOrderItemTable",
                column: "ProductsOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropTable(
                name: "ProductsCartItemTable");

            migrationBuilder.DropTable(
                name: "ProductsOrderItemTable");

            migrationBuilder.DropTable(
                name: "ProductsCartTable");

            migrationBuilder.DropTable(
                name: "ProductsOrderTable");
        }
    }
}
