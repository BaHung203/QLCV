using Microsoft.AspNetCore.Mvc;
using WebApp.Models;
using System.Linq;
using WebApp.Data;
using WebApp.ModelUI;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Controllers
{
    public class CongVanDenController : Controller
    {
        private readonly AppDbContext _context;

        public CongVanDenController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
             var list = _context.CongVanDen.ToList();
            return View(list);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CongVanDenCreateModel cv)
        {
             CongVanDen cvden = new CongVanDen{
                ID = cv.ID,
                IdLoaiCongVan =cv.IdLoaiCongVan,
                IdNoiPhatHanh = cv.IdNoiPhatHanh,
                NgayDen = cv.NgayDen,
                SoHieu = cv.SoHieu,
                Vitri = cv.Vitri,
                NoiDung = cv.NoiDung,
                NoiDungTep = cv.NoiDungTep,
            };
            string filePath = await UploadFile(cv.File);
            cvden.TepDinhKem = filePath;
            if (ModelState.IsValid)
            {
                _context.CongVanDen.Add(cvden);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(cv);
        }

        public IActionResult Details(int id)
        {
             var cv = _context.CongVanDen.FirstOrDefault(x => x.ID == id);
            if (cv == null) return NotFound();
            return View(cv);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var cvd = await _context.CongVanDen.FindAsync(id);
            if (cvd == null)
            {
                return NotFound();
            }

            var model = new CongVanDenCreateModel
            {
                ID = cvd.ID,
                IdLoaiCongVan = cvd.IdLoaiCongVan,
                IdNoiPhatHanh = cvd.IdNoiPhatHanh,
                NgayDen = cvd.NgayDen,
                SoHieu = cvd.SoHieu,
                Vitri = cvd.Vitri,
                NoiDung = cvd.NoiDung,
                NoiDungTep = cvd.NoiDungTep,
                TepDinhKem = cvd.TepDinhKem // Nếu bạn cần hiện tên file cũ
            };

            
            return View(model);
        }


        [HttpPost]
        // POST: CongVanDi/Edit/5

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CongVanDenCreateModel cv)
        {
            if (id != cv.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var cvd = await _context.CongVanDen.FindAsync(id);
                    if (cvd == null)
                    {
                        return NotFound();
                    }

                    cvd.IdLoaiCongVan = cv.IdLoaiCongVan;
                    cvd.IdNoiPhatHanh = cv.IdNoiPhatHanh;
                    cvd.NgayDen = cv.NgayDen;
                    cvd.SoHieu = cv.SoHieu;
                    cvd.Vitri = cv.Vitri;
                    cvd.NoiDung = cv.NoiDung;
                    cvd.NoiDungTep = cv.NoiDungTep;

                    // Nếu có file mới thì upload và cập nhật
                    if (cv.File != null)
                    {
                        string filePath = await UploadFile(cv.File);
                        cvd.TepDinhKem = filePath;
                    }

                    _context.Update(cvd);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.CongVanDen.Any(e => e.ID == id))
                        return NotFound();
                    else
                        throw;
                }
            }

            return View(cv);
        }

        public IActionResult Delete(int id)
        {
            var cv = _context.CongVanDen.Find(id);
            _context.CongVanDen.Remove(cv);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public override bool Equals(object? obj)
        {
            return obj is CongVanDenController controller &&
                   EqualityComparer<AppDbContext>.Default.Equals(_context, controller._context);
        }
        public async Task<string> UploadFile(IFormFile? file)
        {
            try
            {
                if (file != null && file.Length > 0)
                {
                    // Tạo thư mục nếu chưa tồn tại
                    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "File");
                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }

                    var fileName = Path.GetFileName(file.FileName);
                    var filePath = Path.Combine(uploadPath, fileName);

                    // Lưu file vào server
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    ViewBag.Message = "File Uploaded Successfully!!";
                    return filePath;
                }
                else
                {
                    ViewBag.Message = "No file selected.";
                }
                return string.Empty;
                
            }
            catch (Exception ex)
            {
                ViewBag.Message = "File upload failed!! " + ex.Message;
                return string.Empty;
            }
        }
        public async Task<IActionResult> Download(int id)
        {
            var cv = await _context.CongVanDen.FindAsync(id);
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(cv?.TepDinhKem ?? "");
            var fileName = cv?.TepDinhKem?.Substring(cv.TepDinhKem.LastIndexOf('\\') + 1);
            return File(bytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }
    }
}
