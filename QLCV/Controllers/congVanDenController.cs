using Microsoft.AspNetCore.Mvc;
using WebApp.ModelUI;
using WebApp.Services;
using Microsoft.EntityFrameworkCore;
using WebApp.Models;
using WebApp.Libs;

namespace WebApp.Controllers
{
    public class CongVanDenController : Controller
    {
        private readonly ICongVanDenService _service;

        public CongVanDenController(ICongVanDenService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index(string keyword, int page = 1, int pageSize = 10)
        {
            var result = await _service.GetAllAsync(keyword, page, pageSize);
            return View(result);
        }


        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(CongVanCreateModel model)
        {
            if (ModelState.IsValid)
            {
                await _service.CreateAsync(model);
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var cv = await _service.GetByIdAsync(id);
            if (cv == null) return NotFound();
            return View(cv);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var cv = await _service.GetByIdAsync(id);
            if (cv == null) return NotFound();

            var model = new congVan
            {
                ID = cv.ID,
                LoaiCongVan = cv.LoaiCongVan,
                IdNoiPhatHanh = cv.IdNoiPhatHanh,
                Ngay = cv.Ngay,
                SoHieu = cv.SoHieu,
                ViTri = cv.ViTri,
                NoiDung = cv.NoiDung,
                NoiDungTep = cv.NoiDungTep,
                TepDinhKem = cv.TepDinhKem
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CongVanUpdateModel model)
        {
            if (ModelState.IsValid)
            {
                await _service.UpdateAsync(id, model);
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Download(int id)
        {
            var bytes = await _service.DownloadAsync(id);
            var cv = await _service.GetByIdAsync(id);
            var fileName = cv?.TepDinhKem?.Substring(cv.TepDinhKem.LastIndexOf('\\') + 1) ?? "file.bin";
            return File(bytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }

        [HttpGet]
        public async Task<IActionResult> GetNoiPhatHanh()
        {
           var noiPhatHanhs = await _service.GetNoiPhatHanhAsync();
            return Json(noiPhatHanhs);

        }
    
    }
}
