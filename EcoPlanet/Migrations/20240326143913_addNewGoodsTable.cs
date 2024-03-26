using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcoPlanet.Migrations
{
    /// <inheritdoc />
    public partial class addNewGoodsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.CreateTable(
                name: "GoodsTable",
                columns: table => new
                {
                    goodsId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    goodsType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    goodsName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    goodsDescriptions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    goodsPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    goodsQuantity = table.Column<int>(type: "int", nullable: false),
                    goodsExpiry = table.Column<DateTime>(type: "datetime2", nullable: false),
                    goodsStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    goodsImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SellerId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoodsTable", x => x.goodsId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GoodsTable");

        }
    }
}
