using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApp.Models
{
    public class PhanLoaiCongVan
    {
        [Key]
        public int IdPhanLoai { get; set; }

        public string? TenPhanLoai { get; set; }

        // // Điều hướng
        // public ICollection<CongVanDen> CongVanDen { get; set; }
        // public ICollection<CongVanDi> CongVanDi { get; set; }
    }
}
