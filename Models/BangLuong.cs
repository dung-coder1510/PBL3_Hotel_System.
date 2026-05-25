using PBL3_Hotel_System.Models.UserModels;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PBL3_Hotel_System.Models
{
    public enum PayrollType
    {
        Monthly,    // Lương chốt theo tháng
        Weekly,     // Lương chốt theo tuần (nếu sau này cần)
        Bonus,      // Tiền thưởng
        Advance     // Ứng trước (Khấu trừ)
    }

    public class BangLuong
    {
        [Key]
        public int MaLuong { get; set; }

        public int MaNV { get; set; }
        [ForeignKey("MaNV")]
        public virtual NhanVien NhanVien { get; set; }

        public DateTime TuNgay { get; set; }
        public DateTime DenNgay { get; set; }

        // Tổng số ca đã làm trong khoảng thời gian trên
        public int TongSoCa { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TongTien { get; set; }

        public PayrollType LoaiKyLuong { get; set; } // "Monthly", "Weekly", "Bonus"

        public bool DaThanhToan { get; set; } = false; // Check xem kế toán đã ck chưa

        public DateTime NgayChotLuong { get; set; } = DateTime.Now;
        public DateTime? NgayThanhToan { get; set; }
    }
}
