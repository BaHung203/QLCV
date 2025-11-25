using System;
using System.ComponentModel.DataAnnotations;

namespace WebApp.Models
{
    public class NoiNhan
{
    [Key]
    public int ID { get; set; }
    public string? TenNoiNhan { get; set; }
}
}
