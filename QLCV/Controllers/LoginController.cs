using Microsoft.AspNetCore.Mvc;
using WebApp.Data; // namespace của DbContext
using WebApp.Models; // namespace chứa class Account
using Microsoft.EntityFrameworkCore;

namespace WebApp.Controllers
{
    public class LoginController : Controller
    {
        private readonly AppDbContext _context;

        public LoginController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Login/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Login/Login
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            
            var user = await _context.Accounts
                .FirstOrDefaultAsync(a => a.Username == username && a.Password == password);

            if (user != null)
            {
                HttpContext.Session.SetString("Username", user.Username);
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không đúng.";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Login");
        }
    }
}
