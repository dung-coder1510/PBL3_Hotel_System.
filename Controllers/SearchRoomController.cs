using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PBL3_Hotel_System.Data;
using PBL3_Hotel_System.ViewModels;
namespace PBL3_Hotel_System.Controllers
{
    public class SearchRoomController(HotelDbContext _context) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Search(SearchRoomViewModel model)
        {
            // Bắt đầu truy vấn từ bảng Rooms
            var query = _context.Rooms.AsQueryable();

            // 1. Lọc theo Loại phòng (loaiphong trong ViewModel)
            if (!string.IsNullOrEmpty(model.loaiphong) && model.loaiphong != "All")
            {
                query = query.Where(r => r.LoaiPhong.ToString() == model.loaiphong);
            }

            // 2. Lọc theo Sức chứa (Size trong ViewModel)
            // Lấy những phòng có sức chứa lớn hơn hoặc bằng số người khách nhập
            if (model.Size > 0)
            {
                query = query.Where(r => r.Size >= model.Size);
            }

            // 3. Lọc theo Ngày nhận và Ngày trả (Quan trọng nhất)
            // Logic: Tìm những phòng mà KHÔNG CÓ bất kỳ lịch đặt nào bị trùng (overlap)
            if (model.NgayNhanPhong != default && model.NgayTraPhong != default)
            {
                DateTime checkIn = model.NgayNhanPhong;
                DateTime checkOut = model.NgayTraPhong;

                // Sử dụng thuộc tính điều hướng r.Bookings để kiểm tra
                query = query.Where(r => !r.Bookings.Any(b =>
                    b.CheckIn < checkOut && b.CheckOut > checkIn
                ));
            }

            // Thực thi truy vấn và lấy danh sách phòng trống
            var availableRooms = await query.ToListAsync();


 

            // Trả về View Index kèm theo danh sách kết quả
            return PartialView("_RoomResults", availableRooms);
        }
    }
}
