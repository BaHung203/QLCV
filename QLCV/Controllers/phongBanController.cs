using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Models;

namespace WebApp.Controllers
{
    public class PhongBanController : Controller
    {
        private readonly AppDbContext _context;

        public PhongBanController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /PhongBan
        public async Task<IActionResult> Index()
        {
            var dsPhongBan = await _context.phongBan.ToListAsync();

            var dsNhanVien = await _context.nhanVien.ToListAsync();

            foreach (var pb in dsPhongBan)
            {
                pb.nhanVien = dsNhanVien.Where(nv => nv.IdNhanVien == pb.IdTruongPhong).ToList();
            }

            return View(dsPhongBan);
        }


        // GET: /PhongBan/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var pb = await _context.phongBan
                .FirstOrDefaultAsync(m => m.IdPhongBan == id);

            if (pb == null) return NotFound();

            return View(pb);
        }

        // GET: /PhongBan/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /PhongBan/Create
        [HttpPost]
        public async Task<IActionResult> Create(phongBan phongBan)
        {
            if (ModelState.IsValid)
            {
                _context.phongBan.Add(phongBan);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(phongBan);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var pb = await _context.phongBan.FindAsync(id);
            if (pb == null) return NotFound();

            return View(pb);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit( phongBan phongBan)
        {
            var id = phongBan.IdPhongBan;
            if (id != phongBan.IdPhongBan) 
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var pb = await _context.phongBan.FindAsync(id);
                    if (pb == null)
                    {
                        return NotFound();
                    }
                    pb.TenPhongBan = phongBan.TenPhongBan;
                    pb.IdTruongPhong = phongBan.IdTruongPhong;
                    pb.SoDienThoai = phongBan.SoDienThoai;
                    
                    _context.Update(pb);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.phongBan.Any(e => e.IdPhongBan == id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction("Index");
            }
            return View(phongBan);
        }

        
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var pb = _context.phongBan.Find(id);
            if (pb == null) return NotFound();
            _context.phongBan.Remove(pb);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> CheckTen(string TenPhongBan, int? IdPhongBan)
        {
            if(string.IsNullOrWhiteSpace(TenPhongBan))
            return Json(false);
             bool exists = await _context.phongBan
                .AnyAsync(x => x.TenPhongBan.ToLower() == TenPhongBan.ToLower() 
                            && x.IdPhongBan != IdPhongBan);

            return Json(exists);
        }
        [HttpGet]
        public async Task<IActionResult> GetNhanVien()
        {
            var nhanViens = await _context.nhanVien
                .Select(p => new
                {
                    p.IdNhanVien,
                    p.HoTen
                })
                .ToListAsync();

            return Json(nhanViens);
        }



    }
}
