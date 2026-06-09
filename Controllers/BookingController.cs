using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PBL3_Hotel_System.Data;
using PBL3_Hotel_System.Helpers;
using PBL3_Hotel_System.Models;
using PBL3_Hotel_System.ViewModels;




namespace PBL3_Hotel_System.Controllers
{
    [Authorize(Roles = "KhachHang, NhanVien")]
    public class BookingController(HotelDbContext _context) : Controller
    {
        [HttpGet]
        public IActionResult Book(int roomID)
        {
            var room = _context.Rooms.FirstOrDefault(r => r.SoPhong == roomID);

            if (room == null)
            {
     
                TempData["Error"] = "Phòng này không tồn tại hoặc đang bảo trì. Vui lòng chọn phòng khác!";

                return RedirectToAction("Index", "Room");
            }

            var viewmodel = new BookViewModel
            {
                SoPhong = room.SoPhong,
                LoaiPhong = room.LoaiPhong.ToString(),
                GiaPhong = room.GiaPhong,
                Size = room.Size
            };

            return View(viewmodel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmBook(BookViewModel model)
        {
            if (!ModelState.IsValid) return View("Book", model);

            // 1. Lấy thông tin phòng và người dùng
            var room = _context.Rooms.FirstOrDefault(r => r.SoPhong == model.SoPhong);
            var currentUserName = User.Identity.Name;
            var account = _context.Accounts
                .Include(a => a.UserProfile)
                .FirstOrDefault(a => a.Username == currentUserName);

            if (account == null || account.UserProfile == null)
            {
                return Content("Lỗi: Không tìm thấy thông tin tài khoản hoặc hồ sơ khách hàng.");
            }
            if (room == null)
            {
                return Content("Lỗi: Không tìm thấy dữ liệu phòng.");
            }

            // 2. Tính toán tiền (Luôn tính lại ở Server để tránh khách hack F12)
            int soNgay = (model.CheckOut - model.CheckIn).Days;
            if (soNgay <= 0)
            {
                ModelState.AddModelError("", "Ngày trả phòng phải sau ngày nhận ít nhất 1 ngày");
                return View("Book", model);
            }
            // === Kiểm tra trùng lịch==
            bool isTrungLich = await _context.Bookings.AnyAsync(b =>
                b.SoPhong == model.SoPhong && // Kiểm tra đúng phòng đó
                b.TrangThaiDat != BookingStatus.DaHuy && // BỎ QUA các đơn đã bị hủy
                (model.CheckIn < b.CheckOut && model.CheckOut > b.CheckIn) // Công thức Overlap
            );

            if (isTrungLich)
            {
                // Nếu trùng, ném thông báo lỗi và đẩy khách quay lại trang đặt phòng
                TempData["Error"] = "Rất tiếc! Phòng này đã có người đặt trong khoảng thời gian bạn chọn. Vui lòng dời ngày hoặc chọn phòng khác.";
                var phong = await _context.Rooms.FirstOrDefaultAsync(r => r.SoPhong == model.SoPhong);
                if (phong != null)
                {
                    model.GiaPhong = room.GiaPhong;
                    model.LoaiPhong = room.LoaiPhong.ToString();
                    model.Size = room.Size;
                }
                return View("Book", model);
            }
            // 3. Tạo Model Database thực sự để lưu
            var booking = new Booking
            {
                MaKhachHang = account.UserProfile.UserID,
                SoPhong = model.SoPhong,
                CheckIn = model.CheckIn,
                CheckOut = model.CheckOut,
                GiaLucDat = soNgay * room.GiaPhong,
                GhiChu = model.GhiChu,
                TrangThaiDat = BookingStatus.ChoXacNhan, // Dùng Enum
                NgayDat = DateTime.Now
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Đặt phòng {model.SoPhong} thành công! Lịch hẹn từ {model.CheckIn:dd/MM} đến {model.CheckOut:dd/MM}.";
            return RedirectToAction("BookingView", "Room");
        }


        [HttpGet]
     
        public async Task<IActionResult> GetBookingDetailPartial(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.kh) // Phải có để lấy thông tin khách
                .FirstOrDefaultAsync(b => b.BookingID == id);

            if (booking == null) return NotFound("Không tìm thấy đơn đặt phòng.");

            // Xử lý đếm số ngày (Tránh lỗi ngày đi trùng ngày đến = 0)
            int soNgay = (booking.CheckOut - booking.CheckIn).Days;
            if (soNgay <= 0) soNgay = 1;

            var statusInfo = HotelStatusHelper.GetBookingStatus(booking.TrangThaiDat);
            var model = new BookingDetailViewModel
            {
                BookingID = booking.BookingID,
                TenKhachHang = booking.kh?.Hoten ?? "Khách vãng lai",
                SoDienThoai = booking.kh?.sđt ?? "N/A",
                CCCD = booking.kh?.CCCD ?? "N/A",
                DiaChi = booking.kh?.DiaChi ?? "N/A",
                SoPhong = booking.SoPhong,
                LoaiPhong = booking.Room?.LoaiPhong.ToString() ?? "N/A",
                GiaMotDem = booking.GiaLucDat,
                NgayDat = booking.NgayDat.ToString("dd/MM/yyyy HH:mm"),
                CheckIn = booking.CheckIn.ToString("dd/MM/yyyy"),
                CheckOut = booking.CheckOut.ToString("dd/MM/yyyy"),
                SoDem = soNgay,
                TongTien = booking.GiaLucDat.ToString("N0") + " ₫",
                GhiChu = booking.GhiChu ?? "Không có",
                RealCheckInFormatted = booking.RealCheckIn.HasValue
                           ? booking.RealCheckIn.Value.ToString("dd/MM/yyyy HH:mm")
                           : "Chưa cập nhật",
                TenTrangThai = statusInfo.Text,       // Trả về "Đang ở", "Chờ duyệt"...
                CssClassTrangThai = statusInfo.CssClass, // Trả về "status-info", "status-pending"...
                IsPaid = booking.IsPaid
            };

            return PartialView("_BookingDetailPartial", model);
        }
    }
}
