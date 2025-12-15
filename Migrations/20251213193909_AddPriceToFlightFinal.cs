using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SUPFLY.Migrations
{
    /// <inheritdoc />
    public partial class AddPriceToFlightFinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Destination",
                table: "Flights",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Origin",
                table: "Flights",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Flights",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Destination",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "Origin",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Flights");
        }
    }
}
