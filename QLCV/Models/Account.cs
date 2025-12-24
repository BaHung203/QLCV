using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApp.Libs;

namespace WebApp.Models
{
    public class Account

    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        public string? Username { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]

        public string? Password { get; set; } = null!;
         public string? Email { get; set; } 
        public int? IdNhanVien { get; set; }

        [ForeignKey("IdNhanVien")]
        public virtual nhanVien? NhanVien { get; set; }

        // Quyền [Required]
        [Column(TypeName = "nvarchar(50)")]
        public UserRole Role { get; set; } = UserRole.Admin;


        }
}
