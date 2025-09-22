using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GreaseMonkeyJournal.Migrations
{
    /// <inheritdoc />
    public partial class AddReminderTypeField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Reminders",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "Reminders");
        }
    }
}
