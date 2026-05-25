using PBL3_Hotel_System.Models;
using System.ComponentModel.DataAnnotations;

namespace PBL3_Hotel_System.ViewModels
{
    public class AdminAccountViewModel
    {
        // Thuộc tính này sẽ mang giá trị 0 nếu là Thêm Mới, có giá trị > 0 nếu là Sửa
        public int AccountID { get; set; }

        [Required(ErrorMessage = "Username không được để trống")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        // Chúng ta tạm thời gỡ bỏ [Required] ở đây để dùng chung cho Sửa
        // Lát nữa sẽ dùng logic C# ở Controller để bắt buộc khi Thêm mới
        public string? Password { get; set; }

        [Required(ErrorMessage = "Quyền truy cập là bắt buộc")]
        public UserRole Role { get; set; }

        // Thông tin hiển thị thêm (Chỉ có khi Sửa)
        public string? OwnerName { get; set; }
        public bool IsLocked { get; set; }
    }
}
