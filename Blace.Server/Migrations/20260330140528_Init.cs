using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Blace.Server.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Deletes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    DateTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Deletes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "places",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    CreatedTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LastChangeTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Discriminator = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    Canvas = table.Column<byte[]>(type: "bytea", nullable: true),
                    Height = table.Column<int>(type: "integer", nullable: true),
                    Width = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_places", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tiles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    CreatedTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PlaceId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    X = table.Column<int>(type: "integer", nullable: false),
                    Y = table.Column<int>(type: "integer", nullable: false),
                    Color = table.Column<byte>(type: "smallint", nullable: false),
                    PreviousColor = table.Column<byte>(type: "smallint", nullable: false),
                    DeleteId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tiles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tiles_DeleteId",
                table: "Tiles",
                column: "DeleteId");

            migrationBuilder.CreateIndex(
                name: "IX_Tiles_PlaceId",
                table: "Tiles",
                column: "PlaceId");

            migrationBuilder.CreateIndex(
                name: "IX_Tiles_PlaceId_UserId",
                table: "Tiles",
                columns: new[] { "PlaceId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_Tiles_PlaceId_X_Y",
                table: "Tiles",
                columns: new[] { "PlaceId", "X", "Y" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Deletes");

            migrationBuilder.DropTable(
                name: "places");

            migrationBuilder.DropTable(
                name: "Tiles");
        }
    }
}
