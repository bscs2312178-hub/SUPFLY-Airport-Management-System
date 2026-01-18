using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SUPFLY.Migrations
{
    /// <inheritdoc />
    public partial class AddFlightTimesAndRoundTrip : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRoundTrip",
                table: "Bookings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ReturnFlightId",
                table: "Bookings",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_ReturnFlightId",
                table: "Bookings",
                column: "ReturnFlightId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Flights_ReturnFlightId",
                table: "Bookings",
                column: "ReturnFlightId",
                principalTable: "Flights",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Flights_ReturnFlightId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_ReturnFlightId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "IsRoundTrip",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "ReturnFlightId",
                table: "Bookings");
        }
    }
}
