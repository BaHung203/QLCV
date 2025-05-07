using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApp.Models
{
    public class NoiPhatHanh
    {
        [Key]
        public int IdNoiPhatHanh { get; set; }

        public string? TenNoiPhatHanh { get; set; }

        // // Điều hướng
        // public ICollection<CongVanDen>? CongVanDen { get; set; }
        // public ICollection<CongVanDi>? CongVanDi { get; set; }
    }
}
