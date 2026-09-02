using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CinemaReservation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailUniqueAndReservationSeatShowtimeFk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ReservationSeats_Showtimes_ShowtimeId",
                table: "ReservationSeats",
                column: "ShowtimeId",
                principalTable: "Showtimes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReservationSeats_Showtimes_ShowtimeId",
                table: "ReservationSeats");

            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");
        }
    }
}
