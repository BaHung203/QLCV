using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Models
{
    [Table("NhanVienPermission", Schema = "dbo")] 
    public class NhanVienPermission
    {
        public int NhanVienId { get; set; }
        public nhanVien? NhanVien { get; set; } = null!;

        public int PermissionId { get; set; }
        public Permission Permission { get; set; } = null!;
    }

}
