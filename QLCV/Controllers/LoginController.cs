using Microsoft.AspNetCore.Mvc;
using WebApp.Data;
using WebApp.Models;
using WebApp.ModelUI;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Filters;
using WebApp.Libs;

namespace WebApp.Controllers
{
    public class LoginController : Controller
    {
        private readonly AppDbContext _context;

        public LoginController(AppDbContext context)
        {
            _context = context;
        }

       
        public async Task<IActionResult> Index(string keyword, int page = 1, int pageSize = 10)
        {
            keyword = keyword?.Trim().ToLower() ?? string.Empty;

            var query = _context.Accounts
                .Include(x => x.NhanVien)
                .AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(c =>
                    c.Username.ToLower().Contains(keyword) ||
                    (c.Email ?? "").ToLower().Contains(keyword) ||
                    c.Role.ToString().ToLower().Contains(keyword)
                );
            }

            int totalItems = await query.CountAsync();

            var accounts = await query
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

      
        public async Task<IActionResult> Edit(int id)
        {
            var acc = await _context.Accounts.FindAsync(id);
            if (acc == null) return NotFound();
            return View(acc);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Account model)
        {
            var acc = await _context.Accounts.FindAsync(model.Id);
            if (acc == null) return NotFound();

            acc.Username = model.Username;
            acc.Email = model.Email;
            acc.Role = model.Role;

            if (!string.IsNullOrEmpty(model.Password))
                acc.Password = model.Password;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Cập nhật tài khoản thành công!";
            return RedirectToAction("Index");
        }

     
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var ac = await _context.Accounts.FindAsync(id);
            if (ac == null) return NotFound();

            _context.Accounts.Remove(ac);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

     
        public IActionResult Login()
        {
            return View();
        }

        
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin.";
                return View();
            }

            var user = await _context.Accounts
                .Include(a => a.NhanVien)
                    .ThenInclude(nv => nv.NhanVienPermission)
                        .ThenInclude(np => np.Permission)
                .FirstOrDefaultAsync(a => a.Username == username && a.Password == password);

            if (user == null)
            {
                ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không đúng.";
                return View();
            }

            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("Role", user.Role.ToString());
            HttpContext.Session.SetInt32("AccountId", user.Id);

            if (user.IdNhanVien != null)
                HttpContext.Session.SetInt32("IdNhanVien", user.IdNhanVien.Value);

            List<string> permissionNames = new List<string>();

            // Admin = full quyền
            if (user.Role == UserRole.Admin)
            {
                permissionNames = await _context.Permission
                    .Select(p => p.Name)
                    .ToListAsync();
            }
            else
            {
                // Nhân viên có quyền riêng
                if (user.NhanVien != null)
                {
                    permissionNames = user.NhanVien.NhanVienPermission
                        .Select(p => p.Permission.Name)
                        .Distinct()
                        .ToList();
                }
            }

            // Save permission vào session
            HttpContext.Session.SetString(
                "Permissions",
                System.Text.Json.JsonSerializer.Serialize(permissionNames)
            );

            return RedirectToAction("Index", "Home");
        }

    
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string username, string password, string email, UserRole role)
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
                Role = role,
                IdNhanVien = null
            };

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đăng ký thành công!";
            return RedirectToAction("Login");
        }

       
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        
        public async Task<IActionResult> PhanQuyen()
        {
            var role = HttpContext.Session.GetString("Role");

            if (role != "Admin")
            {
                return Unauthorized(); // hoặc redirect tới trang báo lỗi
            }

            var allNV = await _context.nhanVien
                .Include(i => i.NhanVienPermission)
                .ThenInclude(p => p.Permission)
                .Include(i => i.Account)
                .ToListAsync();
            ViewBag.CurrentUserRole = HttpContext.Session.GetString("Role");
            ViewBag.AllPermissions = await _context.Permission.ToListAsync();

            return View(allNV);
        }


      
        [HttpPost]
        public async Task<IActionResult> UpdateRole(int id, UserRole role)
        {
            var acc = await _context.Accounts.FindAsync(id);
            if (acc == null) return NotFound();

            acc.Role = role;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Cập nhật quyền thành công!" });
        }

       
        [HttpPost]
        public async Task<IActionResult> UpdatePermissions(int nhanVienId, List<int> selectedPermissions)
        {
            var role = HttpContext.Session.GetString("Role");

            if (role != "Admin")
            {
                return Json(new { success = false, message = "Bạn không có quyền thực hiện hành động này!" });
            }

            var nv = await _context.nhanVien
                .Include(i => i.Account)
                .FirstOrDefaultAsync(i => i.IdNhanVien == nhanVienId);

            if (nv == null)
                return Json(new { success = false, message = "Nhân viên không tồn tại!" });

            // Admin không được chỉnh quyền Admin khác
            if (nv.Account != null && nv.Account.Role == UserRole.Admin)
                return Json(new { success = false, message = "Không thể chỉnh sửa quyền của Admin!" });

            // Xóa quyền cũ
            var old = _context.NhanVienPermission.Where(p => p.NhanVienId == nhanVienId);
            _context.NhanVienPermission.RemoveRange(old);

            // Thêm quyền mới
            foreach (var pid in selectedPermissions)
            {
                _context.NhanVienPermission.Add(new NhanVienPermission
                {
                    NhanVienId = nhanVienId,
                    PermissionId = pid
                });
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Cập nhật quyền thành công!" });
        }

       
        private bool HasPermission(Account account, string permission)
        {
            if (account.Role == UserRole.Admin) return true;
            if (account.NhanVien == null) return false;

            return account.NhanVien.NhanVienPermission
                .Any(p => p.Permission.Name == permission);
        }
    }

    
    public class AuthorizeRoleAttribute : TypeFilterAttribute
    {
        public AuthorizeRoleAttribute(params UserRole[] roles)
            : base(typeof(AuthorizeRoleFilter))
        {
            Arguments = new object[] { roles };
        }
    }

    public class AuthorizeRoleFilter : IAuthorizationFilter
    {
        private readonly UserRole[] _roles;

        public AuthorizeRoleFilter(UserRole[] roles)
        {
            _roles = roles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var roleString = context.HttpContext.Session.GetString("Role");
            if (roleString == null)
            {
                context.Result = new RedirectToActionResult("Login", "Login", null);
                return;
            }

            Enum.TryParse<UserRole>(roleString, out var userRole);

            if (!_roles.Contains(userRole))
            {
                context.Result = new ContentResult
                {
                    Content = "Bạn không có quyền truy cập trang này."
                };
            }
        }
    }
}
