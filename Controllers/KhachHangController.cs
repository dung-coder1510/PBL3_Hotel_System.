using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PBL3_Hotel_System.Data;
using PBL3_Hotel_System.Models;
using PBL3_Hotel_System.ViewModels;

namespace PBL3_Hotel_System.Controllers
{
    [Authorize(Roles = "KhachHang")]
    public class KhachHangController(HotelDbContext _context) : Controller
    {
        [Authorize]   
        public async Task<IActionResult> Index()
        {
            // 1. Tìm UserProfile của khách hàng đang đăng nhập
            var currentUserName = User.Identity.Name;
            var account = await GetCurrentAccountAsync();

            if (account?.UserProfile == null) return NotFound("Không tìm thấy hồ sơ.");

            int khachHangId = account.UserProfile.UserID;

            // 2. Kéo dữ liệu thô từ Database lên (Lấy 5 cái mới nhất)
            var rawBookings = await _context.Bookings
                .Include(b => b.Room)
                .Where(b => b.MaKhachHang == khachHangId)
                .OrderByDescending(b => b.NgayDat)
                .Take(5)
                .ToListAsync();

            // 3. Mapping (Chuyển đổi) dữ liệu thô thành Dữ liệu hiển thị (ViewModel)
            var listGiaoDich = rawBookings.Select(b => new RecentBookingViewModel
            {
                SoPhong = b.SoPhong,
                LoaiPhong = b.Room?.LoaiPhong.ToString() ?? "Không xác định",
                NgayDatFormatted = b.NgayDat.ToString("dd/MM/yyyy HH:mm"),
                ThoiGianLuuTru = b.CheckIn.ToString("dd/MM") + " - " + b.CheckOut.ToString("dd/MM"),
                TongTienFormatted = b.GiaLucDat.ToString("N0") + " ₫",

                // Xử lý Logic màu sắc và chữ ngay tại C#
                TenTrangThai = b.TrangThaiDat == BookingStatus.ChoXacNhan ? "Chờ duyệt" :
                               b.TrangThaiDat == BookingStatus.DaXacNhan ? "Đã duyệt" :
                               b.TrangThaiDat == BookingStatus.DaHoanThanh ? "Thành công" : "Đã hủy",

                CssClassTrangThai = b.TrangThaiDat == BookingStatus.ChoXacNhan ? "status-pending" :
                                    b.TrangThaiDat == BookingStatus.DaXacNhan ? "status-info" :
                                    b.TrangThaiDat == BookingStatus.DaHoanThanh ? "status-success" : "status-cancel"
            }).ToList();

            // 4. Tính toán thống kê
            int tongSoPhong = await _context.Bookings.CountAsync(b => b.MaKhachHang == khachHangId);
            decimal tongTienTieu = await _context.Bookings.Where(b => b.MaKhachHang == khachHangId && b.TrangThaiDat == BookingStatus.DaHoanThanh).SumAsync(b => b.GiaLucDat);

            // 5. Đóng gói vào "Vali Tổng" và ném ra View
            var dashboardData = new DashboardViewModel
            {
                TenKhachHang = account.UserProfile.Hoten,
                TotalBookings = tongSoPhong,
                TotalPoints = tongTienTieu / 10000, // Ví dụ: 10.000đ = 1 điểm
                MemberRank = tongSoPhong >= 5 ? "Platinum" : "Gold", // Logic rank cơ bản
                RecentBookings = listGiaoDich
            };

            return View(dashboardData);
        }

        // ====================================================
        // 2. TRANG LỊCH SỬ: Gọi giao diện lần đầu (Load trang 1)
        // ====================================================
        [HttpGet]
        public async Task<IActionResult> MyHistory()
        {
            // Trả về View rỗng kèm theo layout, dữ liệu sẽ được AJAX nạp tự động
            return View();
        }

        // ====================================================
        // 3. AJAX CORE: Lọc, Sắp xếp và Phân trang (Mỗi trang 10 dòng)
        // ====================================================
        [HttpGet]
        public async Task<IActionResult> FilterTransactions(string status, string date, string sort, int page = 1)
        {
            var account = await GetCurrentAccountAsync();
            if (account == null) return BadRequest();

            // 1. Khởi tạo Query
            var query = _context.Bookings.Include(b => b.Room)
                .Where(b => b.MaKhachHang == account.UserProfile.UserID).AsQueryable();

            // 2. Lọc theo Trạng thái
            if (!string.IsNullOrEmpty(status) && status != "All" && Enum.TryParse<BookingStatus>(status, out var parsedStatus))
                query = query.Where(b => b.TrangThaiDat == parsedStatus);

            // 3. Lọc theo Ngày nhận phòng
            if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out DateTime parsedDate))
                query = query.Where(b => b.CheckIn.Date == parsedDate.Date);

            // 4. Sắp xếp
            query = sort switch
            {
                "PriceAsc" => query.OrderBy(b => b.GiaLucDat),
                "PriceDesc" => query.OrderByDescending(b => b.GiaLucDat),
                "Oldest" => query.OrderBy(b => b.NgayDat),
                _ => query.OrderByDescending(b => b.NgayDat) // Mặc định Mới nhất
            };

            // 5. Phân trang (Pagination)
            int pageSize = 10;
            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            if (page < 1) page = 1;

            var pagedData = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            // 6. Truyền dữ liệu phân trang ra ViewBag cho Partial View
            ViewBag.IsDashboard = false;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return PartialView("_TransactionTablePartial", MapToViewModel(pagedData));
        }

        private async Task<Account> GetCurrentAccountAsync()
        {
            return await _context.Accounts.Include(a => a.UserProfile)
                .FirstOrDefaultAsync(a => a.Username == User.Identity.Name);
        }

        private List<RecentBookingViewModel> MapToViewModel(List<Booking> bookings)
        {
            return bookings.Select(b => new RecentBookingViewModel
            {
                SoPhong = b.SoPhong,
                LoaiPhong = b.Room?.LoaiPhong.ToString() ?? "N/A",
                NgayDatFormatted = b.NgayDat.ToString("dd/MM/yyyy HH:mm"),
                ThoiGianLuuTru = b.CheckIn.ToString("dd/MM") + " - " + b.CheckOut.ToString("dd/MM"),
                TongTienFormatted = b.GiaLucDat.ToString("N0") + " ₫",
                TenTrangThai = b.TrangThaiDat == BookingStatus.ChoXacNhan ? "Chờ duyệt" :
                               b.TrangThaiDat == BookingStatus.DaXacNhan ? "Đã duyệt" :
                               b.TrangThaiDat == BookingStatus.DaHoanThanh ? "Thành công" : "Đã hủy",
                CssClassTrangThai = b.TrangThaiDat switch
                {
                    BookingStatus.ChoXacNhan => "status-pending",  // Vàng
                    BookingStatus.DaXacNhan => "status-success",   // Xanh lá
                    BookingStatus.DaHoanThanh => "status-finished",// Xám
                    BookingStatus.DaHuy => "status-danger",     // Đỏ
                    _=> "status-finished"
                }
            }).ToList();
        }

    }
}
