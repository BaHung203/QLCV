using Microsoft.EntityFrameworkCore;
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

        public DbSet<CongVanDen> CongVanDen { get; set; }
        public DbSet<CongVanDi> CongVanDi { get; set; }
        public DbSet<PhanLoaiCongVan> PhanLoaiCongVan { get; set; }
        public DbSet<NoiPhatHanh> NoiPhatHanh { get; set; }
        public DbSet<NoiNhan> NoiNhan { get; set; }
        public DbSet<Account> Accounts { get; set; }

        // Thêm các DbSet khác nếu có
    }
}
