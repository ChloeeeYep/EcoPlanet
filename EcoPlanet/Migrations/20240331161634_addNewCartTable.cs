using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcoPlanet.Migrations
{
    /// <inheritdoc />
    public partial class addNewCartTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CartTable",
                columns: table => new
                {
                    cartId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    userId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartTable", x => x.cartId);
                });

            migrationBuilder.CreateTable(
                name: "CartItemTable",
                columns: table => new
                {
                    cartItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    goodsId = table.Column<int>(type: "int", nullable: false),
                    goodsName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    goodsQuantity = table.Column<int>(type: "int", nullable: false),
                    goodsPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    cartId = table.Column<int>(type: "int", nullable: false)
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
                });

            migrationBuilder.CreateIndex(
                name: "IX_CartItemTable_cartId",
                table: "CartItemTable",
                column: "cartId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CartItemTable");

            migrationBuilder.DropTable(
                name: "CartTable");
        }
    }
}
