using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Database.Migrations
{
    public partial class CharacterCreation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Appearances",
                schema: "Game",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CharacterId = table.Column<int>(type: "integer", nullable: false),
                    Size = table.Column<byte>(type: "smallint", nullable: false, defaultValue: (byte)0),
                    Voice = table.Column<byte>(type: "smallint", nullable: false, defaultValue: (byte)0),
                    SkinColor = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    HairStyle = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    HairColor = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    HairHighlightColor = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    HairVariation = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    EyeColor = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    CharacteristicsColor = table.Column<byte>(type: "smallint", nullable: false, defaultValue: (byte)0),
                    FaceType = table.Column<byte>(type: "smallint", nullable: false, defaultValue: (byte)0),
                    FaceEyebrows = table.Column<byte>(type: "smallint", nullable: false, defaultValue: (byte)0),
                    FaceEyeShape = table.Column<byte>(type: "smallint", nullable: false, defaultValue: (byte)0),
                    FaceIrisSize = table.Column<byte>(type: "smallint", nullable: false, defaultValue: (byte)0),
                    FaceNose = table.Column<byte>(type: "smallint", nullable: false, defaultValue: (byte)0),
                    FaceMouth = table.Column<byte>(type: "smallint", nullable: false, defaultValue: (byte)0),
                    FaceFeatures = table.Column<byte>(type: "smallint", nullable: false, defaultValue: (byte)0),
                    Characteristics = table.Column<byte>(type: "smallint", nullable: false, defaultValue: (byte)0),
                    Ears = table.Column<byte>(type: "smallint", nullable: false, defaultValue: (byte)0),
                    MainHand = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    OffHand = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Head = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Body = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Legs = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Hands = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Feet = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Waist = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Neck = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    RightEar = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    LeftEar = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    RightIndex = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    LeftIndex = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    RightFinger = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    LeftFinger = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appearances", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Characters",
                schema: "Game",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<short>(type: "smallint", nullable: false),
                    Slot = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    ServerId = table.Column<short>(type: "smallint", nullable: false),
                    Name = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    State = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)2),
                    IsLegacy = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DoRename = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CurrentZoneId = table.Column<int>(type: "integer", nullable: false),
                    Guardian = table.Column<byte>(type: "smallint", nullable: false),
                    BirthMonth = table.Column<byte>(type: "smallint", nullable: false, defaultValue: (byte)1),
                    BirthDay = table.Column<byte>(type: "smallint", nullable: false, defaultValue: (byte)1),
                    InitialTown = table.Column<byte>(type: "smallint", nullable: false),
                    Tribe = table.Column<byte>(type: "smallint", nullable: false, defaultValue: (byte)1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Characters", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Appearances",
                schema: "Game");

            migrationBuilder.DropTable(
                name: "Characters",
                schema: "Game");
        }
    }
}
