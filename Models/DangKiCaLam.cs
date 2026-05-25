using PBL3_Hotel_System.Models.UserModels;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PBL3_Hotel_System.Models
{
    public enum ShiftStatus
    {
        Pending = 0,    // Chờ duyệt
        Approved = 1,   // Đã duyệt (Dùng để tính lương)
        Rejected = 2    // Từ chối
    }
    public class DangKiCaLam
    {
        [Key]
        [Column("MaDK")]
        public int MaDK { get; set; }

        [Required]
        [Column("MaNV")]
        public int MaNV { get; set; }

        [Required]
        [Column("MaCa")]
        public int MaCa { get; set; }

        [Required]
        [Column("NgayLam")]
        [Display(Name = "Ngày làm")]
        [DataType(DataType.Date)]
        public DateTime NgayLam { get; set; }

        [Column("TrangThai")]
        [StringLength(20)]
        public ShiftStatus TrangThai { get; set; } = ShiftStatus.Pending;

        [StringLength(200)]
        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }

        [ForeignKey("MaNV")]
        public virtual NhanVien? NhanVien { get; set; }

        [ForeignKey("MaCa")]
        public virtual CaLam? CaLam { get; set; }
    }
}
