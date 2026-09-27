using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Cinema.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Auditoriums",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Auditoriums", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Movies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    DurationMinutes = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Movies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Seats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AuditoriumId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Row = table.Column<int>(type: "INTEGER", nullable: false),
                    Number = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Seats_Auditoriums_AuditoriumId",
                        column: x => x.AuditoriumId,
                        principalTable: "Auditoriums",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Showtimes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    MovieId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AuditoriumId = table.Column<Guid>(type: "TEXT", nullable: false),
                    StartTime = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Showtimes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Showtimes_Auditoriums_AuditoriumId",
                        column: x => x.AuditoriumId,
                        principalTable: "Auditoriums",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Showtimes_Movies_MovieId",
                        column: x => x.MovieId,
                        principalTable: "Movies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Reservations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ShowtimeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reservations_Showtimes_ShowtimeId",
                        column: x => x.ShowtimeId,
                        principalTable: "Showtimes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ShowtimeSeats",
                columns: table => new
                {
                    ShowtimeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SeatId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    ReservationId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShowtimeSeats", x => new { x.ShowtimeId, x.SeatId });
                    table.ForeignKey(
                        name: "FK_ShowtimeSeats_Reservations_ReservationId",
                        column: x => x.ReservationId,
                        principalTable: "Reservations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShowtimeSeats_Seats_SeatId",
                        column: x => x.SeatId,
                        principalTable: "Seats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShowtimeSeats_Showtimes_ShowtimeId",
                        column: x => x.ShowtimeId,
                        principalTable: "Showtimes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Auditoriums",
                columns: new[] { "Id", "Name" },
                values: new object[] { new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000000000001"), "Hall 1" });

            migrationBuilder.InsertData(
                table: "Movies",
                columns: new[] { "Id", "Category", "DurationMinutes", "Title", "Year" },
                values: new object[,]
                {
                    { new Guid("6d1e2f3a-4b5c-4d6e-8f90-000000000001"), "Sci-Fi", 148, "Inception", 2010 },
                    { new Guid("6d1e2f3a-4b5c-4d6e-8f90-000000000002"), "Crime", 175, "The Godfather", 1972 },
                    { new Guid("6d1e2f3a-4b5c-4d6e-8f90-000000000003"), "Animation", 125, "Spirited Away", 2001 },
                    { new Guid("6d1e2f3a-4b5c-4d6e-8f90-000000000004"), "Comedy", 99, "The Grand Budapest Hotel", 2014 },
                    { new Guid("6d1e2f3a-4b5c-4d6e-8f90-000000000005"), "Action", 120, "Mad Max: Fury Road", 2015 }
                });

            migrationBuilder.InsertData(
                table: "Seats",
                columns: new[] { "Id", "AuditoriumId", "Number", "Row" },
                values: new object[,]
                {
                    { new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000001000001"), new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000000000001"), 1, 1 },
                    { new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000001000002"), new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000000000001"), 2, 1 },
                    { new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000001000003"), new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000000000001"), 3, 1 },
                    { new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000001000004"), new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000000000001"), 4, 1 },
                    { new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000001000005"), new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000000000001"), 5, 1 },
                    { new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000001000006"), new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000000000001"), 6, 1 },
                    { new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000001000007"), new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000000000001"), 7, 1 },
                    { new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000001000008"), new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000000000001"), 8, 1 },
                    { new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000002000001"), new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000000000001"), 1, 2 },
                    { new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000002000002"), new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000000000001"), 2, 2 },
                    { new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000002000003"), new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000000000001"), 3, 2 },
                    { new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000002000004"), new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000000000001"), 4, 2 },
                    { new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000002000005"), new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000000000001"), 5, 2 },
                    { new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000002000006"), new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000000000001"), 6, 2 },
                    { new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000002000007"), new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000000000001"), 7, 2 },
                    { new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000002000008"), new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000000000001"), 8, 2 },
                    { new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000003000001"), new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000000000001"), 1, 3 },
                    { new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000003000002"), new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000000000001"), 2, 3 },
                    { new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000003000003"), new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000000000001"), 3, 3 },
                    { new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000003000004"), new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000000000001"), 4, 3 },
                    { new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000003000005"), new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000000000001"), 5, 3 },
                    { new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000003000006"), new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000000000001"), 6, 3 },
                    { new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000003000007"), new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000000000001"), 7, 3 },
                    { new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000003000008"), new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000000000001"), 8, 3 },
                    { new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000004000001"), new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000000000001"), 1, 4 },
                    { new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000004000002"), new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000000000001"), 2, 4 },
                    { new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000004000003"), new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000000000001"), 3, 4 },
                    { new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000004000004"), new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000000000001"), 4, 4 },
                    { new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000004000005"), new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000000000001"), 5, 4 },
                    { new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000004000006"), new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000000000001"), 6, 4 },
                    { new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000004000007"), new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000000000001"), 7, 4 },
                    { new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000004000008"), new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000000000001"), 8, 4 },
                    { new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000005000001"), new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000000000001"), 1, 5 },
                    { new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000005000002"), new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000000000001"), 2, 5 },
                    { new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000005000003"), new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000000000001"), 3, 5 },
                    { new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000005000004"), new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000000000001"), 4, 5 },
                    { new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000005000005"), new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000000000001"), 5, 5 },
                    { new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000005000006"), new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000000000001"), 6, 5 },
                    { new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000005000007"), new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000000000001"), 7, 5 },
                    { new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000005000008"), new Guid("0d3a1b6e-1c4f-4a2d-9b7e-000000000001"), 8, 5 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_ShowtimeId",
                table: "Reservations",
                column: "ShowtimeId");

            migrationBuilder.CreateIndex(
                name: "IX_Seats_AuditoriumId_Row_Number",
                table: "Seats",
                columns: new[] { "AuditoriumId", "Row", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Showtimes_AuditoriumId_StartTime",
                table: "Showtimes",
                columns: new[] { "AuditoriumId", "StartTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Showtimes_MovieId",
                table: "Showtimes",
                column: "MovieId");

            migrationBuilder.CreateIndex(
                name: "IX_ShowtimeSeats_ReservationId",
                table: "ShowtimeSeats",
                column: "ReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_ShowtimeSeats_SeatId",
                table: "ShowtimeSeats",
                column: "SeatId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShowtimeSeats");

            migrationBuilder.DropTable(
                name: "Reservations");

            migrationBuilder.DropTable(
                name: "Seats");

            migrationBuilder.DropTable(
                name: "Showtimes");

            migrationBuilder.DropTable(
                name: "Auditoriums");

            migrationBuilder.DropTable(
                name: "Movies");
        }
    }
}
