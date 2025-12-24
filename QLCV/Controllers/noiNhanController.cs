using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Controllers
{
    public class NoiNhanController : Controller
    {
        private readonly AppDbContext _context;

        public NoiNhanController(AppDbContext context)
        {
            _context = context;
        }

        // GET: NoiNhan
        public async Task<IActionResult> Index(string keyword, int page = 1, int pageSize = 10)
        {
            var noiNhans = _context.NoiNhan.AsQueryable();

            // Tìm kiếm
            if (!string.IsNullOrEmpty(searchString))
            {
                noiNhans = noiNhans.Where(n => 
                    n.TenNoiNhan.Contains(searchString));
                ViewData["CurrentFilter"] = searchString;
            }

            // Sắp xếp
            noiNhans = noiNhans.OrderBy(n => n.TenNoiNhan);

            // Phân trang (cần cài đặt X.PagedList)
            int pageSize = 10;
            return View(await PaginatedList<NoiNhan>.CreateAsync(
                noiNhans.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: NoiNhan/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var noiNhan = await _context.NoiNhan
                .FirstOrDefaultAsync(m => m.ID == id);
                
            if (noiNhan == null)
            {
                return NotFound();
            }

            return View(noiNhan);
        }

        // GET: NoiNhan/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: NoiNhan/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,TenNoiNhan")] NoiNhan noiNhan)
        {
            // Kiểm tra trùng tên
            if (await _context.NoiNhan.AnyAsync(n => n.TenNoiNhan == noiNhan.TenNoiNhan))
            {
                ModelState.AddModelError("TenNoiNhan", "Tên nơi nhận đã tồn tại");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(noiNhan);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Thêm mới nơi nhận thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Lỗi khi lưu dữ liệu: {ex.Message}");
                }
            }
            return View(noiNhan);
        }

        // GET: NoiNhan/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var noiNhan = await _context.NoiNhan.FindAsync(id);
            if (noiNhan == null)
            {
                return NotFound();
            }
            return View(noiNhan);
        }

        // POST: NoiNhan/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,TenNoiNhan")] NoiNhan noiNhan)
        {
            if (id != noiNhan.ID)
            {
                return NotFound();
            }

            // Kiểm tra trùng tên (loại trừ bản ghi hiện tại)
            if (await _context.NoiNhan
                .AnyAsync(n => n.TenNoiNhan == noiNhan.TenNoiNhan && n.ID != id))
            {
                ModelState.AddModelError("TenNoiNhan", "Tên nơi nhận đã tồn tại");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(noiNhan);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật nơi nhận thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await NoiNhanExists(noiNhan.ID))
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
            return View(noiNhan);
        }

        // GET: NoiNhan/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var noiNhan = await _context.NoiNhan
                .FirstOrDefaultAsync(m => m.ID == id);
                
            if (noiNhan == null)
            {
                return NotFound();
            }

            return View(noiNhan);
        }

        // POST: NoiNhan/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var noiNhan = await _context.NoiNhan.FindAsync(id);
                if (noiNhan == null)
                {
                    return NotFound();
                }

                // Kiểm tra ràng buộc khóa ngoại trước khi xóa
                // var hasRelatedRecords = await _context.CongVanDen.AnyAsync(c => c.NoiNhanID == id);
                // if (hasRelatedRecords)
                // {
                //     TempData["ErrorMessage"] = "Không thể xóa vì có công văn liên quan!";
                //     return RedirectToAction(nameof(Delete), new { id });
                // }

                _context.NoiNhan.Remove(noiNhan);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa nơi nhận thành công!";
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
        public async Task<IActionResult> VerifyTenNoiNhan(string tenNoiNhan, int id = 0)
        {
            var exists = await _context.NoiNhan
                .AnyAsync(n => n.TenNoiNhan == tenNoiNhan && n.ID != id);
                
            return Json(!exists);
        }

        private async Task<bool> NoiNhanExists(int id)
        {
            return await _context.NoiNhan.AnyAsync(e => e.ID == id);
        }
    }
}