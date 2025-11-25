using Microsoft.AspNetCore.Mvc;
using WebApp.Data;
using WebApp.Models;
using WebApp.ModelUI;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;
using iText.Kernel.Geom;

namespace WebApp.Controllers
{
    public class LoginController : Controller
    {
        private readonly AppDbContext _context;

        public LoginController(AppDbContext context)
        {
            _context = context;
        }
       public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            int totalItems = await _context.Accounts.CountAsync();

            var accounts = await _context.Accounts
                .OrderBy(a => a.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var model = new PagedResult<Account>()
            {
                Items = accounts,
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize
            };

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var ac = _context.Accounts.Find(id);
            if (ac == null) return NotFound();
            _context.Accounts.Remove(ac);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
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
                HttpContext.Session.SetString("Role", user.Role);

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không đúng.";
            return View();
        }

        // GET: /Login/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Login/Register
        [HttpPost]
        public async Task<IActionResult> Register(string username, string password, string email, string role = "User")
        {
            var exists = await _context.Accounts.AnyAsync(a => a.Username == username);
            if (exists)
            {
                ViewBag.Error = "Tên đăng nhập đã tồn tại.";
                return View();
            }

            var account = new Account
            {
                Username = username,
                Password = password,
                Email = email,
                Role = role
            };

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            return RedirectToAction("Login");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Login");
        }
       
    }

    // Middleware kiểm tra quyền theo Role
    public class AuthorizeRoleAttribute : TypeFilterAttribute
    {
        public AuthorizeRoleAttribute(string role) : base(typeof(AuthorizeRoleFilter))
        {
            Arguments = new object[] { role };
        }
    }

    public class AuthorizeRoleFilter : IAuthorizationFilter
    {
        private readonly string _role;
        public AuthorizeRoleFilter(string role)
        {
            _role = role;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var userRole = context.HttpContext.Session.GetString("Role");
            if (userRole == null || userRole != _role)
            {
                context.Result = new RedirectToActionResult("Login", "Login", null);
            }
        }
    }
    
}