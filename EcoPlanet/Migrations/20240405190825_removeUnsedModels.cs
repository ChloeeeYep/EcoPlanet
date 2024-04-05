using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcoPlanet.Migrations
{
    /// <inheritdoc />
    public partial class removeUnsedModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CartItemTable");

            migrationBuilder.DropTable(
                name: "OrderItemTable");

            migrationBuilder.DropTable(
                name: "CartTable");

            migrationBuilder.DropTable(
                name: "GoodsTable");

            migrationBuilder.DropTable(
                name: "OrderTable");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CartTable",
                columns: table => new
                {
                    cartId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    userId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartTable", x => x.cartId);
                });

            migrationBuilder.CreateTable(
                name: "GoodsTable",
                columns: table => new
                {
                    goodsId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SellerId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    goodsDescriptions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    goodsExpiry = table.Column<DateTime>(type: "datetime2", nullable: false),
                    goodsImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    goodsName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    goodsPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    goodsQuantity = table.Column<int>(type: "int", nullable: false),
                    goodsStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    goodsType = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoodsTable", x => x.goodsId);
                });

            migrationBuilder.CreateTable(
                name: "OrderTable",
                columns: table => new
                {
                    OrderId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Contact = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DriverId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OrderStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PaymentStatus = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderTable", x => x.OrderId);
                });

            migrationBuilder.CreateTable(
                name: "CartItemTable",
                columns: table => new
                {
                    cartItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    cartId = table.Column<int>(type: "int", nullable: false),
                    goodsId = table.Column<int>(type: "int", nullable: false),
                    goodsImage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    goodsName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    goodsPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    goodsQuantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartItemTable", x => x.cartItemId);
                    table.ForeignKey(
                        name: "FK_CartItemTable_CartTable_cartId",
                        column: x => x.cartId,
                        principalTable: "CartTable",
                        principalColumn: "cartId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CartItemTable_GoodsTable_goodsId",
                        column: x => x.goodsId,
                        principalTable: "GoodsTable",
                        principalColumn: "goodsId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderItemTable",
                columns: table => new
                {
                    OrderItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    SellerId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    goodsId = table.Column<int>(type: "int", nullable: false),
                    goodsImage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    goodsName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    goodsPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    goodsQuantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItemTable", x => x.OrderItemId);
                    table.ForeignKey(
                        name: "FK_OrderItemTable_OrderTable_OrderId",
                        column: x => x.OrderId,
                        principalTable: "OrderTable",
                        principalColumn: "OrderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CartItemTable_cartId",
                table: "CartItemTable",
                column: "cartId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItemTable_goodsId",
                table: "CartItemTable",
                column: "goodsId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemTable_OrderId",
                table: "OrderItemTable",
                column: "OrderId");
        }
    }
}
