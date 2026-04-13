using System.ComponentModel.DataAnnotations;

namespace PBL3_Hotel_System.ViewModels
{
    public class BookViewModel
    {
        public int SoPhong { get; set; }
        public string? LoaiPhong { get; set; }
        public decimal GiaPhong { get; set; }
        public string? HinhAnh { get; set; }
        public int Size { get; set; }

        // --- Dữ liệu dùng để Post về Server (Hứng từ Form) ---
        [Required(ErrorMessage = "Vui lòng chọn ngày nhận phòng")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Date)]
        public DateTime CheckIn { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Vui lòng chọn ngày trả phòng")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Date)]
        public DateTime CheckOut { get; set; } = DateTime.Now.AddDays(1);

        [StringLength(500, ErrorMessage = "Ghi chú không được quá 500 ký tự")]
        public string? GhiChu { get; set; }
    }
}
