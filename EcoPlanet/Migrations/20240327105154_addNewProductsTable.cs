using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcoPlanet.Migrations
{
    /// <inheritdoc />
    public partial class addNewProductsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProductsTable",
                columns: table => new
                {
                    productsId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    productsType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    productsName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    productsDescriptions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    productsPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    productsQuantity = table.Column<int>(type: "int", nullable: false),
                    productsStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    productsImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    adminId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    createdAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductsTable", x => x.productsId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductsTable");
        }
    }
}
