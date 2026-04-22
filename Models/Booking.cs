using PBL3_Hotel_System_.Models.UserModels;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace PBL3_Hotel_System.Models
{
    public enum BookingStatus
    {
        [Display(Name = "Chờ xác nhận")]
        ChoXacNhan, 
        
        [Display(Name = "Đã xác nhận")]
        DaXacNhan, 
        
        [Display(Name = "Đã hoàn thành")]
        DaHoanThanh, 
        
        [Display(Name = "Đã hủy")]
        DaHuy
    }
    public class Booking

    {
        [Key]
        public int BookingID { get; set; }

        [ForeignKey("MaKhachHang")]
        public virtual KhachHang kh { get; set; }
        public int MaKhachHang {  get; set; }
        [Required]
        public int SoPhong { get; set; } // Khóa ngoại liên kết tới Room.SoPhong

        [ForeignKey("SoPhong")]
        public virtual Room Room { get; set; } // Thuộc tính điều hướng ngược lại Room

        [Required]
        public DateTime CheckIn { get; set; }

        [Required]
        public DateTime CheckOut { get; set; }

        [Required]
        public DateTime NgayDat { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal GiaLucDat { get; set; }  
        public BookingStatus TrangThaiDat { get; set; } // Ví dụ: "Đã xác nhận", "Đã hủy"

        [MaxLength(500)] 
        [Display(Name = "Yêu cầu đặc biệt")]
        public string? GhiChu { get; set; } // Dấu ? cho phép Null vì không phải ai cũng có ghi chú
    }
}
