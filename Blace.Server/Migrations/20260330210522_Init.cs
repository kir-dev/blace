using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

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
                name: "places",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", maxLength: 450, nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LastChangeTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Discriminator = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    Height = table.Column<int>(type: "integer", nullable: true),
                    Width = table.Column<int>(type: "integer", nullable: true),
                    Canvas = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_places", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Deletes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", maxLength: 450, nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DateTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Deletes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Deletes_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", maxLength: 450, nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DeleteId = table.Column<int>(type: "integer", maxLength: 450, nullable: true),
                    PlaceId = table.Column<int>(type: "integer", maxLength: 450, nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    CreatedTimeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    X = table.Column<int>(type: "integer", nullable: false),
                    Y = table.Column<int>(type: "integer", nullable: false),
                    Color = table.Column<byte>(type: "smallint", nullable: false),
                    PreviousColor = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tiles_Deletes_DeleteId",
                        column: x => x.DeleteId,
                        principalTable: "Deletes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Tiles_places_PlaceId",
                        column: x => x.PlaceId,
                        principalTable: "places",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Deletes_UserId",
                table: "Deletes",
                column: "UserId");

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
                name: "Tiles");

            migrationBuilder.DropTable(
                name: "Deletes");

            migrationBuilder.DropTable(
                name: "places");

            migrationBuilder.DropTable(
                name: "User");
        }
    }
}
