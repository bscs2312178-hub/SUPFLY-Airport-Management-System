using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SUPFLY.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueSeatConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bookings_FlightId",
                table: "Bookings");

            migrationBuilder.AlterColumn<string>(
                name: "SeatNumber",
                table: "Bookings",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_FlightId_SeatNumber",
                table: "Bookings",
                columns: new[] { "FlightId", "SeatNumber" },
                unique: true,
                filter: "[SeatNumber] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bookings_FlightId_SeatNumber",
                table: "Bookings");

            migrationBuilder.AlterColumn<string>(
                name: "SeatNumber",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_FlightId",
                table: "Bookings",
                column: "FlightId");
        }
    }
}
