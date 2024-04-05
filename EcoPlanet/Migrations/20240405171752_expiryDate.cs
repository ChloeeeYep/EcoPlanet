using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcoPlanet.Migrations
{
    /// <inheritdoc />
    public partial class expiryDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "expiryDate",
                table: "ProductsTable",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "expiryDate",
                table: "ProductsTable");
        }
    }
}
