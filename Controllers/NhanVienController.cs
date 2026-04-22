using Microsoft.AspNetCore.Mvc;
using PBL3_Hotel_System.Data;
using PBL3_Hotel_System.Models;

using Microsoft.EntityFrameworkCore;

namespace PBL3_Hotel_System_.Controllers
{
    public class NhanVienController(HotelDbContext _context) : Controller
    {
        private int GetMaNV()
        {
            // 1. Lấy tên tài khoản đang đăng nhập từ Cookie
            var userName = User.Identity?.Name;
            if (string.IsNullOrEmpty(userName)) return 0;

            // 2. Tìm tài khoản trong DB và nạp kèm (Include) hồ sơ người dùng
            var account = _context.Accounts
                .Include(a => a.UserProfile)
                .FirstOrDefault(a => a.Username == userName);

            // 3. Trả về UserID của hồ sơ (Đây chính là MaNV mà Database đang chờ)
            return account?.UserProfile?.UserID ?? 0;
        }
        private List<DateTime> GetWeekDays()
        {
            DateTime today = DateTime.Today;

            // Tính khoảng cách từ ngày hiện tại lùi về Thứ 2 gần nhất
            // DayOfWeek: Sunday = 0, Monday = 1, ..., Saturday = 6
            int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
            DateTime startOfWeek = today.AddDays(-1 * diff);

            // Tạo danh sách 7 ngày liên tiếp
            return Enumerable.Range(0, 7)
                             .Select(i => startOfWeek.AddDays(i))
                             .ToList();
        }
        public IActionResult Index()
        {
            ViewBag.HoTen = User.Identity?.Name;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> DatCaLam()
        {
            // 1. Lấy thông tin nhân viên đang đăng nhập
            int maNV = GetMaNV();
            if (maNV == 0) return RedirectToAction("Login", "Account");

            // 2. Lấy danh sách 7 ngày trong tuần hiện tại (Thứ 2 -> Chủ nhật)
            List<DateTime> weekDays = GetWeekDays(); // Hàm phụ lấy list 7 ngày
            ViewBag.WeekDays = weekDays;
            DateTime startDate = weekDays.First();
            DateTime endDate = weekDays.Last();

            // 3. Lấy danh sách các ca mà nhân viên này ĐÃ đăng ký trong tuần
            ViewBag.RegisteredShifts = await _context.DangKyCaLams
                .Where(r => r.MaNV == maNV && r.NgayLam >= startDate && r.NgayLam <= endDate)
                .ToListAsync();
            ViewBag.RegisteredCount = ViewBag.RegisteredShifts.Count;   
            // 4. THUẬT TOÁN ĐẾM CHỖ TRỐNG: 
            // Group dữ liệu để biết mỗi Ca_Ngày đã có bao nhiêu người đặt
            var shiftOccupancy = await _context.DangKyCaLams
                .Where(r => r.NgayLam >= startDate && r.NgayLam <= endDate && r.TrangThai != "Rejected")
                .GroupBy(r => new { r.MaCa, r.NgayLam.Date })
                .Select(g => new {
                    Key = g.Key.MaCa + "_" + g.Key.Date.ToString("yyyy-MM-dd"),
                    Count = g.Count()
                })
                .ToDictionaryAsync(x => x.Key, x => x.Count);

            ViewBag.ShiftOccupancy = shiftOccupancy;

            // 5. Lấy danh sách khung giờ ca làm (Sáng, Chiều, Tối)
            var allShifts = await _context.CaLams.ToListAsync();

            ViewData["ActiveMenu"] = "DatCaLam";
            return View(allShifts);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> DangKyCa(List<string> selectedShifts)
        {
            int maNV = GetMaNV();
            if (maNV == 0) return RedirectToAction("Login", "Account");

            // Lỗi 1: Không chọn ca nào
            if (selectedShifts == null || !selectedShifts.Any())
            {
                TempData["Error"] = "Vui lòng chọn ít nhất một ca làm để đăng ký!";
                return RedirectToAction("DatCaLam");
            }

            var requests = selectedShifts.Select(s => {
                var parts = s.Split('|');
                return new { MaCa = int.Parse(parts[0]), NgayLam = DateTime.Parse(parts[1]).Date };
            }).ToList();

            DateTime weekStart = requests.Min(r => r.NgayLam);
            DateTime weekEnd = requests.Max(r => r.NgayLam);

            // Lỗi 2: Quá 7 ca / tuần
            int existingWeekCount = await _context.DangKyCaLams
        .CountAsync(x => x.MaNV == maNV
                    && x.NgayLam.Date >= weekStart.Date
                    && x.NgayLam.Date <= weekEnd.Date
                    && x.TrangThai != "Rejected");

            if (existingWeekCount + requests.Count > 7)
            {
                TempData["Error"] = $"không thể đăng ký thêm {requests.Count} ca (Tối đa 7 ca/tuần).";
                return RedirectToAction("DatCaLam");
            }

            // Lỗi 3: Quá 2 ca / ngày
            var dailyGroups = requests.GroupBy(r => r.NgayLam);
            foreach (var group in dailyGroups)
            {
                DateTime currentDay = group.Key;
                int inDbCount = await _context.DangKyCaLams
                    .CountAsync(x => x.MaNV == maNV
                                && x.NgayLam.Date == currentDay.Date
                                && x.TrangThai != "Rejected");

                if (inDbCount + group.Count() > 2)
                {
                    TempData["Error"] = $"Ngày {currentDay:dd/MM} bạn đã chọn 3 ca (Tối đa 2 ca/ngày).";
                    return RedirectToAction("DatCaLam");
                }
            }

            // Lỗi 4: Ca bị đầy trong lúc khách đang chọn (Chống hack F12)
            List<DangKiCaLam> listToSave = new List<DangKiCaLam>();
            foreach (var item in requests)
            {
                var caHienTai = await _context.CaLams.FindAsync(item.MaCa);
                int occupied = await _context.DangKyCaLams.CountAsync(x => x.MaCa == item.MaCa && x.NgayLam == item.NgayLam && x.TrangThai != "Rejected");

                if (occupied >= caHienTai.SoLuongToiDa)
                {
                    TempData["Error"] = $"Ca {caHienTai.TenCa} ngày {item.NgayLam:dd/MM} vừa mới đầy chỗ. Vui lòng chọn ca khác!";
                    return RedirectToAction("DatCaLam");
                }
               
                listToSave.Add(new DangKiCaLam { MaNV = maNV, MaCa = item.MaCa, NgayLam = item.NgayLam, TrangThai = "Pending" });
            }

            // LƯU THÀNH CÔNG
            _context.DangKyCaLams.AddRange(listToSave);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đăng ký thành công {listToSave.Count} ca làm việc!";
            return RedirectToAction("DatCaLam");
        }
        public IActionResult QuanLyPhong(string searchString, RoomStatus? statusFilter)
        {
            // 1. Lấy tất cả danh sách dưới dạng IQueryable để lọc dần dần
            var rooms = _context.Rooms.AsQueryable();

            // 2. Tìm kiếm theo Số phòng (nếu người dùng nhập)
            if (!string.IsNullOrEmpty(searchString))
            {
                // Vì SoPhong là kiểu int, ta chuyển về string để tìm kiếm chứa (Contains)
                // hoặc dùng == nếu muốn tìm chính xác số phòng
                rooms = rooms.Where(r => r.SoPhong.ToString().Contains(searchString));
            }

            // 3. Lọc theo Trạng thái (nếu người dùng chọn)
            if (statusFilter.HasValue)
            {
                rooms = rooms.Where(r => r.TrangThai == statusFilter.Value);
            }

            // Gửi lại giá trị tìm kiếm để hiển thị trên input sau khi Load lại trang
            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentStatus = statusFilter;

            return View(rooms.ToList());
        }

        [HttpPost]
        public IActionResult CapNhatTrangThaiPhong(int soPhong, string trangThaiMoi)
        {
            var phong = _context.Rooms.Find(soPhong);
            if (trangThaiMoi == "Đang ở")
            {
                TempData["Error"] = "Lỗi bảo mật: Trạng thái 'Đang ở' chỉ được cập nhật tự động thông qua quy trình Check-in khách hàng!";
                return RedirectToAction("QuanLyPhong");
            }

            // 2. NGĂN CHẶN ĐỔI TỪ "ĐANG Ở" SANG TRẠNG THÁI KHÁC (Phải qua Check-out)
            if (phong.TrangThai == RoomStatus.Occupied)
            {
                TempData["Error"] = "Phòng đang có khách! Vui lòng thực hiện Check-out trước khi dọn dẹp hoặc bảo trì.";
                return RedirectToAction("QuanLyPhong");
            }
            if (phong != null)
            {
                // Ánh xạ từ chuỗi tiếng Việt nhận được từ giao diện sang Enum tiếng Anh trong Model
                phong.TrangThai = trangThaiMoi switch
                {
                    "Trống" => RoomStatus.Available,
                    "Đang ở" => RoomStatus.Occupied,
                    "Đang dọn dẹp" => RoomStatus.Cleaning,
                    "Bảo trì" => RoomStatus.Maintenance,
                    _ => phong.TrangThai // Nếu không khớp cái nào thì giữ nguyên trạng thái cũ
                };

                _context.SaveChanges();
            }
            return RedirectToAction("QuanLyPhong");
        }

        // Action 1: Trang duyệt đơn đặt phòng
        public async Task<IActionResult> QuanLyKhachHang()
        {
            // Phải có .Include để lấy thông tin khách hàng và phòng, nếu không KhachHang và Room sẽ bị NULL
            var bookings = await _context.Bookings
                .Include(b => b.kh)
                .Include(b => b.Room)
                .OrderByDescending(b => b.NgayDat)
                .ToListAsync();

            return View(bookings); // <--- ĐẢM BẢO CÓ BIẾN bookings Ở ĐÂY
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DuyetBooking(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn đặt phòng!";
                return RedirectToAction("QuanLyKhachHang");
            }

            if (booking.TrangThaiDat == BookingStatus.ChoXacNhan)
            {
                booking.TrangThaiDat = BookingStatus.DaXacNhan;
                _context.Update(booking);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã duyệt thành công đơn hàng #{id}!";
            }

            return RedirectToAction("QuanLyKhachHang");
        }
    }
}
