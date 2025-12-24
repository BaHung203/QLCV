using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApp.Libs;

namespace WebApp.Models
{
    public class XuLyCongVan
    {
        [Key]
        public int IdXuLy { get; set; }

        [ForeignKey("CongVan")]
        public int IdCongVan { get; set; }
        public congVan CongVan { get; set; } = null!;

        [ForeignKey("NhanVien")]
        public int IdNhanVien { get; set; }
        public nhanVien NhanVien { get; set; } = null!;

        public DateTime NgayXuLy { get; set; } = DateTime.Now;

        public TrangThaiXuLy TrangThai { get; set; } = TrangThaiXuLy.DangCho;

        public string? GhiChu { get; set; }

    }
}
