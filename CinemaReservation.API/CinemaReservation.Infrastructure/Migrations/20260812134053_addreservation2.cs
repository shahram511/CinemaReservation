using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CinemaReservation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addreservation2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReservationSeats_Seats_SeatId",
                table: "ReservationSeats");

            migrationBuilder.RenameColumn(
                name: "ReservationDate",
                table: "Reservations",
                newName: "CreatedAt");

            migrationBuilder.AddColumn<Guid>(
                name: "ID",
                table: "ReservationSeats",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "ReservationSeats",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalPrice",
                table: "Reservations",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddForeignKey(
                name: "FK_ReservationSeats_Seats_SeatId",
                table: "ReservationSeats",
                column: "SeatId",
                principalTable: "Seats",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReservationSeats_Seats_SeatId",
                table: "ReservationSeats");

            migrationBuilder.DropColumn(
                name: "ID",
                table: "ReservationSeats");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "ReservationSeats");

            migrationBuilder.DropColumn(
                name: "TotalPrice",
                table: "Reservations");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Reservations",
                newName: "ReservationDate");

            migrationBuilder.AddForeignKey(
                name: "FK_ReservationSeats_Seats_SeatId",
                table: "ReservationSeats",
                column: "SeatId",
                principalTable: "Seats",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
