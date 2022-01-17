using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Migrations
{
    public partial class BetterWorldServerMappings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Population",
                schema: "Game",
                table: "GameWorlds");

            migrationBuilder.AddColumn<int>(
                name: "CurrentOnlineChars",
                schema: "Game",
                table: "GameWorlds",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxAllowChars",
                schema: "Game",
                table: "GameWorlds",
                type: "integer",
                nullable: false,
                defaultValue: 5000);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentOnlineChars",
                schema: "Game",
                table: "GameWorlds");

            migrationBuilder.DropColumn(
                name: "MaxAllowChars",
                schema: "Game",
                table: "GameWorlds");

            migrationBuilder.AddColumn<byte>(
                name: "Population",
                schema: "Game",
                table: "GameWorlds",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)2);
        }
    }
}
