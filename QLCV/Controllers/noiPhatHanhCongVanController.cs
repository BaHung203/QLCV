using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Controllers
{
    public class NoiPhatHanhController : Controller
    {
        private readonly AppDbContext _context;

        public NoiPhatHanhController(AppDbContext context)
        {
            _context = context;
        }

        // GET: NoiPhatHanh
        public async Task<IActionResult> Index(string searchString, int? pageNumber)
        {
            var noiPhatHanhs = _context.NoiPhatHanh.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                noiPhatHanhs = noiPhatHanhs.Where(n => 
                    n.TenNoiPhatHanh.Contains(searchString));
                ViewData["CurrentFilter"] = searchString;
            }

            noiPhatHanhs = noiPhatHanhs.OrderBy(n => n.TenNoiPhatHanh);

            int pageSize = 10;
            return View(await PaginatedList<NoiPhatHanh>.CreateAsync(
                noiPhatHanhs.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: NoiPhatHanh/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var noiPhatHanh = await _context.NoiPhatHanh
                .FirstOrDefaultAsync(m => m.ID == id);
                
            if (noiPhatHanh == null)
            {
                return NotFound();
            }

            return View(noiPhatHanh);
        }

        // GET: NoiPhatHanh/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: NoiPhatHanh/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,TenNoiPhatHanh")] NoiPhatHanh noiPhatHanh)
        {
            if (await _context.NoiPhatHanh
                .AnyAsync(n => n.TenNoiPhatHanh == noiPhatHanh.TenNoiPhatHanh))
            {
                ModelState.AddModelError("TenNoiPhatHanh", "Tên nơi phát hành đã tồn tại");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(noiPhatHanh);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Thêm mới nơi phát hành thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Lỗi khi lưu dữ liệu: {ex.Message}");
                }
            }
            return View(noiPhatHanh);
        }

        // GET: NoiPhatHanh/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var noiPhatHanh = await _context.NoiPhatHanh.FindAsync(id);
            if (noiPhatHanh == null)
            {
                return NotFound();
            }
            return View(noiPhatHanh);
        }

        // POST: NoiPhatHanh/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,TenNoiPhatHanh")] NoiPhatHanh noiPhatHanh)
        {
            if (id != noiPhatHanh.ID)
            {
                return NotFound();
            }

            if (await _context.NoiPhatHanh
                .AnyAsync(n => n.TenNoiPhatHanh == noiPhatHanh.TenNoiPhatHanh && n.ID != id))
            {
                ModelState.AddModelError("TenNoiPhatHanh", "Tên nơi phát hành đã tồn tại");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(noiPhatHanh);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật nơi phát hành thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await NoiPhatHanhExists(noiPhatHanh.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Lỗi khi cập nhật: {ex.Message}");
                }
            }
            return View(noiPhatHanh);
        }

        // GET: NoiPhatHanh/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var noiPhatHanh = await _context.NoiPhatHanh
                .FirstOrDefaultAsync(m => m.ID == id);
                
            if (noiPhatHanh == null)
            {
                return NotFound();
            }

            return View(noiPhatHanh);
        }

        // POST: NoiPhatHanh/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var noiPhatHanh = await _context.NoiPhatHanh.FindAsync(id);
                if (noiPhatHanh == null)
                {
                    return NotFound();
                }

                _context.NoiPhatHanh.Remove(noiPhatHanh);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa nơi phát hành thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                TempData["ErrorMessage"] = "Không thể xóa vì có dữ liệu liên quan!";
                return RedirectToAction(nameof(Delete), new { id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi xóa: {ex.Message}";
                return RedirectToAction(nameof(Delete), new { id });
            }
        }

        // AJAX: Kiểm tra tên trùng
        [AcceptVerbs("GET", "POST")]
        public async Task<IActionResult> VerifyTenNoiPhatHanh(string tenNoiPhatHanh, int id = 0)
        {
            var exists = await _context.NoiPhatHanh
                .AnyAsync(n => n.TenNoiPhatHanh == tenNoiPhatHanh && n.ID != id);
                
            return Json(!exists);
        }

        private async Task<bool> NoiPhatHanhExists(int id)
        {
            return await _context.NoiPhatHanh.AnyAsync(e => e.ID == id);
        }
    }
}