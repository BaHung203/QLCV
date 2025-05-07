using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Models
{
    public class Account

    {
        [Key]
        public int Id {get; set;}
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        public string? Username { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
    
        public string? Password { get; set; } = null!;
    }
}
