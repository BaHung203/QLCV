using Microsoft.AspNetCore.Mvc;
using WebApp.Models;
using WebApp.Services;
using Microsoft.AspNetCore.SignalR;
using WebApp.Hubs;

namespace WebApp.Controllers
{
    public class NhanVienController : Controller
    {
        private readonly INhanVienService _service;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NhanVienController(INhanVienService service, IHubContext<NotificationHub> hubContext)
        {
            _service = service;
            _hubContext = hubContext;
        }

        public async Task<IActionResult> Index(string keyword, int page = 1, int pageSize = 10)
        {
            var dsNhanVien = await _service.GetAllAsync(keyword ,page, pageSize);
            return View(dsNhanVien);
        }

        public async Task<IActionResult> Details(int id)
        {
            var nv = await _service.GetByIdAsync(id);
            if (nv == null) return NotFound();
            return View(nv);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.PhongBanList = await _service.GetPhongBanListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(nhanVien nhanVien)
        {
            if (ModelState.IsValid)
            {
                await _service.CreateAsync(nhanVien);
                await SendEmployeeCountUpdate();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.PhongBanList = await _service.GetPhongBanListAsync();
            return View(nhanVien);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var nv = await _service.GetByIdAsync(id);
            if (nv == null) return NotFound();

            ViewBag.PhongBanList = await _service.GetPhongBanListAsync();
            return View(nv);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(nhanVien nhanVien)
        {
            if (ModelState.IsValid)
            {
                await _service.UpdateAsync(nhanVien);
                await SendEmployeeCountUpdate();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.PhongBanList = await _service.GetPhongBanListAsync();
            return View(nhanVien);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            await SendEmployeeCountUpdate();
            return RedirectToAction(nameof(Index));
        }

        private async Task SendEmployeeCountUpdate()
        {
            int employeeCount = await _service.GetEmployeeCountAsync();
            await _hubContext.Clients.All.SendAsync("UpdateEmployeeCount", employeeCount);
        }

        [HttpGet]
        public async Task<IActionResult> GetPhongBan()
        {
            var phongBans = await _service.GetPhongBanListAsync();
            return Json(phongBans.Select(p => new
            {
                p.IdPhongBan,
                p.TenPhongBan
            }));
        }
    }
}
