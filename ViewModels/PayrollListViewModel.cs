namespace PBL3_Hotel_System.ViewModels
{
    public class PayrollListViewModel
    {
        public int MaNV { get; set; }
        public string TenNV { get; set; }

        public int Thang { get; set; }
        public int Nam { get; set; }

        public int TongSoCa { get; set; }
        public string TongTienFormatted { get; set; } // VD: "8,000,000 ₫"

        // Phục vụ giao diện (Màu sắc và Chữ)
        public string TrangThaiText { get; set; }
        public string CssClassTrangThai { get; set; }

        // Dùng để View dùng lệnh IF rẽ nhánh hiện Nút bấm cho dễ
        // 0 = Chưa chốt | 1 = Chờ Thanh toán | 2 = Đã Thanh toán
        public int StatusCode { get; set; }
    }
}
