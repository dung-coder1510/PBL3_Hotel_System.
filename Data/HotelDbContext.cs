using Microsoft.EntityFrameworkCore;
using PBL3_Hotel_System.Models;
using PBL3_Hotel_System.Models;
using PBL3_Hotel_System_.Models.UserModels;


namespace PBL3_Hotel_System.Data // Sửa từ .Models thành .Data
{
    public class HotelDbContext : DbContext
    {
        public HotelDbContext(DbContextOptions<HotelDbContext> options) : base(options) { }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<BaseUser> UserProfiles { get; set; }

        // 2. Quan trọng nhất: PHẢI khai báo các lớp con cụ thể ở đây
        public DbSet<KhachHang> KhachHangs { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //thiết lập quan hệ 1-1 giữa User và Account
             modelBuilder.Entity<Account>()
            .HasOne(a => a.UserProfile)         // 1. Account có một UserProfile
            .WithOne(u => u.Account)            // 2. UserProfile đó thuộc về một Account duy nhất
            .HasForeignKey<BaseUser>(u => u.AccountID) // 3. Chỉ định BaseUser là bên giữ Khóa ngoại
            .OnDelete(DeleteBehavior.Cascade);


            // Giữ cấu hình cho Account cũ
            modelBuilder.Entity<Account>()
                .Property(a => a.Role)
                .HasConversion<string>();

            // Cấu hình mới cho Room.LoaiPhong
            modelBuilder.Entity<Room>()
                .Property(r => r.LoaiPhong)
                .HasConversion<string>();

            // Cấu hình mới cho Room.TrangThai
            modelBuilder.Entity<Room>()
                .Property(r => r.TrangThai)
                .HasConversion<string>();

            modelBuilder.Entity<Booking>()
                .Property(b => b.TrangThaiDat)
                .HasConversion<string>();
        }
    }
}