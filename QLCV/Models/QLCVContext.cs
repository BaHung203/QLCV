using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using QLCV.Models;
using WebApp.Models;
using WebApp.Libs;

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
        public DbSet<Permission> Permission { get; set; }
        public DbSet<NhanVienPermission> NhanVienPermission { get; set; }
        public DbSet<XuLyCongVan> XuLyCongVan { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ===== Quan hệ nhanVien - phongBan =====
            modelBuilder.Entity<nhanVien>()
                .HasOne(nv => nv.PhongBan)
                .WithMany(pb => pb.nhanVien)
                .HasForeignKey(nv => nv.IdPhongBan)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            // ===== Chuyển enum UserRole sang string =====
            modelBuilder.Entity<Account>()
                .Property(a => a.Role)
                .HasConversion(new EnumToStringConverter<UserRole>());

            // ===== Many-to-Many: NhanVien <-> Permission =====
            modelBuilder.Entity<NhanVienPermission>()
                .HasKey(np => new { np.NhanVienId, np.PermissionId });

            modelBuilder.Entity<NhanVienPermission>()
                .HasOne(np => np.NhanVien)
                .WithMany(nv => nv.NhanVienPermission) // Sửa: phải trùng với collection plural
                .HasForeignKey(np => np.NhanVienId);

            modelBuilder.Entity<NhanVienPermission>()
                .HasOne(np => np.Permission)
                .WithMany()
                .HasForeignKey(np => np.PermissionId);
            modelBuilder.Entity<XuLyCongVan>()
                .Property(x => x.TrangThai)
                .HasConversion<string>();


            base.OnModelCreating(modelBuilder);
        }
    }
}
