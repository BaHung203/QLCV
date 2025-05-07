using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Models
{
    public class CongVanDi
    {
        public int ID { get; set; }
        public string? SoHieu { get; set; }
        public DateTime NgayDi { get; set; }
        
        [ForeignKey("NoiNhan")]
        [Display(Name = "Nơi nhận")]
        public int IdNoiNhan { get; set; }
        
        [ForeignKey("PhanLoaiCongVan")]
        [Display(Name = "Loại công văn")]
        public int IdLoaiCongVan { get; set; }
        
        public string? Vitri { get; set; }
        public string? NoiDung { get; set; }
        public string? TepDinhKem { get; set; }
        public string? NoiDungTep { get; set; }


        public virtual NoiNhan? NoiNhan { get; set; }
        public virtual PhanLoaiCongVan? PhanLoaiCongVan { get; set; }
    }
}