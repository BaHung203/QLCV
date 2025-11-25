using WebApp.Libs;

namespace WebApp.ModelUI
{
    public class CongVanCreateModel
    {
        public int ID { get; set; }
        public string? SoHieu { get; set; }
        public DateTime Ngay { get; set; }
        public int? IdNoiNhan { get; set; }
        public int? IdNoiPhatHanh { get; set; }
        // public NoiPhatHanh? NoiPhatHanh { get; set; }

        public LoaiCongVan LoaiCongVan { get; set; }

        public string? ViTri { get; set; }
        public string? NoiDung { get; set; }
        public string? TepDinhKem { get; set; }
        public string? NoiDungTep { get; set; }

        public IFormFile? File { get; set; }

    }
}