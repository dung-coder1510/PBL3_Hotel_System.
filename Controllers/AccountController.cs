using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies; //Xu lu cookie (RememberMe và LogOut)
using Microsoft.AspNetCore.Mvc;
using PBL3_Hotel_System.Data;       // Thêm dòng này để gọi được HotelDbContext
using PBL3_Hotel_System.Models;
using PBL3_Hotel_System.ViewModels; // Thêm dòng này để gọi được LoginViewModel
using PBL3_Hotel_System_.Models.UserModels;
using System.Security.Claims; 

namespace PBL3_Hotel_System.Controllers
{
    public class AccountController(HotelDbContext _context) : Controller
    {
        

        // 1. Chức năng Seed Data bằng Controller
        public IActionResult SeedData()
        {
            if (!_context.Accounts.Any())
            {
                _context.Accounts.AddRange(
                    new Account { Username = "admin", Password = "123", Role = UserRole.QuanTriVien, Email = "admin@gmail.com" },
                    new Account { Username = "khach1", Password = "123", Role = UserRole.KhachHang, Email = "khachhang@gmail.com" }
                );
                _context.SaveChanges();
                return Content("Đã Seed dữ liệu mẫu thành công! Bạn có thể dùng admin/123 hoặc khach1/123 để đăng nhập.");
            }
            return Content("Dữ liệu đã tồn tại trong Database, không cần seed thêm.");
        }

        private IActionResult RedirectByUserRole(string? role)
        {
            if (string.IsNullOrEmpty(role))
            {
                return RedirectToAction("Index", "Home");
            }
            switch (role)
            {
                case "QuanTriVien":
                    return RedirectToAction("Index", "Admin");
                case "NhanVien":
                    return RedirectToAction("Index", "NhanVien");
                default:
                    return RedirectToAction("Index", "KhachHang");
            }
        }
        // 2. Hiển thị trang đăng nhập[HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            if (User.Identity.IsAuthenticated)
            {
                var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
                return RedirectByUserRole(role ?? "KhachHang");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // 3. Xử lý POST Đăng nhập bằng LINQ và RememberMe 
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null) 
        {
            if (!ModelState.IsValid) return View(model);

            var user = _context.Accounts.FirstOrDefault(a =>
                (a.Username == model.UsernameOrEmail || a.Email == model.UsernameOrEmail)
                && a.Password == model.Password);

            if (user != null)
            {
                // 1. Tạo "Chứng minh thư" (Claims) lưu thông tin cơ bản của User
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role.ToString()) // Lưu quyền để sau này phân quyền Admin/Khách
                };

                // 2. Tạo Identity từ danh sách Claim
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                // 3. Cấu hình Remember Me
                var authProperties = new AuthenticationProperties
                {
                    // Nếu người dùng tích "Ghi nhớ", Cookie sẽ sống 30 ngày. 
                    // Nếu KHÔNG tích, Cookie sẽ chết ngay khi tắt trình duyệt (Session Cookie).
                    IsPersistent = model.RememberMe,

                    // Tùy chọn: Set thời gian hết hạn cố định
                    ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(30) : null
                };

                // 4. Lệnh phát hành Cookie và Đăng nhập (Lưu vào trình duyệt)
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return LocalRedirect(returnUrl);
                }
                return RedirectByUserRole(user.Role.ToString());
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Tài khoản, Email hoặc mật khẩu không chính xác!");
                return View(model);
            }
        }




        //Đăng ký tài khoản
        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // 1. Kiểm tra trùng lặp Username hoặc Email
                bool isExist = _context.Accounts.Any(a => a.Username == model.Username || a.Email == model.Email);
                if (isExist)
                {
                    TempData["Error"] = "Tên đăng nhập hoặc Email đã tồn tại!";
                    return RedirectToAction("Login"); // Quay lại trang Login (tab Đăng ký sẽ được active bằng JS)
                }
                BaseUser profile;

                // Mặc định đăng ký trên web là Khách hàng
                profile = new KhachHang
                {
                    Hoten = model.Username, // model ở đây là RegisterViewModel
                    sđt = "Chưa cập nhật",
                };

                var newAccount = new Account
                {
                    Username = model.Username,
                    Email = model.Email,
                    Password = model.Password, // Trong thực tế nên Hash mật khẩu ở đây
                    Role = UserRole.KhachHang, // Mặc định là khách hàng
                    UserProfile = profile
                };

                // 3. Lưu vào DB
                _context.Accounts.Add(newAccount);
                _context.SaveChanges();

                // 4. Thông báo thành công
                TempData["Success"] = "Đăng ký tài khoản thành công! Hãy đăng nhập ngay.";
                return RedirectToAction("Login");
            }

            return View("Login"); // Nếu lỗi validate thì trả về trang Login để hiện lỗi
        }


        [HttpPost]
        [ValidateAntiForgeryToken] // Cờ này yêu cầu form phải có Token bảo mật, chống CSRF tuyệt đối
        public async Task<IActionResult> Logout()
        {
            // Lệnh này sẽ xóa sạch Cookie Authentication của người dùng
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Xóa xong thì đẩy về lại trang Đăng nhập
            return RedirectToAction("Login", "Account");
        }


        // 4. View sau khi đăng nhập thành công
        public IActionResult Success()
        {
            return View();
        }
    }
}