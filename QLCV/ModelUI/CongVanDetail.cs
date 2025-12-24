using WebApp.Libs;
using WebApp.Models;


namespace WebApp.ModelUI
{
    public class CongVanDetailModel
    {
        public int ID { get; set; }
        public string? SoHieu { get; set; }
        public DateTime Ngay { get; set; }
        public int? IdNoiNhan { get; set; }
        public string? NoiNhan { get; set; }
        public int? IdNoiPhatHanh { get; set; }

        public string? NoiPhatHanh { get; set; }

        public LoaiCongVan LoaiCongVan { get; set; }

        public string? ViTri { get; set; }
        public string? NoiDung { get; set; }
        public string? TepDinhKem { get; set; }
        public string? NoiDungTep { get; set; }
        public byte[]? File { get; set; }
        
        public virtual ICollection<XuLyCongVan> XuLyCongVan { get; set; } = new List<XuLyCongVan>();
        public string? TrangThai { get; set; }

    }
}