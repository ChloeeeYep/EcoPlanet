using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcoPlanet.Migrations
{
    /// <inheritdoc />
    public partial class addisSubscribedToUsersTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "isSubscribed",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "isSubscribed",
                table: "AspNetUsers");
        }
    }
}
