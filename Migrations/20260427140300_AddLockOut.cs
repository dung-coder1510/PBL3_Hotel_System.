using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PBL3_Hotel_System_.Migrations
{
    /// <inheritdoc />
    public partial class AddLockOut : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsLocked",
                table: "Accounts",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsLocked",
                table: "Accounts");
        }
    }
}
