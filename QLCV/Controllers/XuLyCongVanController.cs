using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Models;
using WebApp.Libs;

namespace WebApp.Controllers
{
    public class XuLyCongVanController : Controller
    {
        private readonly AppDbContext _context;

        public XuLyCongVanController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /XuLyCongVan/Create?congVanId=123
        [HttpGet]
        public async Task<IActionResult> Create(int congVanId)
        {
            var congVan = await _context.CongVan.FindAsync(congVanId);
            if (congVan == null) return NotFound();

            ViewBag.CongVan = congVan;
            ViewBag.NhanVienList = await _context.nhanVien.ToListAsync();
            return View();
        }

        // POST: /XuLyCongVan/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(XuLyCongVan model)
        {
            var role = HttpContext.Session.GetString("Role");
            var currentNhanVienId = HttpContext.Session.GetInt32("IdNhanVien") ?? 0;

            if (role == null) 
                return RedirectToAction("Login", "Login");

            if (role != "Admin" && currentNhanVienId != model.IdNhanVien)
            {
                TempData["Error"] = "Bạn không có quyền giao/đăng ký người xử lý này.";
                return await RedirectToCongVan(model.IdCongVan);
            }

            model.NgayXuLy = DateTime.Now;

            _context.XuLyCongVan.Add(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Ghi nhận xử lý thành công.";
            return await RedirectToCongVan(model.IdCongVan);
        }

        // POST: /XuLyCongVan/MarkComplete
        [HttpPost]
        public async Task<IActionResult> MarkComplete(int idXuLy, string? ghiChu)
        {
            var xl = await _context.XuLyCongVan
                .Include(x => x.NhanVien)
                .FirstOrDefaultAsync(x => x.IdXuLy == idXuLy);

            if (xl == null) return NotFound();

            var role = HttpContext.Session.GetString("Role");
            var currentNhanVienId = HttpContext.Session.GetInt32("IdNhanVien");

            if (role == null)
                return RedirectToAction("Login", "Login");

            if (role != "Admin" && currentNhanVienId != xl.IdNhanVien)
                return Forbid();

            xl.TrangThai = TrangThaiXuLy.DaHoanThanh;
            if (!string.IsNullOrEmpty(ghiChu)) xl.GhiChu = ghiChu;

            await _context.SaveChangesAsync();

            return await RedirectToCongVan(xl.IdCongVan);
        }

        // GET: /XuLyCongVan/History?congVanId=123
        public async Task<IActionResult> History(int congVanId)
        {
            var list = await _context.XuLyCongVan
                .Include(x => x.NhanVien)
                .Where(x => x.IdCongVan == congVanId)
                .OrderByDescending(x => x.NgayXuLy)
                .ToListAsync();

            ViewBag.CongVan = await _context.CongVan.FindAsync(congVanId);
            return View(list);
        }

        // Optional: index of all processing records (admin)
        public async Task<IActionResult> Index()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin") return Forbid();

            var list = await _context.XuLyCongVan
                .Include(x => x.CongVan)
                .Include(x => x.NhanVien)
                .OrderByDescending(x => x.NgayXuLy)
                .ToListAsync();

            return View(list);
        }

        // Helper: redirect to correct Details based on enum LoaiCongVan
        private async Task<IActionResult> RedirectToCongVan(int idCongVan)
        {
            var cv = await _context.CongVan.FindAsync(idCongVan);
            if (cv == null) return RedirectToAction("Index", "Home");

            // ---- HERE: compare enum to enum (not to string) ----
            // Adjust the enum member names to match your LoaiCongVan definition
            if (cv.LoaiCongVan == LoaiCongVan.CongVanDen)
                return RedirectToAction("Details", "CongVanDen", new { id = idCongVan });

            if (cv.LoaiCongVan == LoaiCongVan.CongVanDi)
                return RedirectToAction("Details", "CongVanDi", new { id = idCongVan });

            // fallback
            return RedirectToAction("Index", "Home");
        }
    }
}
