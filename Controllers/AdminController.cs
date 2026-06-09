using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PBL3_Hotel_System.Data;
using PBL3_Hotel_System.Models;
using PBL3_Hotel_System.ViewModels; // Chứa các ViewModel nếu cần
using PBL3_Hotel_System.Models.UserModels;
namespace PBL3_Hotel_System.Controllers
{
    [Authorize(Roles = "QuanTriVien")] // Chỉ cho phép Admin truy cập
    public class AdminController(HotelDbContext _db) : Controller
    {
        // 1. TRANG DASHBOARD ADMIN
        public async Task<IActionResult> Index()
        {
            ViewData["ActiveMenu"] = "Index";
            // Đếm nhân viên từ bảng kế thừa UserProfiles
            ViewBag.TotalStaff = await _db.UserProfiles.OfType<NhanVien>().CountAsync();
            ViewBag.TotalRooms = await _db.Rooms.CountAsync();
            
            // Giả sử bạn có bảng Đăng ký ca làm
            ViewBag.PendingShifts = await _db.DangKyCaLams.CountAsync(x => x.TrangThai == ShiftStatus.Pending);
            
            return View();
        }
        // ========================================================
        // 1. API: DỮ LIỆU BIỂU ĐỒ DOANH THU (Line Chart)
        // ========================================================
        [HttpGet]
        public async Task<IActionResult> GetRevenueChartData(int year)
        {
            // Lọc các đơn hàng ĐÃ HOÀN THÀNH trong năm được chọn
            var bookings = await _db.Bookings
                .Where(b => b.CheckOut.Year == year && b.TrangThaiDat == BookingStatus.DaHoanThanh)
                .ToListAsync();

            // Tạo mảng 12 tháng mặc định là 0 (Để lỡ tháng nào không có khách thì biểu đồ không bị đứt)
            decimal[] monthlyRevenue = new decimal[12];

            foreach (var b in bookings)
            {
                int monthIndex = b.CheckOut.Month - 1; // Mảng bắt đầu từ 0
                monthlyRevenue[monthIndex] += b.GiaLucDat;
            }

            return Json(new
            {
                labels = new[] { "Thg 1", "Thg 2", "Thg 3", "Thg 4", "Thg 5", "Thg 6", "Thg 7", "Thg 8", "Thg 9", "Thg 10", "Thg 11", "Thg 12" },
                data = monthlyRevenue
            });
        }

        // ========================================================
        // 2. API: DỮ LIỆU TỈ LỆ LOẠI PHÒNG (Pie Chart)
        // ========================================================
        [HttpGet]
        public async Task<IActionResult> GetRoomTypeChartData(int year)
        {
            // Đếm số LƯỢT ĐẶT theo từng loại phòng (Không tính đơn Đã Hủy)
            var roomData = await _db.Bookings
                .Include(b => b.Room)
                .Where(b => b.NgayDat.Year == year && b.TrangThaiDat != BookingStatus.DaHuy)
                .GroupBy(b => b.Room.LoaiPhong.ToString())
                .Select(g => new {
                    LoaiPhong = g.Key,
                    SoLuotDat = g.Count()
                })
                .ToListAsync();

            var labels = roomData.Select(x => x.LoaiPhong).ToArray();
            var data = roomData.Select(x => x.SoLuotDat).ToArray();

            return Json(new { labels, data });
        }

        // ========================================================
        // 3. API: DỮ LIỆU TÌNH TRẠNG PHÒNG HIỆN TẠI (Doughnut Chart)
        // ========================================================
        [HttpGet]
        public async Task<IActionResult> GetRoomStatusChartData()
        {
            // Đếm số lượng phòng nhóm theo trạng thái
            var statusData = await _db.Rooms
                .GroupBy(r => r.TrangThai)
                .Select(g => new {
                    TrangThai = g.Key.ToString(),
                    SoLuong = g.Count()
                })
                .ToListAsync();

            // Dịch Enum tiếng Anh sang tiếng Việt để hiển thị đẹp hơn
            var labels = statusData.Select(x => TranslateRoomStatus(x.TrangThai)).ToArray();
            var data = statusData.Select(x => x.SoLuong).ToArray();

            return Json(new { labels, data });
        }

        // Hàm phụ trợ dịch trạng thái
        private string TranslateRoomStatus(string status)
        {
            return status switch
            {
                "Available" => "Trống (Sẵn sàng)",
                "Occupied" => "Đang có khách",
                "Cleaning" => "Đang dọn dẹp",
                "Maintenance" => "Đang bảo trì",
                _ => "Không xác định"
            };
        }
        // 2. QUẢN LÝ TÀI KHOẢN (Account)
        public async Task<IActionResult> QuanLyTaiKhoan(string searchString)
        {
            ViewData["ActiveMenu"] = "Accounts";
            var query = _db.Accounts.Include(a => a.UserProfile).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {   
                searchString = searchString.ToLower();
                query = query.Where(a => a.Username.ToLower().Contains(searchString)
                                      || (a.UserProfile != null && a.UserProfile.Hoten.ToLower().Contains(searchString)));
            }

            ViewBag.CurrentFilter = searchString;
            return View(await query.ToListAsync());
        }



        [HttpGet]
        public IActionResult GetCreateAccountPartial()
        {
            // Tạo một model rỗng với ID mặc định là 0
            var model = new AdminAccountViewModel { AccountID = 0 };

            // Trả về file HTML dùng chung
            return PartialView("_AccountFormPartial", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAccount(AdminAccountViewModel model)
        {
            // 1. Kiểm tra Validation từ ViewModel (Các thuộc tính [Required], [EmailAddress]...)
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Dữ liệu nhập vào chưa đúng định dạng. Vui lòng kiểm tra lại!";
                return RedirectToAction("QuanLyTaiKhoan");
            }

            // 2. Chốt chặn mật khẩu (Vì trong ViewModel ta để mật khẩu là Nullable để dùng chung cho hàm Sửa)
            if (string.IsNullOrWhiteSpace(model.Password))
            {
                TempData["Error"] = "Mật khẩu là bắt buộc khi cấp tài khoản mới!";
                return RedirectToAction("QuanLyTaiKhoan");
            }

            // 3. KIỂM TRA TRÙNG LẶP (Chốt chặn bảo mật DB)

            // Kiểm tra tên đăng nhập (Username)
            bool isUsernameExist = await _db.Accounts.AnyAsync(a => a.Username == model.Username);
            if (isUsernameExist)
            {
                TempData["Error"] = $"Tên đăng nhập '{model.Username}' đã được sử dụng. Vui lòng chọn tên khác!";
                return RedirectToAction("QuanLyTaiKhoan");
            }

            // Kiểm tra Email
            bool isEmailExist = await _db.Accounts.AnyAsync(a => a.Email == model.Email);
            if (isEmailExist)
            {
                TempData["Error"] = $"Địa chỉ Email '{model.Email}' đã tồn tại trên hệ thống!";
                return RedirectToAction("QuanLyTaiKhoan");
            }
            BaseUser profile;
            if (model.Role == UserRole.KhachHang)
            {
                profile = new KhachHang { 
                    Hoten = model.Username,
                    sđt = "Chưa cập nhật",
                    CCCD = "Chưa cập nhật",
                    DiaChi = "Chưa cập nhật",
                    DiemTichLuy = 0 
                };
            }
            else
            {
                // Nếu là Nhân viên hoặc Admin thì tạo hồ sơ NhanVien
                profile = new NhanVien {
                    sđt = "Chưa cập nhật",
                    CCCD = "Chưa cập nhật",
                    DiaChi = "Chưa cập nhật",
                    Hoten = model.Username
                };
            }
            // 4. TIẾN HÀNH TẠO MỚI
            try
            {
                var newAccount = new Account
                {
                    Username = model.Username.Trim(),
                    Email = model.Email.Trim(),
                    Password = model.Password, // Gợi ý: Sau này hãy dùng thư viện để Hash Password ở đây
                    Role = model.Role,
                    IsLocked = model.IsLocked,
                    UserProfile = profile
                };

                _db.Accounts.Add(newAccount);
                await _db.SaveChangesAsync();

                // Thông báo thành công rực rỡ qua Toast
                TempData["Success"] = $"Hệ thống đã cấp thành công tài khoản cho '{model.Username}'!";
            }
            catch (Exception ex)
            {
                // Xử lý lỗi hệ thống/lỗi Database không lường trước
                //TempData["Error"] = "Có lỗi xảy ra trong quá trình lưu Database. Vui lòng thử lại sau!";
                TempData["Error"] = "Lỗi DB: " + (ex.InnerException != null ? ex.InnerException.Message : ex.Message);
            }

            return RedirectToAction("QuanLyTaiKhoan");
        }



        // 1. AJAX GET: LẤY FORM SỬA TÀI KHOẢN
        [HttpGet]
        public async Task<IActionResult> GetEditAccountPartial(string username)
        {
            var account = await _db.Accounts
                .Include(a => a.UserProfile)
                .FirstOrDefaultAsync(a => a.Username == username);

            if (account == null) return NotFound("Không tìm thấy tài khoản.");

            var model = new AdminAccountViewModel
            {
                AccountID = account.AccountID,
                Username = account.Username,
                Email = account.Email,
                Role = account.Role,
                OwnerName = account.UserProfile?.Hoten ?? "Chưa có hồ sơ"
            };

            return PartialView("_AccountFormPartial", model);
        }

        // 2. POST: ADMIN LƯU THAY ĐỔI
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAccountInfo(AdminAccountViewModel model)
        {
            // 1. Tìm Account gốc trong DB
            var account = await _db.Accounts.FindAsync(model.AccountID);
            if (account == null)
            {
                TempData["Error"] = "Lỗi: Không tìm thấy tài khoản để cập nhật!";
                return RedirectToAction("QuanLyTaiKhoan");
            }

            // 2. KIỂM TRA TRÙNG LẶP CHO CÁC TRƯỜNG CÓ THAY ĐỔI
            // Chỉ kiểm tra Email nếu Admin có gửi Email lên và nó khác với Email cũ
            if (!string.IsNullOrWhiteSpace(model.Email) && model.Email != account.Email)
            {
                bool isEmailExist = await _db.Accounts.AnyAsync(a => a.Email == model.Email);
                if (isEmailExist)
                {
                    TempData["Error"] = "Lỗi: Địa chỉ Email mới đã có người khác sử dụng!";
                    return RedirectToAction("QuanLyTaiKhoan");
                }
            }

            // Tương tự, chỉ kiểm tra Username nếu Admin có gửi Username lên
            if (!string.IsNullOrWhiteSpace(model.Username) && model.Username != account.Username)
            {
                bool isUsernameExist = await _db.Accounts.AnyAsync(a => a.Username == model.Username);
                if (isUsernameExist)
                {
                    TempData["Error"] = "Lỗi: Tên đăng nhập này đã có người sử dụng!";
                    return RedirectToAction("QuanLyTaiKhoan");
                }
            }

            // ========================================================
            // 3. THỰC HIỆN CẬP NHẬT CÓ CHỌN LỌC (SELECTIVE UPDATE)
            // Nguyên tắc: "Chỉ cập nhật khi dữ liệu gửi lên CÓ CHỮ và KHÁC RỖNG"
            // ========================================================

            if (!string.IsNullOrWhiteSpace(model.Username))
            {
                account.Username = model.Username.Trim();
            }

            if (!string.IsNullOrWhiteSpace(model.Email))
            {
                account.Email = model.Email.Trim();
            }

            // Với Enum, nó không bao giờ bị null (mặc định sẽ về 0).
            // Nếu bạn muốn an toàn, có thể dùng kiểu Enum? (Nullable Enum) trong ViewModel, nhưng thường Role luôn được gửi đi từ <select>
            account.Role = model.Role;

            // Với Boolean (IsLocked), nó cũng không thể null. 
            account.IsLocked = model.IsLocked;

            // 4. XỬ LÝ MẬT KHẨU
            // Nếu Admin CÓ NHẬP chữ vào ô Mật khẩu mới thì mới ghi đè
            // Nếu để trống thì giữ nguyên mật khẩu cũ trong DB
            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                account.Password = model.Password;
            }

            // 5. LƯU THAY ĐỔI
            try
            {
                await _db.SaveChangesAsync();
                TempData["Success"] = $"Cập nhật thành công tài khoản: {account.Username}!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi hệ thống trong quá trình lưu dữ liệu!";
            }

            return RedirectToAction("QuanLyTaiKhoan");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLockAccount(int accountId)
        {
            // 1. Tìm tài khoản trong Database
            var account = await _db.Accounts.FindAsync(accountId);

            if (account == null)
            {
                TempData["Error"] = "Lỗi: Không tìm thấy tài khoản!";
                return RedirectToAction("QuanLyTaiKhoan");
            }

            // Bảo vệ: Không cho phép Admin tự khóa tài khoản của chính mình
            if (account.Username == User.Identity.Name)
            {
                TempData["Error"] = "Cảnh báo: Bạn không thể tự khóa tài khoản của chính mình!";
                return RedirectToAction("QuanLyTaiKhoan");
            }

            // 2. Lật ngược trạng thái (Toggle)
            account.IsLocked = !account.IsLocked;

            // 3. Lưu vào Database
            await _db.SaveChangesAsync();

            // 4. Báo cáo bằng Toast notification
            if (account.IsLocked)
            {
                TempData["Success"] = $"Đã KHÓA tài khoản {account.Username} thành công!";
            }
            else
            {
                TempData["Success"] = $"Đã MỞ KHÓA tài khoản {account.Username} thành công!";
            }

            // Load lại trang danh sách tài khoản
            return RedirectToAction("QuanLyTaiKhoan");
        }
        // 3. QUẢN LÝ NHÂN VIÊN (Dựa trên lớp kế thừa NhanVien)
        public async Task<IActionResult> QuanLyNhanVien(string searchString)
        {
            ViewData["ActiveMenu"] = "Staff";
            // Lấy danh sách những người là NhanVien trong bảng UserProfiles
            var query = _db.UserProfiles.OfType<NhanVien>().AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                query = query.Where(nv => nv.Hoten.ToLower().Contains(searchString)
                                       || nv.UserID.ToString().Contains(searchString));
            }

            ViewBag.CurrentFilter = searchString;
            return View(await query.ToListAsync());
        }

        // =========================================================
        // 1. AJAX: MỞ POPUP SỬA HỒ SƠ NHÂN VIÊN
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> GetEditNhanVienPartial(int id)
        {
            // Tìm nhân viên trong DB
            var nv = await _db.UserProfiles.OfType<NhanVien>()
                            .Include(u => u.Account)
                            .FirstOrDefaultAsync(u => u.UserID == id);

            if (nv == null) return NotFound("Không tìm thấy hồ sơ nhân viên.");

            DateTime today = DateTime.Today;

            // Tính đầu tuần (Thứ 2) và cuối tuần (Chủ nhật)
            int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
            DateTime startOfWeek = today.AddDays(-1 * diff);
            DateTime endOfWeek = startOfWeek.AddDays(6);

            // Tính đầu tháng và cuối tháng
            DateTime startOfMonth = new DateTime(today.Year, today.Month, 1);
            DateTime endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            // 2. QUERY SỐ CA ĐÃ DUYỆT CỦA NHÂN VIÊN NÀY
            int caTuanNay = await _db.DangKyCaLams.CountAsync(c =>
                c.MaNV == id && c.TrangThai == ShiftStatus.Approved
                && c.NgayLam.Date >= startOfWeek && c.NgayLam.Date <= endOfWeek);

            int caThangNay = await _db.DangKyCaLams.CountAsync(c =>
                c.MaNV == id && c.TrangThai == ShiftStatus.Approved
                && c.NgayLam.Date >= startOfMonth && c.NgayLam.Date <= endOfMonth);

            // 3. GÁN VÀO VIEW MODEL (Nhân với 400k)
            decimal donGiaCa = 400000m;

            var model = new AdminNhanVienEditViewModel
            {
                UserID = nv.UserID,
                HoTen = nv.Hoten,
                SoDienThoai = nv.sđt,
                DiaChi = nv.DiaChi,
                Username = nv.Account?.Username ?? "Chưa liên kết tài khoản",

                // Format ra tiền Việt Nam luôn
                LuongTuanNay = (caTuanNay * donGiaCa).ToString("N0") + " ₫",
                LuongThangNay = (caThangNay * donGiaCa).ToString("N0") + " ₫"
            };

            return PartialView("_EditNhanVienPartial", model);
        }

        // =========================================================
        // 2. POST: LƯU HỒ SƠ NHÂN VIÊN (CHỐNG LỖI ĐÈ NULL)
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateNhanVienInfo(AdminNhanVienEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Dữ liệu nhập vào chưa đúng định dạng!";
                return RedirectToAction("QuanLyNhanVien");
            }

            // 1. Tìm lại nhân viên gốc từ Database
            var nv = await _db.UserProfiles.OfType<NhanVien>().FirstOrDefaultAsync(u => u.UserID == model.UserID);
            if (nv == null)
            {
                TempData["Error"] = "Lỗi: Nhân viên này không tồn tại trên hệ thống.";
                return RedirectToAction("QuanLyNhanVien");
            }

            // 2. CẬP NHẬT CÓ CHỌN LỌC (SELECTIVE UPDATE)
            // Chỉ cập nhật nếu trường đó được nhập chữ (không rỗng và không toàn dấu cách)

            if (!string.IsNullOrWhiteSpace(model.HoTen)) nv.Hoten = model.HoTen.Trim();
            if (!string.IsNullOrWhiteSpace(model.SoDienThoai)) nv.sđt = model.SoDienThoai.Trim();
            if (!string.IsNullOrWhiteSpace(model.DiaChi)) nv.DiaChi = model.DiaChi.Trim();

            // Với lương (kiểu số), cập nhật nếu có giá trị (Vì null nghĩa là admin không sửa)
          

            // 3. Lưu vào DB
            try
            {
                await _db.SaveChangesAsync();
                TempData["Success"] = $"Đã cập nhật thành công hồ sơ của {nv.Hoten}!";
            }
            catch (Exception)
            {
                TempData["Error"] = "Lỗi hệ thống: Không thể lưu thay đổi vào Database.";
            }

            return RedirectToAction("QuanLyNhanVien");
        }

        // =========================================================
        // 3. KHÓA TÀI KHOẢN THAY VÌ XÓA
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> KhoaNhanVien(int id)
        {
            var nv = await _db.UserProfiles.OfType<NhanVien>()
                             .Include(u => u.Account)
                             .FirstOrDefaultAsync(u => u.UserID == id);

            if (nv != null && nv.Account != null)
            {
                // Chuyển thao tác XÓA thành thao tác KHÓA TÀI KHOẢN
                nv.Account.IsLocked = true;
                await _db.SaveChangesAsync();
                TempData["Success"] = $"Đã khóa quyền truy cập của nhân viên {nv.Hoten}.";
            }
            else
            {
                TempData["Error"] = "Không thể thực hiện. Có thể nhân viên này chưa có tài khoản hệ thống.";
            }

            return RedirectToAction("QuanLyNhanVien");
        }


        // 1. LẤY DANH SÁCH CA LÀM CỦA 1 NHÂN VIÊN ĐỂ ADMIN XEM
        [HttpGet]
        public async Task<IActionResult> GetStaffShiftsPartial(int id)
        {
            var nv = await _db.UserProfiles.FindAsync(id);
            if (nv == null) return NotFound();

            // Lấy tất cả các ca đang ở trạng thái Pending hoặc đã Approved của NV này
            var shifts = await _db.DangKyCaLams
                .Include(s => s.CaLam)
                .Where(s => s.MaNV == id)
                .OrderByDescending(s => s.NgayLam)
                .ToListAsync();

            ViewBag.StaffName = nv.Hoten;
            return PartialView("_StaffShiftsPartial", shifts);
        }

        // 2. XỬ LÝ DUYỆT CA (CHUYỂN SANG APPROVED)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveShift(int requestId) // Tên là requestId
        {
            var dangKy = await _db.DangKyCaLams.FindAsync(requestId);
            if (dangKy != null)
            {
                dangKy.TrangThai = ShiftStatus.Approved;
                await _db.SaveChangesAsync(); // Lưu vào DB

                return Json(new { success = true, message = "Đã phê duyệt ca làm thành công!" });
            }

            return Json(new { success = false, message = "Không tìm thấy dữ liệu ca làm!" });
        }

        private async Task<decimal> TinhLuongNhanVienAsync(int maNV, DateTime tuNgay, DateTime denNgay)
        {
            // 1. Đếm tổng số ca ĐÃ ĐƯỢC DUYỆT trong khoảng thời gian
            int soCa = await _db.DangKyCaLams
                .Where(c => c.MaNV == maNV
                         && c.TrangThai == ShiftStatus.Approved
                         && c.NgayLam.Date >= tuNgay.Date
                         && c.NgayLam.Date <= denNgay.Date)
                .CountAsync();

            // 2. Lấy đơn giá lương của nhân viên đó
            var nv = await _db.UserProfiles.OfType<NhanVien>().FirstOrDefaultAsync(u => u.UserID == maNV);
            decimal donGia = nv?.Luong ?? 400000; // Nếu null thì lấy 400k mặc định

            // 3. Nhân lên
            return soCa * donGia;
        }


        //Quản lý lương 
        [HttpGet]
        public async Task<IActionResult> QuanLyLuong(int? thang, int? nam)
        {
            

            int targetMonth = thang ?? DateTime.Now.Month;
            int targetYear = nam ?? DateTime.Now.Year;

            DateTime startOfMonth = new DateTime(targetYear, targetMonth, 1);
            DateTime endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            ViewBag.SelectedMonth = targetMonth;
            ViewBag.SelectedYear = targetYear;
            ViewBag.MonthPickerValue = $"{targetYear}-{targetMonth:D2}";

            var nhanViens = await _db.UserProfiles.OfType<NhanVien>().ToListAsync();

            // =============================================================
            // SỬA Ở ĐÂY: Trích xuất Tháng và Năm từ trường TuNgay
            // Lọc thêm điều kiện LoaiKyLuong == Monthly (Đề phòng sau này có thưởng/ứng)
            // =============================================================
            var bangLuongs = await _db.BangLuongs
                .Where(b => b.TuNgay.Month == targetMonth
                         && b.TuNgay.Year == targetYear
                         && b.LoaiKyLuong == PayrollType.Monthly)
                .ToListAsync();

            var caLams = await _db.DangKyCaLams
                .Where(c => c.TrangThai == ShiftStatus.Approved
                         && c.NgayLam >= startOfMonth && c.NgayLam <= endOfMonth)
                .ToListAsync();

            var payrollList = new List<PayrollListViewModel>();

            foreach (var nv in nhanViens)
            {
                var model = new PayrollListViewModel
                {
                    MaNV = nv.UserID,
                    TenNV = nv.Hoten,
                    Thang = targetMonth, // Vẫn truyền Tháng/Năm ra View bình thường
                    Nam = targetYear
                };

                var phieuChot = bangLuongs.FirstOrDefault(b => b.MaNV == nv.UserID);

                if (phieuChot != null)
                {
                    model.TongSoCa = phieuChot.TongSoCa;
                    model.TongTienFormatted = phieuChot.TongTien.ToString("N0") + " ₫";

                    // SỬA Ở ĐÂY: Dùng thuộc tính DaThanhToan của bạn
                    if (phieuChot.DaThanhToan)
                    {
                        model.StatusCode = 2;
                        model.TrangThaiText = "Đã Thanh Toán";
                        model.CssClassTrangThai = "status-success";
                    }
                    else
                    {
                        model.StatusCode = 1;
                        model.TrangThaiText = "Chờ Thanh Toán";
                        model.CssClassTrangThai = "status-warning";
                    }
                }
                else
                {
                    int soCaThucTe = caLams.Count(c => c.MaNV == nv.UserID);
                    decimal donGia = nv.Luong ?? 400000;

                    model.TongSoCa = soCaThucTe;
                    model.TongTienFormatted = (soCaThucTe * donGia).ToString("N0") + " ₫";
                    model.StatusCode = 0;
                    model.TrangThaiText = "Chưa Chốt";
                    model.CssClassTrangThai = "status-pending";
                }

                payrollList.Add(model);
            }

            return View(payrollList);
        }


        // ========================================================
        // 1. GET: LẤY DỮ LIỆU HIỂN THỊ LÊN MODAL PHIẾU LƯƠNG
        // ========================================================
        [HttpGet]
        public async Task<IActionResult> GetPayslipPartial(int maNV, int thang, int nam, string actionType)
        {
            var nv = await _db.UserProfiles.OfType<NhanVien>().FirstOrDefaultAsync(u => u.UserID == maNV);
            if (nv == null) return NotFound();

            DateTime startOfMonth = new DateTime(nam, thang, 1);
            DateTime endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            // Kéo danh sách ca làm thực tế trong tháng
            var caLams = await _db.DangKyCaLams
                .Include(c => c.CaLam)
                .Where(c => c.MaNV == maNV && c.TrangThai == ShiftStatus.Approved
                         && c.NgayLam >= startOfMonth && c.NgayLam <= endOfMonth)
                .OrderBy(c => c.NgayLam)
                .ToListAsync();

            decimal donGia = nv.Luong ?? 400000;

            var model = new PayslipViewModel
            {
                MaNV = maNV,
                TenNV = nv.Hoten,
                Thang = thang,
                Nam = nam,
                DonGia = donGia,
                TongSoCa = caLams.Count,
                TongTien = caLams.Count * donGia,
                ActionType = actionType,
                DanhSachCaLam = caLams
            };

            return PartialView("_PayslipDetailPartial", model);
        }

        // ========================================================
        // 2. POST: XÁC NHẬN CHỐT LƯƠNG VÀO DATABASE BẰNG AJAX
        // ========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XacNhanChotLuong(int maNV, int thang, int nam)
        {
            DateTime startOfMonth = new DateTime(nam, thang, 1);
            DateTime endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            // 1. Kiểm tra chống lỗi: Đã chốt rồi thì không chốt lại
            bool isExist = await _db.BangLuongs.AnyAsync(b => b.MaNV == maNV && b.TuNgay == startOfMonth && b.LoaiKyLuong == PayrollType.Monthly);
            if (isExist)
            {
                return Json(new { success = false, message = "Phiếu lương này đã được chốt từ trước!" });
            }

            // 2. Tính toán lại ở Server để bảo mật tuyệt đối (Không tin dữ liệu từ UI gửi lên)
            var nv = await _db.UserProfiles.OfType<NhanVien>().FirstOrDefaultAsync(u => u.UserID == maNV);
            int soCa = await _db.DangKyCaLams.CountAsync(c => c.MaNV == maNV && c.TrangThai == ShiftStatus.Approved && c.NgayLam >= startOfMonth && c.NgayLam <= endOfMonth);

            if (soCa == 0) return Json(new { success = false, message = "Nhân viên không làm ca nào, không thể chốt phiếu lương 0đ!" });

            decimal donGia = nv?.Luong ?? 400000;

            // 3. Tạo mới Bản ghi Bảng Lương
            var phieuLuong = new BangLuong
            {
                MaNV = maNV,
                TuNgay = startOfMonth,
                DenNgay = endOfMonth,
                TongSoCa = soCa,
                TongTien = soCa * donGia,
                LoaiKyLuong = PayrollType.Monthly,
                DaThanhToan = false, // Vừa chốt sổ nên chắc chắn chưa trả tiền
                NgayChotLuong = DateTime.Now
            };

            _db.BangLuongs.Add(phieuLuong);
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = "Đã chốt sổ lương thành công!" });
        }


        // ========================================================
        // 3. POST: XÁC NHẬN ĐÃ CHUYỂN KHOẢN (THANH TOÁN LƯƠNG)
        // ========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XacNhanThanhToan(int maNV, int thang, int nam)
        {
            // Tìm Phiếu lương của tháng đó (dựa vào ngày mùng 1 của tháng)
            DateTime startOfMonth = new DateTime(nam, thang, 1);

            var phieuLuong = await _db.BangLuongs.FirstOrDefaultAsync(b =>
                b.MaNV == maNV &&
                b.TuNgay == startOfMonth &&
                b.LoaiKyLuong == PayrollType.Monthly);

            if (phieuLuong == null)
            {
                return Json(new { success = false, message = "Lỗi: Không tìm thấy Phiếu lương chốt sổ của tháng này!" });
            }

            // NGHIỆP VỤ: Kiểm tra xem đã thanh toán chưa để tránh lỗi bấm đúp (Race Condition)
            if (phieuLuong.DaThanhToan)
            {
                return Json(new { success = false, message = "Phiếu lương này đã được thanh toán từ trước!" });
            }

            // ==========================================
            // THỰC THI NGHIỆP VỤ (LƯU VẾT)
            // ==========================================
            phieuLuong.DaThanhToan = true;
            phieuLuong.NgayThanhToan = DateTime.Now; // Ghi nhận thời điểm Kế toán báo hoàn thành

            try
            {
                await _db.SaveChangesAsync();
                return Json(new { success = true, message = $"Đã xác nhận thanh toán lương tháng {thang} cho nhân viên!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống khi cập nhật dữ liệu: " + ex.Message });
            }
        }   



        // 4. QUẢN LÝ HỆ THỐNG PHÒNG
        public async Task<IActionResult> QuanLyHeThong()
        {
            ViewData["ActiveMenu"] = "System";
            var phongs = await _db.Rooms.OrderBy(r => r.SoPhong).ToListAsync();
            return View(phongs);
        }

        // --- NGHIỆP VỤ PHÒNG ---

        // ========================================================
        // 1. GET: LẤY FORM THÊM HOẶC SỬA PHÒNG (TRẢ VỀ PARTIAL VIEW)
        // ========================================================
        [HttpGet]
        public async Task<IActionResult> GetRoomFormPartial(int? id)
        {
            if (id == null)
            {
                // TRƯỜNG HỢP: THÊM MỚI
                ViewBag.IsEdit = false;
                return PartialView("_RoomFormPartial", new Room());
            }
            else
            {
                // TRƯỜNG HỢP: SỬA PHÒNG
                var room = await _db.Rooms.FindAsync(id);
                if (room == null) return NotFound();

                if (room.TrangThai == RoomStatus.Occupied)
                {
                    return Content("<div style='padding:40px; text-align:center; color:red;'><i class='fas fa-ban fa-3x'></i><h4 class='mt-3'>Phòng đang có khách, không thể chỉnh sửa!</h4></div>");
                }

                ViewBag.IsEdit = true;
                return PartialView("_RoomFormPartial", room);
            }
        }

        // ========================================================
        // 2. POST: LƯU DỮ LIỆU (DÙNG CHUNG CHO CẢ THÊM VÀ SỬA)
        // ========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveRoom(Room model, bool isEdit)
        {
            // Bỏ qua lỗi ModelState.IsValid vì Form không có HinhAnh sẽ bị báo lỗi Required
            // Thay vào đó, chúng ta tự Validation thủ công cho chắc chắn.
            if (model.Size < 1 || model.Size > 5)
            {
                return Json(new { success = false, message = "Sức chứa phòng chỉ được phép từ 1 đến 5 người!" });
            }

            try
            {
                // Tự động tính giá ở Server để chống Hack F12
                decimal giaMoi = model.TinhGiaDeXuat();

                if (!isEdit)
                {
                    // =====================================
                    // LOGIC THÊM MỚI PHÒNG
                    // =====================================
                    bool isExist = await _db.Rooms.AnyAsync(r => r.SoPhong == model.SoPhong);
                    if (isExist) return Json(new { success = false, message = $"Số phòng {model.SoPhong} đã tồn tại!" });

                    model.GiaPhong = giaMoi;
                    model.TrangThai = RoomStatus.Available;

                    _db.Rooms.Add(model);
                    await _db.SaveChangesAsync();
                    return Json(new { success = true, message = $"Đã thêm phòng #{model.SoPhong} thành công!" });
                }
                else
                {
                    // =====================================
                    // LOGIC SỬA PHÒNG (SELECTIVE UPDATE CHUẨN)
                    // =====================================
                    var roomDb = await _db.Rooms.FindAsync(model.SoPhong);
                    if (roomDb == null) return Json(new { success = false, message = "Không tìm thấy phòng!" });

                    // 1. Chỉ cập nhật Loại Phòng nếu form có gửi lên
                    // Tuy enum không thể null, nhưng check cho an toàn nghiệp vụ
                    roomDb.LoaiPhong = model.LoaiPhong;

                    // 2. Chỉ cập nhật Size (đã qua kiểm tra 1 -> 5 ở trên)
                    roomDb.Size = model.Size;

                    // 3. Ghi đè Giá phòng (Đã được tính lại bởi C# thay vì dùng số lấy từ UI)
                    roomDb.GiaPhong = giaMoi;

                    // TUYỆT ĐỐI KHÔNG ĐỤNG CHẠM ĐẾN CÁC TRƯỜNG KHÁC!
                    // Ví dụ: KHÔNG VIẾT roomDb.HinhAnh = model.HinhAnh;
                    // Ví dụ: KHÔNG VIẾT roomDb.TrangThai = model.TrangThai;

                    await _db.SaveChangesAsync();
                    return Json(new { success = true, message = $"Đã cập nhật phòng #{model.SoPhong} thành công!" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi Database: " + (ex.InnerException?.Message ?? ex.Message) });
            }
        }

        // ========================================================
        // NGHIỆP VỤ XÓA MỀM / KHÓA PHÒNG
        // ========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaPhong(int id)
        {
            var phong = await _db.Rooms.FindAsync(id);
            if (phong == null) return Json(new { success = false, message = "Không tìm thấy phòng!" });

            // Chỉ chặn nếu phòng ĐANG ĐƯỢC ĐẶT hoặc ĐANG Ở. Nếu là phòng trống hoặc quá khứ thì vẫn cho Khóa.
            bool hasActiveBookings = await _db.Bookings.AnyAsync(b =>
                b.SoPhong == id &&
                (b.TrangThaiDat == BookingStatus.DangO || b.TrangThaiDat == BookingStatus.ChoXacNhan || b.TrangThaiDat == BookingStatus.DaXacNhan));

            if (hasActiveBookings)
            {
                return Json(new { success = false, message = "Phòng đang có khách ở hoặc sắp đến, không thể khóa!" });
            }

            // THỰC HIỆN XÓA MỀM (SOFT DELETE)
            // Cách 1 (Khuyên dùng): Nếu bạn thêm cột "public bool IsDeleted {get; set;}" vào model Room
            // phong.IsDeleted = true; 

            // Cách 2 (Dùng tạm): Đổi trạng thái sang Bảo trì để không hiện lên trang đặt phòng của khách nữa
            phong.TrangThai = RoomStatus.Maintenance;

            await _db.SaveChangesAsync();

            return Json(new { success = true, message = $"Đã NGỪNG HOẠT ĐỘNG phòng #{id}. Dữ liệu lịch sử vẫn được giữ nguyên." });
        }
        public async Task<IActionResult> MoKhoaPhong(int id)
        {
            try
            {
                var phong = await _db.Rooms.FindAsync(id);

                if (phong == null)
                    return Json(new { success = false, message = "Không tìm thấy phòng!" });

                if (phong.TrangThai != RoomStatus.Maintenance)
                {
                    return Json(new { success = false, message = "Phòng này không ở trạng thái Bảo trì!" });
                }

                // Chuyển trạng thái về Sẵn Sàng (Trống)
                phong.TrangThai = RoomStatus.Available;

                await _db.SaveChangesAsync();

                return Json(new { success = true, message = $"Đã mở khóa phòng #{id}. Phòng sẵn sàng đón khách!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }
        // 5. XÓA NHÂN VIÊN (Và tài khoản đi kèm)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaNhanVien(int id)
        {
            var nv = await _db.UserProfiles.OfType<NhanVien>()
                             .Include(u => u.Account)
                             .FirstOrDefaultAsync(u => u.UserID == id);

            if (nv != null)
            {
                // Nếu xóa nhân viên, ta nên xóa luôn tài khoản Login của họ để tránh rác DB
                if (nv.Account != null) _db.Accounts.Remove(nv.Account);
                
                _db.NhanViens.Remove(nv);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Đã xóa nhân viên và tài khoản liên quan thành công!";
            }
            return RedirectToAction("QuanLyNhanVien");
        }
    }
}