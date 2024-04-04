using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcoPlanet.Migrations
{
    /// <inheritdoc />
    public partial class addproductstable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "productsImage",
                table: "ProductsOrderItemTable",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_ProductsCartItemTable_productsId",
                table: "ProductsCartItemTable",
                column: "productsId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductsCartItemTable_ProductsTable_productsId",
                table: "ProductsCartItemTable",
                column: "productsId",
                principalTable: "ProductsTable",
                principalColumn: "productsId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductsCartItemTable_ProductsTable_productsId",
                table: "ProductsCartItemTable");

            migrationBuilder.DropIndex(
                name: "IX_ProductsCartItemTable_productsId",
                table: "ProductsCartItemTable");

            migrationBuilder.DropColumn(
                name: "productsImage",
                table: "ProductsOrderItemTable");
        }
    }
}
