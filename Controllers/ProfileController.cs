using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PBL3_Hotel_System.Data;
using PBL3_Hotel_System_.Models.UserModels;
using PBL3_Hotel_System.ViewModels.UserProfileViewModel;

namespace PBL3_Hotel_System_.Controllers
{
    [Authorize]
    public class ProfileController(HotelDbContext _context) :  Controller
    {
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userName = User.Identity.Name;
            var account = await _context.Accounts.Include(a => a.UserProfile)
                .FirstOrDefaultAsync(a => a.Username == userName);

            if (account == null) return NotFound();

            BaseProfileViewModel model;

            // KIỂM TRA ROLE ĐỂ KHỞI TẠO MODEL CON TƯƠNG ỨNG
            if (account.UserProfile is KhachHang kh)
            {
                model = new KhachHangProfileViewModel
                {
                    DiemTichLuy = kh.DiemTichLuy,
                    MemberRank = kh.MemberRank
                };
            }
            //else if (account.UserProfile is NhanVien nv)
            //{
            //    model = new NhanVienProfileViewModel
            //    {
            //        ChucVu = nv.ChucVu,
            //        Luong = nv.Luong
            //    };
            //}
            else
            {
                model = new BaseProfileViewModel();
            }

            // Gán thông tin chung
            model.Username = account.Username;
            model.Email = account.Email;
            model.HoTen = account.UserProfile.Hoten;
            model.SoDienThoai = account.UserProfile.sđt;
            model.CCCD = account.UserProfile.CCCD;
            model.DiaChi = account.UserProfile.DiaChi;
            model.Role = account.Role.ToString();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(BaseProfileViewModel model, string redirectUrl)
        {
            // Vì model truyền lên là Base, ta cần lấy dữ liệu thật từ DB để update
            var userName = User.Identity.Name;
            var account = await _context.Accounts.Include(a => a.UserProfile)
                .FirstOrDefaultAsync(a => a.Username == userName);

            if (account != null)
            {
                // Cập nhật thông tin chung (ai cũng có)
                account.UserProfile.Hoten = model.HoTen;
                account.UserProfile.sđt = model.SoDienThoai;
                account.UserProfile.CCCD = model.CCCD;
                account.UserProfile.DiaChi = model.DiaChi;

                // Xử lý thông tin riêng (nếu cần sửa thêm các trường đặc thù)
                // Ví dụ: if (account.UserProfile is KhachHang kh) { ... }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã cập nhật hồ sơ!";
            }

            if (!string.IsNullOrEmpty(redirectUrl) && Url.IsLocalUrl(redirectUrl))
                return LocalRedirect(redirectUrl);

            return RedirectToAction("Index");
        }

    }
}
