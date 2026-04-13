using System.ComponentModel.DataAnnotations;

namespace PBL3_Hotel_System.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập Username hoặc Email")]
        public string UsernameOrEmail { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}