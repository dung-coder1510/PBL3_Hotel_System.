using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PBL3_Hotel_System.Data;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
namespace PBL3_Hotel_System.Controllers
{
    [Authorize]
    public class KhachHangController(HotelDbContext _context) : Controller
    {
        [Authorize]   
        public IActionResult Index()
        {
            return View();
        }
        
    }
}
