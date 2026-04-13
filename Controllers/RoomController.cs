using Microsoft.AspNetCore.Mvc;
using PBL3_Hotel_System.Data;

namespace PBL3_Hotel_System.Controllers
{
    public class RoomController(HotelDbContext _context) : Controller
    {
    

        [HttpGet]
        public IActionResult Detail(int id) // Chữ 'id' này hứng dữ liệu từ asp-route-id
        {
            // Vào Database tìm đúng cái phòng có ID đó
            var roomDetail = _context.Rooms.FirstOrDefault(r => r.SoPhong == id);

            // Nếu ai đó gõ bậy ID lên URL mà không tìm thấy phòng -> Báo lỗi 404
            if (roomDetail == null)
            {
                return Content(
                   "<div style='text-align: center; padding: 50px; color: #dc3545;'>" +
                       "<i class='fas fa-exclamation-triangle' style='font-size: 4rem; margin-bottom: 20px; opacity: 0.5;'></i>" +
                       "<h3 style='font-family: var(--font-heading);'>Không tìm thấy phòng!</h3>" +
                       "<p style='color: #666;'>Phòng này có thể đã bị xóa hoặc đang bảo trì.</p>" +
                       "<button class='btn btn-book' onclick='closeRoomModal()' style='margin-top: 20px;'>Đóng lại</button>" +
                   "</div>",
                   "text/html" // Báo cho trình duyệt biết đây là mã HTML
                );
            }

            // Có phòng rồi thì ném toàn bộ dữ liệu (roomDetail) sang trang View
            return PartialView("_RoomDetail", roomDetail);
        }   
        public IActionResult BookingView()
        {
            return View();
        }
    }
}
