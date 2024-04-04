using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcoPlanet.Migrations
{
    /// <inheritdoc />
    public partial class addGoodsImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "goodsImage",
                table: "OrderItemTable",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_CartItemTable_goodsId",
                table: "CartItemTable",
                column: "goodsId");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItemTable_GoodsTable_goodsId",
                table: "CartItemTable",
                column: "goodsId",
                principalTable: "GoodsTable",
                principalColumn: "goodsId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItemTable_GoodsTable_goodsId",
                table: "CartItemTable");

            migrationBuilder.DropIndex(
                name: "IX_CartItemTable_goodsId",
                table: "CartItemTable");

            migrationBuilder.DropColumn(
                name: "goodsImage",
                table: "OrderItemTable");
        }
    }
}
