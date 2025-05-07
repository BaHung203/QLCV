namespace WebApp.ModelUI
{
    public class CongVanDenCreateModel
    {
        public int ID { get; set; }
        public string? SoHieu { get; set; }
        public DateTime NgayDen { get; set; }
        public int IdNoiPhatHanh { get; set; }
        public int IdLoaiCongVan { get; set; }
        
        public string? Vitri { get; set; }
        public string? NoiDung { get; set; }
        public string? TepDinhKem { get; set; }
        public string? NoiDungTep { get; set; }

        public IFormFile? File {get;set;}
        
    }
}