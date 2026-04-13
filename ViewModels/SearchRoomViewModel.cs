using Microsoft.AspNetCore.Mvc.Rendering;
using PBL3_Hotel_System.Models;
using System.ComponentModel.DataAnnotations;

namespace PBL3_Hotel_System.ViewModels
{
    public class SearchRoomViewModel
    {
        public DateTime NgayNhanPhong {  get; set; }

        public DateTime NgayTraPhong { get; set; }
        [Display(Name = "Trạng thái")]
        public string trangthai { get; set; }

        [Display(Name = "Loại phòng")]
        public string loaiphong {  get; set; }

        [Display(Name = "Số người ở")]
        public int Size {  get; set; }

    }
}
