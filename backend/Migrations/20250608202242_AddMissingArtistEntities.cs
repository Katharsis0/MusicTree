using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicTree.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingArtistEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Artists",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Biography = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    OriginCountry = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ActivityYears = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CoverImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    TimeStamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Artists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Clusters",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    TimeStamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clusters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Albums",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ArtistId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ReleaseDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CoverImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DurationSeconds = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Albums", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Albums_Artists_ArtistId",
                        column: x => x.ArtistId,
                        principalTable: "Artists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArtistMembers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ArtistId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    FullName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Instrument = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ActivityPeriod = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    JoinedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtistMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArtistMembers_Artists_ArtistId",
                        column: x => x.ArtistId,
                        principalTable: "Artists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ArtistId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Content = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    AuthorName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comments_Artists_ArtistId",
                        column: x => x.ArtistId,
                        principalTable: "Artists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ArtistId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Venue = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EventDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Events_Artists_ArtistId",
                        column: x => x.ArtistId,
                        principalTable: "Artists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Photos",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ArtistId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Caption = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Photos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Photos_Artists_ArtistId",
                        column: x => x.ArtistId,
                        principalTable: "Artists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Genres",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsSubgenre = table.Column<bool>(type: "boolean", nullable: false),
                    ParentGenreId = table.Column<string>(type: "character varying(128)", nullable: true),
                    ClusterId = table.Column<string>(type: "character varying(128)", nullable: true),
                    Key = table.Column<int>(type: "integer", nullable: false, defaultValue: -1),
                    BpmLower = table.Column<int>(type: "integer", nullable: false),
                    BpmUpper = table.Column<int>(type: "integer", nullable: false),
                    Bpm = table.Column<int>(type: "integer", nullable: false, defaultValue: 120),
                    GenreTipicalMode = table.Column<float>(type: "real", nullable: false),
                    Volume = table.Column<int>(type: "integer", nullable: false, defaultValue: -20),
                    CompasMetric = table.Column<int>(type: "integer", nullable: false, defaultValue: 4),
                    AvrgDuration = table.Column<int>(type: "integer", nullable: false),
                    ColorR = table.Column<int>(type: "integer", nullable: true),
                    ColorG = table.Column<int>(type: "integer", nullable: true),
                    ColorB = table.Column<int>(type: "integer", nullable: true),
                    GenreCreationYear = table.Column<int>(type: "integer", nullable: true),
                    GenreOriginCountry = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    TimeStamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Genres", x => x.Id);
                    table.CheckConstraint("CK_Genres_ColorB", "[ColorB] IS NULL OR ([ColorB] >= 0 AND [ColorB] <= 255)");
                    table.CheckConstraint("CK_Genres_ColorG", "[ColorG] IS NULL OR ([ColorG] >= 0 AND [ColorG] <= 255)");
                    table.CheckConstraint("CK_Genres_ColorR", "[ColorR] IS NULL OR ([ColorR] >= 0 AND [ColorR] <= 255)");
                    table.CheckConstraint("CK_Genres_RGB_AllOrNone", "([ColorR] IS NULL AND [ColorG] IS NULL AND [ColorB] IS NULL) OR ([ColorR] IS NOT NULL AND [ColorG] IS NOT NULL AND [ColorB] IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_Genres_Clusters_ClusterId",
                        column: x => x.ClusterId,
                        principalTable: "Clusters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Genres_Genres_ParentGenreId",
                        column: x => x.ParentGenreId,
                        principalTable: "Genres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ArtistGenre",
                columns: table => new
                {
                    ArtistId = table.Column<string>(type: "character varying(128)", nullable: false),
                    GenreId = table.Column<string>(type: "character varying(128)", nullable: false),
                    InfluenceCoefficient = table.Column<float>(type: "real", nullable: false, defaultValue: 1f),
                    AssociatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtistGenre", x => new { x.ArtistId, x.GenreId });
                    table.CheckConstraint("CK_ArtistGenre_NotSubgenre", "NOT EXISTS (SELECT 1 FROM \"Genres\" WHERE \"Id\" = \"GenreId\" AND \"IsSubgenre\" = true)");
                    table.ForeignKey(
                        name: "FK_ArtistGenre_Artists_ArtistId",
                        column: x => x.ArtistId,
                        principalTable: "Artists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArtistGenre_Genres_GenreId",
                        column: x => x.GenreId,
                        principalTable: "Genres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ArtistSubgenre",
                columns: table => new
                {
                    ArtistId = table.Column<string>(type: "character varying(128)", nullable: false),
                    GenreId = table.Column<string>(type: "character varying(128)", nullable: false),
                    InfluenceCoefficient = table.Column<float>(type: "real", nullable: false, defaultValue: 1f),
                    AssociatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtistSubgenre", x => new { x.ArtistId, x.GenreId });
                    table.CheckConstraint("CK_ArtistSubgenre_IsSubgenre", "EXISTS (SELECT 1 FROM \"Genres\" WHERE \"Id\" = \"GenreId\" AND \"IsSubgenre\" = true)");
                    table.ForeignKey(
                        name: "FK_ArtistSubgenre_Artists_ArtistId",
                        column: x => x.ArtistId,
                        principalTable: "Artists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArtistSubgenre_Genres_GenreId",
                        column: x => x.GenreId,
                        principalTable: "Genres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GenreRelations",
                columns: table => new
                {
                    GenreId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    RelatedGenreId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Influence = table.Column<int>(type: "integer", nullable: false, defaultValue: 5),
                    MGPC = table.Column<float>(type: "real", nullable: false, defaultValue: 0f)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GenreRelations", x => new { x.GenreId, x.RelatedGenreId });
                    table.CheckConstraint("CK_GenreRelations_Influence", "[Influence] >= 1 AND [Influence] <= 10");
                    table.CheckConstraint("CK_GenreRelations_MGPC", "[MGPC] >= 0.0 AND [MGPC] <= 1.0");
                    table.CheckConstraint("CK_GenreRelations_NoSelfReference", "[GenreId] != [RelatedGenreId]");
                    table.ForeignKey(
                        name: "FK_GenreRelations_Genres_GenreId",
                        column: x => x.GenreId,
                        principalTable: "Genres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GenreRelations_Genres_RelatedGenreId",
                        column: x => x.RelatedGenreId,
                        principalTable: "Genres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Albums_ArtistId",
                table: "Albums",
                column: "ArtistId");

            migrationBuilder.CreateIndex(
                name: "IX_Albums_IsActive",
                table: "Albums",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Albums_ReleaseDate",
                table: "Albums",
                column: "ReleaseDate");

            migrationBuilder.CreateIndex(
                name: "IX_ArtistGenre_GenreId",
                table: "ArtistGenre",
                column: "GenreId");

            migrationBuilder.CreateIndex(
                name: "IX_ArtistMembers_ArtistId",
                table: "ArtistMembers",
                column: "ArtistId");

            migrationBuilder.CreateIndex(
                name: "IX_ArtistMembers_IsActive",
                table: "ArtistMembers",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Artists_IsActive",
                table: "Artists",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Artists_Name",
                table: "Artists",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Artists_OriginCountry",
                table: "Artists",
                column: "OriginCountry");

            migrationBuilder.CreateIndex(
                name: "IX_ArtistSubgenre_GenreId",
                table: "ArtistSubgenre",
                column: "GenreId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_ArtistId",
                table: "Comments",
                column: "ArtistId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_CreatedAt",
                table: "Comments",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_IsActive",
                table: "Comments",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Events_ArtistId",
                table: "Events",
                column: "ArtistId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_EventDate",
                table: "Events",
                column: "EventDate");

            migrationBuilder.CreateIndex(
                name: "IX_Events_IsActive",
                table: "Events",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_GenreRelations_RelatedGenreId",
                table: "GenreRelations",
                column: "RelatedGenreId");

            migrationBuilder.CreateIndex(
                name: "IX_Genres_ClusterId",
                table: "Genres",
                column: "ClusterId");

            migrationBuilder.CreateIndex(
                name: "IX_Genres_ParentGenreId",
                table: "Genres",
                column: "ParentGenreId");

            migrationBuilder.CreateIndex(
                name: "IX_Photos_ArtistId",
                table: "Photos",
                column: "ArtistId");

            migrationBuilder.CreateIndex(
                name: "IX_Photos_CreatedAt",
                table: "Photos",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Photos_IsActive",
                table: "Photos",
                column: "IsActive");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Albums");

            migrationBuilder.DropTable(
                name: "ArtistGenre");

            migrationBuilder.DropTable(
                name: "ArtistMembers");

            migrationBuilder.DropTable(
                name: "ArtistSubgenre");

            migrationBuilder.DropTable(
                name: "Comments");

            migrationBuilder.DropTable(
                name: "Events");

            migrationBuilder.DropTable(
                name: "GenreRelations");

            migrationBuilder.DropTable(
                name: "Photos");

            migrationBuilder.DropTable(
                name: "Genres");

            migrationBuilder.DropTable(
                name: "Artists");

            migrationBuilder.DropTable(
                name: "Clusters");
        }
    }
}
