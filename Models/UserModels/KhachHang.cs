namespace PBL3_Hotel_System_.Models.UserModels
{
    public class KhachHang : BaseUser
    {
        public override string RoleName => "KhachHang";
        public string MemberRank {  get; set; }
        public int DiemTichLuy {  get; set; }
    }
}
