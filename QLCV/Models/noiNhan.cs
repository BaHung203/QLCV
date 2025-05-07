using System;
using System.ComponentModel.DataAnnotations;

namespace WebApp.Models
{
    public class NoiNhan
{
    [Key]
    public int IdNoiNhan { get; set; }
    public string? TenNoiNhan { get; set; }
}
}
