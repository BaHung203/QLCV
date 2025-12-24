using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApp.Libs;

namespace WebApp.Models
{
    [Table("CongVan")]
    public class congVan
    {
        [Key]
        public int ID { get; set; }
        [Required(ErrorMessage = "Số hiệu công văn không được để trống")]
        [StringLength(50, ErrorMessage = "Số hiệu tối đa 50 ký tự")]
        public string SoHieu { get; set; } = null!;

        public DateTime Ngay { get; set; }

        [StringLength(10)]
        public LoaiCongVan LoaiCongVan { get; set; } 

        [ForeignKey("NoiPhatHanh")]
        public int? IdNoiPhatHanh { get; set; }
        public NoiPhatHanh? NoiPhatHanh { get; set; }

        [ForeignKey("NoiNhan")]
        public int? IdNoiNhan { get; set; }
        public NoiNhan? NoiNhan { get; set; }

        public string? ViTri { get; set; }

        public string? NoiDung { get; set; } 

        [StringLength(255)]
        public string? TepDinhKem { get; set; }
        public string? NoiDungTep { get; set; }
         // trong nhanVien
        public virtual ICollection<XuLyCongVan> XuLyCongVan { get; set; } = new List<XuLyCongVan>();

    }
}
