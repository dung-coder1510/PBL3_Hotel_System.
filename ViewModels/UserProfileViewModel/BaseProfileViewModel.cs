using System.ComponentModel.DataAnnotations;

namespace PBL3_Hotel_System.ViewModels.UserProfileViewModel
{
    public class BaseProfileViewModel
    {
        // --- Thông tin chỉ đọc (Không được sửa) ---
        public string Username { get; set; }
        public string Email { get; set; }

        // --- Thông tin cho phép sửa (Inline Editing) ---
        [Required(ErrorMessage = "Họ tên không được để trống")]
        public string HoTen { get; set; }
        [RegularExpression(@"^\d{10,11}$", ErrorMessage = "Số điện thoại không hợp lệ")]
        public string SoDienThoai { get; set; }
        [StringLength(20, ErrorMessage = "CCCD không hợp lệ")]
        public string CCCD { get; set; }

        public string DiaChi { get; set; }
        public string Role { get; set; }
    }
}
