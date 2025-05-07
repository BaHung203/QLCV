namespace WebApp.ModelUI
{
    public class CongVanDiDetailModel
    {
        public int ID { get; set; }
        public string? SoHieu { get; set; }
        public DateTime NgayDi { get; set; }
        public int IdNoiNhan { get; set; }
        public int IdLoaiCongVan { get; set; }
        
        public string? Vitri { get; set; }
        public string? NoiDung { get; set; }
        public string? TepDinhKem { get; set; }
        public string? NoiDungTep { get; set; }
        public byte[]? file {get; set;}

        
    }
}