using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PBL3_Hotel_System_.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBooking2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DangKyCaLams_MaNV",
                table: "DangKyCaLams");

            migrationBuilder.AddColumn<DateTime>(
                name: "RealCheckIn",
                table: "Bookings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RealCheckOut",
                table: "Bookings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DangKyCaLams_MaNV_MaCa_NgayLam",
                table: "DangKyCaLams",
                columns: new[] { "MaNV", "MaCa", "NgayLam" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DangKyCaLams_MaNV_MaCa_NgayLam",
                table: "DangKyCaLams");

            migrationBuilder.DropColumn(
                name: "RealCheckIn",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "RealCheckOut",
                table: "Bookings");

            migrationBuilder.CreateIndex(
                name: "IX_DangKyCaLams_MaNV",
                table: "DangKyCaLams",
                column: "MaNV");
        }
    }
}
