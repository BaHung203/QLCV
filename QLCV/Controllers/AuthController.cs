using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using WebApp.Data;

namespace WebApp.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        // ===== GET: Login =====
        public IActionResult Login()
        {
            return View();
        }

        // ===== POST: Login =====
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin";
                return View();
            }

            var acc = await _context.Accounts
                .Include(a => a.NhanVien)
                    .ThenInclude(nv => nv.NhanVienPermission)
                        .ThenInclude(np => np.Permission)
                .FirstOrDefaultAsync(a => a.Username == username);

            if (acc == null || acc.Password != password)
            {
                ViewBag.Error = "Sai tài khoản hoặc mật khẩu";
                return View();
            }

            // ===== Lưu session =====
            HttpContext.Session.SetInt32("AccountId", acc.Id);
            HttpContext.Session.SetString("Username", acc.Username);

            if (acc.IdNhanVien != null)
                HttpContext.Session.SetInt32("NhanVienId", acc.IdNhanVien.Value);

            // ===== Lấy permission =====
            var permissions = acc.NhanVien != null
                ? acc.NhanVien.NhanVienPermission
                    .Select(p => p.Permission.Name)
                    .Distinct()
                    .ToList()
                : new List<string>();

            HttpContext.Session.SetString(
                "Permissions",
                JsonSerializer.Serialize(permissions)
            );

            return RedirectToAction("Index", "Home");
        }

        // ===== Logout =====
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
