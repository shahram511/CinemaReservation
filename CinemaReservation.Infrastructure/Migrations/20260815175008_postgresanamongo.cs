using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CinemaReservation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class postgresanamongo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("a1b2c3d4-e5f6-7a8b-9c0d-123456789abc"),
                columns: new[] { "PasswordHash", "Username" },
                values: new object[] { "$2a$11$BUGE2c3vcSEvBH.TFk0Ic.OEu2wH8uBWvWWL.KDz1IHZ6PkFPhamy ", "shahram" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("a1b2c3d4-e5f6-7a8b-9c0d-123456789abc"),
                columns: new[] { "PasswordHash", "Username" },
                values: new object[] { "$2a$11$YourHashedPasswordHere", "" });
        }
    }
}
