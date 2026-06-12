    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using PBL3_Hotel_System.Data;
    using PBL3_Hotel_System.Helpers;
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


                if (account == null || account.UserProfile == null)
                {
                    // Nếu không có hồ sơ, báo lỗi hoặc đẩy về trang cập nhật hồ sơ
                    return NotFound("Tài khoản của bạn chưa có hồ sơ thông tin!");
                }

                await AutoUpdateBookingStatusesAsync(account.UserProfile.UserID);//Auto Cap Nhat du lieu

          

                int khachHangId = account.UserProfile.UserID;

                // 2. Kéo dữ liệu thô từ Database lên (Lấy 5 cái mới nhất)
                var rawBookings = await _context.Bookings
                    .Include(b => b.Room)
                    .Where(b => b.MaKhachHang == khachHangId)
                    .OrderByDescending(b => b.NgayDat)
                    .Take(5)
                    .ToListAsync();

                var listGiaoDich = MapToViewModel(rawBookings);


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


            private async Task AutoUpdateBookingStatusesAsync(int khachHangId)
            {
                var now = DateTime.Now;
                var today = DateTime.Today; 
                // 1. Tìm tất cả đơn hàng của khách này mà chưa kết thúc
                var bookings = await _context.Bookings
                    .Where(b => b.MaKhachHang == khachHangId &&
                                b.TrangThaiDat != BookingStatus.DaHoanThanh &&
                                b.TrangThaiDat != BookingStatus.DaHuy)
                    .ToListAsync();

                bool hasChanged = false;

                foreach (var b in bookings)
                {
                    // TRƯỜNG HỢP 1: Chuyển sang "Sắp đến" 
                    // (Nếu đơn đã duyệt và thời gian hiện tại cách giờ Check-in < 24 tiếng)
                    if (b.TrangThaiDat == BookingStatus.DaXacNhan && b.CheckIn <= now.AddDays(1) && b.CheckIn > now)
                    {
                        b.TrangThaiDat = BookingStatus.SapDen;
                        hasChanged = true;
                    }

                    // TRƯỜNG HỢP 2: Chuyển sang "Hết hạn/Hủy" (No-show)
                    // (Nếu khách chưa nhận phòng mà đã quá giờ Check-out dự kiến)
                    if ((b.TrangThaiDat == BookingStatus.DaXacNhan || b.TrangThaiDat == BookingStatus.SapDen)
                        && today > b.CheckIn.Date
                        && b.CheckIn.Date < today)
                    {
                        b.TrangThaiDat = BookingStatus.DaHuy;
                        hasChanged = true;
                    }

                    // TRƯỜNG HỢP 3: Chuyển sang "Quá hạn trả phòng"
                    // (Nếu khách đang ở nhưng đã quá giờ Check-out dự kiến)
                    if (b.TrangThaiDat == BookingStatus.DangO && b.CheckOut < now)
                    {
                        b.TrangThaiDat = BookingStatus.QuaHan;
                        hasChanged = true;
                    }
                }

                // 2. Chỉ SaveChanges nếu thực sự có sự thay đổi để tối ưu hiệu năng
                if (hasChanged)
                {
                    await _context.SaveChangesAsync();
                }
            }

            // ====================================================
            // 2. TRANG LỊCH SỬ: Gọi giao diện lần đầu (Load trang 1)
            // ====================================================
            [HttpGet]
            public async Task<IActionResult> MyHistory()
            {
                var account = await GetCurrentAccountAsync();
                if (account?.UserProfile == null) return NotFound();

                // GỌI Ở ĐÂY: Để khách vào xem lịch sử luôn thấy trạng thái đúng nhất
                await AutoUpdateBookingStatusesAsync(account.UserProfile.UserID);

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

            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> XacNhanNhanPhong(int bookingID)
            {
                var currentUserName = User.Identity.Name;
                DateTime gioHienTai = DateTime.Now;

                // 1. Tìm đơn hàng kèm thông tin Phòng và User hồ sơ
                var booking = await _context.Bookings
                    .Include(b => b.Room)
                    .Include(b => b.kh) 
                        .ThenInclude(u => u.Account)
                    .FirstOrDefaultAsync(b => b.BookingID == bookingID && b.kh.Account.Username == currentUserName);

                if (booking == null) return NotFound();

                // ========================================================
                // 2. CÁC CHỐT CHẶN NGHIỆP VỤ (BẢO MẬT TẦNG SERVER)
                // ======================================================== 

                // Chốt 1: Phải ở trạng thái "Đã duyệt" mới được nhận phòng
                if (booking.TrangThaiDat != BookingStatus.DaXacNhan && booking.TrangThaiDat != BookingStatus.SapDen)
            {
                    TempData["Error"] = "Đơn hàng chưa sẵn sàng để nhận phòng!";
                    return RedirectToAction("Index");
                }

                // Chốt 2: Kiểm tra Giờ hẹn của nhân viên (QUAN TRỌNG NHẤT)
                if (booking.GioHenNhanPhong == null)
                {
                    TempData["Error"] = "Phòng của bạn đang được sắp xếp, vui lòng đợi lễ tân hẹn giờ!";
                    return RedirectToAction("Index");
                }

                if (gioHienTai < booking.GioHenNhanPhong)
                {
                    TempData["Error"] = $"Chưa đến giờ nhận phòng! Vui lòng quay lại sau {booking.GioHenNhanPhong:HH:mm dd/MM}.";
                    return RedirectToAction("Index");
                }

                // Chốt 3: Nếu khách để quá ngày trả phòng mới bấm thì coi như hết hạn
                if (gioHienTai > booking.CheckOut)
                {
                    TempData["Error"] = "Đơn đặt phòng này đã hết hạn thời gian lưu trú!";
                    return RedirectToAction("Index");
                }

                // ========================================================
                // 3. THỰC HIỆN CẬP NHẬT (THÀNH CÔNG)
                // ========================================================
                try
                {
                    // Đổi trạng thái Đơn hàng
                    booking.TrangThaiDat = BookingStatus.DangO;

                    // Đóng dấu giờ vào phòng THẬT SỰ
                    booking.RealCheckIn = gioHienTai;

                    // Đổi trạng thái vật lý của PHÒNG sang "Đang ở"
                    if (booking.Room != null)
                    {
                        booking.Room.TrangThai = RoomStatus.Occupied;
                    }

                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"Chúc mừng! Bạn đã nhận phòng {booking.SoPhong} thành công lúc {gioHienTai:HH:mm}.";
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Có lỗi xảy ra trong quá trình nhận phòng. Vui lòng liên hệ lễ tân!";
                }

                return RedirectToAction("Index");
            }

            private async Task<Account> GetCurrentAccountAsync()
            {
                return await _context.Accounts.Include(a => a.UserProfile)
                    .FirstOrDefaultAsync(a => a.Username == User.Identity.Name);
            }

            private List<RecentBookingViewModel> MapToViewModel(List<Booking> bookings)
            {
                return bookings.Select(b =>
                {
                    // 1. Lấy thông tin trạng thái từ Bộ từ điển dùng chung (Helper)
                    var statusInfo = HotelStatusHelper.GetBookingStatus(b.TrangThaiDat);

                    return new RecentBookingViewModel
                    {
                        SoPhong = b.SoPhong,
                        LoaiPhong = b.Room?.LoaiPhong.ToString() ?? "N/A",
                        BookingID = b.BookingID,
                        NgayDatFormatted = b.NgayDat.ToString("dd/MM/yyyy HH:mm"),
                        ThoiGianLuuTru = $"{b.CheckIn:dd/MM} - {b.CheckOut:dd/MM}",
                        CheckInDate = b.CheckIn,
                        CheckOutDate = b.CheckOut,
                        GioHenNhanPhong = b.GioHenNhanPhong,
                        TrangThaiRaw = b.TrangThaiDat.ToString(),

                        // SỬA LỖI TIỀN: Phải dùng b.TongTien (Tổng hóa đơn) thay vì GiaLucDat (1 đêm)
                        TongTienFormatted = b.GiaLucDat.ToString("N0") + " ₫",

                        // SỬA LỖI TRẠNG THÁI: Gán trực tiếp từ Helper
                        TenTrangThai = statusInfo.Text,       // Trả về "Chờ duyệt", "Đang ở"...
                        CssClassTrangThai = statusInfo.CssClass, // Trả về "status-pending", "status-info"...
                        IsPaid = b.IsPaid
                    };
                }).ToList();
            }

            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> YeuCauTraPhong(int bookingId)
            {
                var currentUserName = User.Identity.Name;
                var booking = await _context.Bookings
                    .Include(b => b.kh).ThenInclude(u => u.Account)
                    .FirstOrDefaultAsync(b => b.BookingID == bookingId && b.kh.Account.Username == currentUserName);

                if (booking == null) return Json(new { success = false, message = "Không tìm thấy giao dịch hoặc bạn không có quyền!" });

            // Chỉ cho phép gửi yêu cầu khi đang ở hoặc đã quá hạn
            if (booking.TrangThaiDat == BookingStatus.DangO || booking.TrangThaiDat == BookingStatus.QuaHan)
                {
                    booking.TrangThaiDat = BookingStatus.YeuCauTraPhong;
                    await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Đã gửi yêu cầu trả phòng. Vui lòng mang chìa khóa xuống quầy Lễ tân." });
            }

            return Json(new { success = false, message = "Giao dịch này không thể yêu cầu trả phòng lúc này!" });
        }

        }
    }
