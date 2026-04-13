using PBL3_Hotel_System.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PBL3_Hotel_System_.Models.UserModels
{
    public abstract class BaseUser
    {
        [Key]
        public int UserID { get; set; }
        public string Hoten {  get; set; }
        public string sđt { get; set; }
        public abstract string RoleName { get; }

        public int AccountID { get; set; }

        [ForeignKey("AccountID")]
        public virtual Account Account { get; set; }
    }
}
