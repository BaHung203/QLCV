using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApp.Models
{
    public class phongBan
    {
        [Key]
        public int IdPhongBan { get; set; }
        public string? TenPhongBan { get; set; }

        public int? IdTruongPhong { get; set; }

        public string? SoDienThoai { get; set; }

        // Quan hệ 1-n: Một phòng ban có nhiều nhân viên
        public virtual  ICollection<nhanVien>? nhanVien { get; set; }
    }
}
