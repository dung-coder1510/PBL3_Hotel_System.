namespace PBL3_Hotel_System.ViewModels
{
    public class BookingDetailViewModel
    {
        public int BookingID { get; set; }

        // Thông tin Khách
        public string TenKhachHang { get; set; }
        public string SoDienThoai { get; set; }
        public string CCCD { get; set; }
        public string DiaChi { get; set; }

        // Thông tin Phòng
        public int SoPhong { get; set; }
        public string LoaiPhong { get; set; }
        public decimal GiaMotDem { get; set; }

        // Lịch trình & Tiền
        public string NgayDat { get; set; }
        public string CheckIn { get; set; }
        public string CheckOut { get; set; }
        public int SoDem { get; set; }
        public string TongTien { get; set; }
        public string GhiChu { get; set; }

        // Trạng thái
        public string TenTrangThai { get; set; }
        public string CssClassTrangThai { get; set; }

        // Thêm các trường giờ thực tế (Để hiển thị nếu đã check-in)
        public string? RealCheckInFormatted { get; set; }
        public string? RealCheckOutFormatted { get; set; }

        // Thêm 2 trường này để lấy dữ liệu từ Form (Nhân viên nhập)
        public DateTime InputRealCheckIn { get; set; } = DateTime.Now;
        public DateTime InputRealCheckOut { get; set; } = DateTime.Now;

        public DateTime InputGioHen { get; set; } = DateTime.Now; // Mặc định gợi ý 14h
        public bool IsPaid { get; set; }
    }
}
