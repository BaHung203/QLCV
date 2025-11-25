using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Models
{
    [Table("ThongBao")]
    public class ThongBao
    {
        [Key]
        public int IdThongBao { get; set; }

        [Required]
        [StringLength(255)]
        public string TieuDe { get; set; } = string.Empty;

        public string? NoiDung { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime NgayTao { get; set; } = DateTime.Now;

        public bool DaXem { get; set; } = false;

        // 🔗 Khóa ngoại tới bảng Công Văn (tùy chọn)
        public int? IdCongVan { get; set; }

        [ForeignKey("IdCongVan")]
        public congVan? CongVan { get; set; }
    }
}
