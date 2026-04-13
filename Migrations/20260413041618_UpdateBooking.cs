using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PBL3_Hotel_System_.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Booking_Rooms_SoPhong",
                table: "Booking");

            migrationBuilder.DropForeignKey(
                name: "FK_Booking_UserProfiles_MaKhachHang",
                table: "Booking");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Booking",
                table: "Booking");

            migrationBuilder.RenameTable(
                name: "Booking",
                newName: "Bookings");

            migrationBuilder.RenameIndex(
                name: "IX_Booking_SoPhong",
                table: "Bookings",
                newName: "IX_Bookings_SoPhong");

            migrationBuilder.RenameIndex(
                name: "IX_Booking_MaKhachHang",
                table: "Bookings",
                newName: "IX_Bookings_MaKhachHang");

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayDat",
                table: "Bookings",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK_Bookings",
                table: "Bookings",
                column: "BookingID");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Rooms_SoPhong",
                table: "Bookings",
                column: "SoPhong",
                principalTable: "Rooms",
                principalColumn: "SoPhong",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_UserProfiles_MaKhachHang",
                table: "Bookings",
                column: "MaKhachHang",
                principalTable: "UserProfiles",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Rooms_SoPhong",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_UserProfiles_MaKhachHang",
                table: "Bookings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Bookings",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "NgayDat",
                table: "Bookings");

            migrationBuilder.RenameTable(
                name: "Bookings",
                newName: "Booking");

            migrationBuilder.RenameIndex(
                name: "IX_Bookings_SoPhong",
                table: "Booking",
                newName: "IX_Booking_SoPhong");

            migrationBuilder.RenameIndex(
                name: "IX_Bookings_MaKhachHang",
                table: "Booking",
                newName: "IX_Booking_MaKhachHang");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Booking",
                table: "Booking",
                column: "BookingID");

            migrationBuilder.AddForeignKey(
                name: "FK_Booking_Rooms_SoPhong",
                table: "Booking",
                column: "SoPhong",
                principalTable: "Rooms",
                principalColumn: "SoPhong",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Booking_UserProfiles_MaKhachHang",
                table: "Booking",
                column: "MaKhachHang",
                principalTable: "UserProfiles",
                principalColumn: "UserID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
