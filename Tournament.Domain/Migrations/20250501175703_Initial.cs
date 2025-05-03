using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Tournament.Domain.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tournaments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    GameType = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tournaments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RealName = table.Column<string>(type: "text", nullable: false),
                    UniqueName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Country = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Points = table.Column<long>(type: "bigint", nullable: false),
                    RegisteredOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TournamentEvents",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EventRefId = table.Column<Guid>(type: "uuid", nullable: false),
                    TournamentId = table.Column<long>(type: "bigint", nullable: false),
                    AccountId = table.Column<long>(type: "bigint", nullable: false),
                    EntityType = table.Column<int>(type: "integer", nullable: false),
                    ActionType = table.Column<int>(type: "integer", nullable: false),
                    StakeAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    WinAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    Odd = table.Column<decimal>(type: "numeric", nullable: true),
                    MetaData = table.Column<string>(type: "text", nullable: false),
                    IsQualifying = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DateAdded = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TournamentEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TournamentEvents_Tournaments_TournamentId",
                        column: x => x.TournamentId,
                        principalTable: "Tournaments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TournamentParticipationRules",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TournamentId = table.Column<long>(type: "bigint", nullable: false),
                    MinBetAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    MinOdd = table.Column<decimal>(type: "numeric", nullable: true),
                    MinSpinsCount = table.Column<int>(type: "integer", nullable: true),
                    MinEvents = table.Column<int>(type: "integer", nullable: true),
                    GameCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TournamentParticipationRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TournamentParticipationRules_Tournaments_TournamentId",
                        column: x => x.TournamentId,
                        principalTable: "Tournaments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TournamentPrizes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TournamentId = table.Column<long>(type: "bigint", nullable: false),
                    RankFrom = table.Column<int>(type: "integer", nullable: false),
                    RankTo = table.Column<int>(type: "integer", nullable: false),
                    PrizeAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    PrizeType = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TournamentPrizes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TournamentPrizes_Tournaments_TournamentId",
                        column: x => x.TournamentId,
                        principalTable: "Tournaments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TournamentParticipants",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TournamentId = table.Column<long>(type: "bigint", nullable: false),
                    AccountId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    JoinedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EligibilityPassed = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TournamentParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TournamentParticipants_Tournaments_TournamentId",
                        column: x => x.TournamentId,
                        principalTable: "Tournaments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TournamentParticipants_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TournamentPoints",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TournamentId = table.Column<long>(type: "bigint", nullable: false),
                    AccountId = table.Column<long>(type: "bigint", nullable: false),
                    EventId = table.Column<long>(type: "bigint", nullable: false),
                    Points = table.Column<decimal>(type: "numeric", nullable: false),
                    Multiplier = table.Column<decimal>(type: "numeric", nullable: false),
                    Reason = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TournamentPoints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TournamentPoints_TournamentEvents_EventId",
                        column: x => x.EventId,
                        principalTable: "TournamentEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TournamentPoints_Tournaments_TournamentId",
                        column: x => x.TournamentId,
                        principalTable: "Tournaments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LeaderboardEntries",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TournamentId = table.Column<long>(type: "bigint", nullable: false),
                    AccountId = table.Column<long>(type: "bigint", nullable: false),
                    ParticipantId = table.Column<long>(type: "bigint", nullable: false),
                    TotalStake = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalWin = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalPoints = table.Column<decimal>(type: "numeric", nullable: false),
                    BestMultiplier = table.Column<decimal>(type: "numeric", nullable: false),
                    Rank = table.Column<string>(type: "text", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaderboardEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeaderboardEntries_TournamentParticipants_ParticipantId",
                        column: x => x.ParticipantId,
                        principalTable: "TournamentParticipants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LeaderboardEntries_Tournaments_TournamentId",
                        column: x => x.TournamentId,
                        principalTable: "Tournaments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LeaderboardEntries_ParticipantId",
                table: "LeaderboardEntries",
                column: "ParticipantId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaderboardEntries_TournamentId_AccountId",
                table: "LeaderboardEntries",
                columns: new[] { "TournamentId", "AccountId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TournamentEvents_TournamentId",
                table: "TournamentEvents",
                column: "TournamentId");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentParticipants_TournamentId",
                table: "TournamentParticipants",
                column: "TournamentId");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentParticipants_UserId",
                table: "TournamentParticipants",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentParticipationRules_TournamentId",
                table: "TournamentParticipationRules",
                column: "TournamentId");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentPoints_EventId",
                table: "TournamentPoints",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentPoints_TournamentId",
                table: "TournamentPoints",
                column: "TournamentId");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentPrizes_TournamentId",
                table: "TournamentPrizes",
                column: "TournamentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LeaderboardEntries");

            migrationBuilder.DropTable(
                name: "TournamentParticipationRules");

            migrationBuilder.DropTable(
                name: "TournamentPoints");

            migrationBuilder.DropTable(
                name: "TournamentPrizes");

            migrationBuilder.DropTable(
                name: "TournamentParticipants");

            migrationBuilder.DropTable(
                name: "TournamentEvents");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "Tournaments");
        }
    }
}
