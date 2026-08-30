using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CinemaReservation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class optemesticconcurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ReservationSeats",
                table: "ReservationSeats");

            migrationBuilder.AlterColumn<Guid>(
                name: "ReservationId",
                table: "ReservationSeats",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "ShowtimeId",
                table: "ReservationSeats",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<byte[]>(
                name: "Version",
                table: "ReservationSeats",
                type: "bytea",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReservationSeats",
                table: "ReservationSeats",
                column: "ID");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationSeats_ReservationId",
                table: "ReservationSeats",
                column: "ReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationSeats_ShowtimeId_SeatId",
                table: "ReservationSeats",
                columns: new[] { "ShowtimeId", "SeatId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ReservationSeats",
                table: "ReservationSeats");

            migrationBuilder.DropIndex(
                name: "IX_ReservationSeats_ReservationId",
                table: "ReservationSeats");

            migrationBuilder.DropIndex(
                name: "IX_ReservationSeats_ShowtimeId_SeatId",
                table: "ReservationSeats");

            migrationBuilder.DropColumn(
                name: "ShowtimeId",
                table: "ReservationSeats");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "ReservationSeats");

            migrationBuilder.AlterColumn<Guid>(
                name: "ReservationId",
                table: "ReservationSeats",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReservationSeats",
                table: "ReservationSeats",
                columns: new[] { "ReservationId", "SeatId" });
        }
    }
}
