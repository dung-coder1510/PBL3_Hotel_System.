using PBL3_Hotel_System.Models;

namespace PBL3_Hotel_System .Helpers
{
    public class StatusDisplay
    {
        public string Text { get; set; }
        public string CssClass { get; set; }
    }

    public static class HotelStatusHelper
    {
        // 1. DỊCH TRẠNG THÁI ĐƠN ĐẶT PHÒNG (BOOKING)
        public static StatusDisplay GetBookingStatus(BookingStatus status)
        {
            return status switch
            {
                BookingStatus.ChoXacNhan => new StatusDisplay { Text = "Chờ duyệt", CssClass = "status-pending" },
                BookingStatus.DaXacNhan => new StatusDisplay { Text = "Đã duyệt", CssClass = "status-success" },
                BookingStatus.SapDen => new StatusDisplay { Text = "Sắp đến", CssClass = "status-warning" },
                BookingStatus.DangO => new StatusDisplay { Text = "Đang ở", CssClass = "status-info" },
                BookingStatus.YeuCauTraPhong => new StatusDisplay { Text = "Đang chờ Lễ tân", CssClass = "status-warning" },
                BookingStatus.QuaHan => new StatusDisplay { Text = "Quá hạn trả", CssClass = "status-danger" },
                BookingStatus.DaHoanThanh => new StatusDisplay { Text = "Hoàn thành", CssClass = "status-finished" },
                BookingStatus.DaHuy => new StatusDisplay { Text = "Đã hủy", CssClass = "status-danger" },
                _ => new StatusDisplay { Text = "Không xác định", CssClass = "status-finished" }
            };
        }

        // 2. DỊCH TRẠNG THÁI PHÒNG VẬT LÝ (ROOM)
        public static StatusDisplay GetRoomStatus(RoomStatus status)
        {
            return status switch
            {
                RoomStatus.Available => new StatusDisplay { Text = "Sẵn sàng", CssClass = "status-success" },
                RoomStatus.Occupied => new StatusDisplay { Text = "Đang ở", CssClass = "status-info" },
                RoomStatus.Cleaning => new StatusDisplay { Text = "Dọn dẹp", CssClass = "status-warning" },
                RoomStatus.Maintenance => new StatusDisplay { Text = "Bảo trì", CssClass = "status-danger" },
                _ => new StatusDisplay { Text = "Không xác định", CssClass = "status-finished" }
            };
        }

        public static StatusDisplay GetShiftStatus(ShiftStatus status)
        {
            return status switch
            {
                ShiftStatus.Pending => new StatusDisplay { Text = "Chờ duyệt", CssClass = "status-pending" },
                ShiftStatus.Approved => new StatusDisplay { Text = "Đã duyệt", CssClass = "status-success" },
                ShiftStatus.Rejected => new StatusDisplay { Text = "Từ chối", CssClass = "status-danger" },
                _ => new StatusDisplay { Text = "N/A", CssClass = "status-finished" }
            };
        }
    }
}
