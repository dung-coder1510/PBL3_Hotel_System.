using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PBL3_Hotel_System_.Migrations
{
    /// <inheritdoc />
    public partial class BoonkingUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GhiChu",
                table: "Booking",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GhiChu",
                table: "Booking");
        }
    }
}
