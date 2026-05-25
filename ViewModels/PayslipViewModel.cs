using PBL3_Hotel_System.Models;

namespace PBL3_Hotel_System.ViewModels
{
    public class PayslipViewModel
    {
        public int MaNV { get; set; }
        public string TenNV { get; set; }
        public int Thang { get; set; }
        public int Nam { get; set; }

        public decimal DonGia { get; set; }
        public int TongSoCa { get; set; }
        public decimal TongTien { get; set; }

        public string ActionType { get; set; } // 'chot', 'pay', 'view'

        // Danh sách chi tiết các ca làm để kế toán đối chiếu
        public List<DangKiCaLam> DanhSachCaLam { get; set; } = new List<DangKiCaLam>();
    }
}