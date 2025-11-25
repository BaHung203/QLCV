using Microsoft.EntityFrameworkCore;
using QLCV.Models;
using WebApp.Models;

namespace WebApp.Data
{
    public class AppDbContext : DbContext
    {


        public AppDbContext()
        {
        }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<congVan> CongVan { get; set; }
        public DbSet<NoiPhatHanh> NoiPhatHanh { get; set; }
        public DbSet<NoiNhan> NoiNhan { get; set; }
        public DbSet<phongBan> phongBan { get; set; }
        public DbSet<nhanVien> nhanVien { get; set; }
        public DbSet<ThongBao> ThongBao { get; set; }
        public DbSet<Account> Accounts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<nhanVien>()
                .HasOne(nv => nv.PhongBan)
                .WithMany(pb => pb.nhanVien)  // Khớp với tên mới
                .HasForeignKey(nv => nv.IdPhongBan)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            base.OnModelCreating(modelBuilder);
        }
        // Thêm các DbSet khác nếu có
    }
}
