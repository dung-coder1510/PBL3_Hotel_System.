using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PBL3_Hotel_System.Data;
using PBL3_Hotel_System.Models.UserModels;
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
            // 1. Lấy dữ liệu hiện tại "xịn" từ Database lên
            var userName = User.Identity.Name;
            var account = await _context.Accounts
                .Include(a => a.UserProfile)
                .FirstOrDefaultAsync(a => a.Username == userName);

            if (account != null && account.UserProfile != null)
            {
                // 2. CHỐT CHẶN THÔNG MINH: Chỉ cập nhật nếu dữ liệu gửi lên không rỗng

                if (!string.IsNullOrWhiteSpace(model.HoTen))
                {
                    bool sdtTonTai = await _context.UserProfiles
                        .AnyAsync(u => u.sđt == model.SoDienThoai && u.AccountID != account.AccountID);

                    if (sdtTonTai)
                    {
                        TempData["Error"] = "Số điện thoại này đã được sử dụng bởi một thành viên khác!";
                        return RedirectToAction("Index");
                    }
                    account.UserProfile.Hoten = model.HoTen;
                }

                if (!string.IsNullOrWhiteSpace(model.SoDienThoai))
                {
                    bool cccdTonTai = await _context.UserProfiles
                        .AnyAsync(u => u.CCCD == model.CCCD && u.AccountID != account.AccountID);

                    if (cccdTonTai)
                    {
                        TempData["Error"] = "Số CCCD/Hộ chiếu này đã tồn tại trên hệ thống!";
                        return RedirectToAction("Index");
                    }
                    account.UserProfile.sđt = model.SoDienThoai;
                }

                if (!string.IsNullOrWhiteSpace(model.CCCD))
                {
                    account.UserProfile.CCCD = model.CCCD;
                }

                if (!string.IsNullOrWhiteSpace(model.DiaChi))
                {
                    account.UserProfile.DiaChi = model.DiaChi;
                }

                // 3. Xử lý các trường đặc thù của Khách hàng (nếu có)
                if (account.UserProfile is KhachHang kh)
                {
                    // Ví dụ: chỉ cập nhật nếu trong model con có dữ liệu đặc thù
                    if (model is KhachHangProfileViewModel khModel && !string.IsNullOrWhiteSpace(khModel.CCCD))
                    {
                        kh.CCCD = khModel.CCCD;
                    }
                }

                // 4. Lưu thay đổi
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã cập nhật thông tin thành công!";
            }

            // Điều hướng như cũ
            if (!string.IsNullOrEmpty(redirectUrl) && Url.IsLocalUrl(redirectUrl))
                return LocalRedirect(redirectUrl);

            return RedirectToAction("Index");
        }

    }
}
