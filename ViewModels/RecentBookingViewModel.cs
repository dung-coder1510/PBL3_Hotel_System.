namespace PBL3_Hotel_System.ViewModels
{
    public class RecentBookingViewModel
    {
        public int BookingID { get; set; }
        public int SoPhong { get; set; }
        public string LoaiPhong { get; set; }

        // Đã được format sẵn thành chuỗi cực đẹp từ C#, View không cần format lại
        public string NgayDatFormatted { get; set; }
        public string ThoiGianLuuTru { get; set; }
        public string TongTienFormatted { get; set; }

        // Trạng thái đã được chuyển thành Tiếng Việt và gán Class màu sắc
        public string TenTrangThai { get; set; }
        public string TrangThaiRaw { get; set; } // Ví dụ: "DaXacNhan"
        public DateTime CheckInDate { get; set; } // Dữ liệu DateTime thật để so sánh if
        public DateTime CheckOutDate { get; set; }
        public DateTime? GioHenNhanPhong { get; set; }
        public string CssClassTrangThai { get; set; }
    }
    public class DashboardViewModel
    {
        public string TenKhachHang { get; set; }
        public int TotalBookings { get; set; }
        public decimal TotalPoints { get; set; }
        public string MemberRank { get; set; }
        public List<RecentBookingViewModel> RecentBookings { get; set; } = new List<RecentBookingViewModel>();
    }
}
