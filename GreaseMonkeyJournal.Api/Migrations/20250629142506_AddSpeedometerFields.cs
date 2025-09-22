using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GreaseMonkeyJournal.Migrations
{
    /// <inheritdoc />
    public partial class AddSpeedometerFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SpeedometerType",
                table: "Vehicles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "DueSpeedometerReading",
                table: "Reminders",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SpeedometerReading",
                table: "LogEntries",
                type: "decimal(65,30)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SpeedometerType",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "DueSpeedometerReading",
                table: "Reminders");

            migrationBuilder.DropColumn(
                name: "SpeedometerReading",
                table: "LogEntries");
        }
    }
}
