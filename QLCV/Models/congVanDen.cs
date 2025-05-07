using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Models
{
    public class CongVanDen
{
    public int ID { get; set; }
    public string? SoHieu { get; set; }
    public DateTime NgayDen { get; set; }

    [ForeignKey("NoiPhatHanh")]
    [Display(Name = "Nơi phát hành")]
    public int IdNoiPhatHanh { get; set; }
    public int IdLoaiCongVan { get; set; }
    public string? Vitri { get; set; }
    public string? NoiDung { get; set; }
    public string? TepDinhKem { get; set; }
    public string? NoiDungTep { get; set; }


    // public virtual NoiPhatHanh? NoiPhatHanh { get; set; }
    // public virtual PhanLoaiCongVan? PhanLoaiCongVan { get; set; }
}
}
