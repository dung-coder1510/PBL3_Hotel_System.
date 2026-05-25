namespace PBL3_Hotel_System.Models.UserModels
{
    public class NhanVien : BaseUser
    {
        public override string RoleName => "NhanVien";
        public decimal? Luong { get; set; } 
    }
}
