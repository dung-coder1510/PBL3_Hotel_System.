using System.ComponentModel.DataAnnotations;

namespace PBL3_Hotel_System.ViewModels
{
    public class AdminNhanVienEditViewModel
    {
        public int UserID { get; set; } // Khóa chính để biết đang sửa ai

        // Các trường được phép sửa (Không dùng [Required] để dùng chung cho việc thêm mới/sửa một phần)
        public string? HoTen { get; set; }

        [RegularExpression(@"^\d{10,11}$", ErrorMessage = "SĐT không hợp lệ")]
        public string? SoDienThoai { get; set; }

        public string? DiaChi { get; set; }
        public string? ChucVu { get; set; }

        // Không dùng decimal trực tiếp vì có thể admin để trống lúc sửa
        public string LuongTuanNay { get; set; } = "0 ₫";
        public string LuongThangNay { get; set; } = "0 ₫";
        // Thông tin Readonly (Chỉ xem)
        public string? Username { get; set; }
    }
}
