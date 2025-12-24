using System;
using System.ComponentModel.DataAnnotations;

namespace WebApp.Models
{
    public class nhanVien
    {
        [Key]
        public int IdNhanVien { get; set; }
        public string? HoTen { get; set; }
        public DateTime NgaySinh { get; set; }
        public string? GioiTinh { get; set; }
        public string? SoDienThoai { get; set; }

        [EmailAddress, StringLength(100)]
        public string? Email { get; set; }
        public string? ChucVu { get; set; }

        public int? IdPhongBan { get; set; }

         // Navigation property
        public virtual  phongBan? PhongBan { get; set; } 
        public virtual ICollection<NhanVienPermission> NhanVienPermission { get; set; } = new List<NhanVienPermission>();
         // Navigation property đến Account (nếu có)
        public virtual Account? Account { get; set; }
        // ở cuối class congVan, thêm:
        public virtual ICollection<XuLyCongVan> XuLyLichSu { get; set; } = new List<XuLyCongVan>();

    }
}
