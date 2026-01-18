using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SUPFLY.Migrations
{
    /// <inheritdoc />
    public partial class AddReturnSeat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReturnSeatNumber",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReturnSeatNumber",
                table: "Bookings");
        }
    }
}
