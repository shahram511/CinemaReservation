using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CinemaReservation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class new1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("a1b2c3d4-e5f6-7a8b-9c0d-123456789abc"),
                column: "PasswordHash",
                value: "$2a$12$hlNKXUY.A18uzOvU6UcXlujuHZA.vEEILXiQRWVBVErfNr0dMa8jG");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("a1b2c3d4-e5f6-7a8b-9c0d-123456789abc"),
                column: "PasswordHash",
                value: "$2a$11$BUGE2c3vcSEvBH.TFk0Ic.OEu2wH8uBWvWWL.KDz1IHZ6PkFPhamy ");
        }
    }
}
