using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApp.Models;
using WebApp.Data;
using System.Linq;
using WebApp.ModelUI;
using Microsoft.EntityFrameworkCore;


namespace WebApp.Controllers
{
    public class CongVanDiController : Controller
    {
        private readonly AppDbContext _context;

        public CongVanDiController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var danhSach = _context.CongVanDi
            .Select(c => new CongVanDiDetailModel
            {
                ID = c.ID,
                SoHieu = c.SoHieu,
                NgayDi = c.NgayDi,
                IdNoiNhan = c.IdNoiNhan,
                IdLoaiCongVan = c.IdLoaiCongVan,
                Vitri = c.Vitri,
                NoiDung = c.NoiDung,
                TepDinhKem = c.TepDinhKem,
                NoiDungTep = c.NoiDungTep,
                file = GetBytesFromFilePath(c.TepDinhKem),
            }).ToList();
    
            return View(danhSach);
        }
        // Hàm tạo file byte
        public static byte[] GetBytesFromFilePath(string filePath)
        {
            // Kiểm tra file có tồn tại không
            if (!System.IO.File.Exists(filePath))
            {
                throw new FileNotFoundException("File không tồn tại", filePath);
            }

            // Đọc toàn bộ file vào mảng byte
            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
            
            return fileBytes;
        }
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CongVanDiCreateModel cv)
        {
            CongVanDi cvd = new CongVanDi{
                ID = cv.ID,
                IdLoaiCongVan =cv.IdLoaiCongVan,
                IdNoiNhan = cv.IdNoiNhan,
                NgayDi = cv.NgayDi,
                SoHieu = cv.SoHieu,
                Vitri = cv.Vitri,
                NoiDung = cv.NoiDung,
                NoiDungTep = cv.NoiDungTep,
            };
            string filePath = await UploadFile(cv.File);
            cvd.TepDinhKem = filePath;
            if (ModelState.IsValid)
            {
                _context.CongVanDi.Add(cvd);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

        
            return View(cv);
        }

        public IActionResult Details(int id)
        {
            var cv = _context.CongVanDi.FirstOrDefault(x => x.ID == id);
            if (cv == null) return NotFound();
            return View(cv);
        }

        // GET: CongVanDi/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var cvd = await _context.CongVanDi.FindAsync(id);
            if (cvd == null)
            {
                return NotFound();
            }

            var model = new CongVanDiCreateModel
            {
                ID = cvd.ID,
                IdLoaiCongVan = cvd.IdLoaiCongVan,
                IdNoiNhan = cvd.IdNoiNhan,
                NgayDi = cvd.NgayDi,
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
        public async Task<IActionResult> Edit(int id, CongVanDiCreateModel cv)
        {
            if (id != cv.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var cvd = await _context.CongVanDi.FindAsync(id);
                    if (cvd == null)
                    {
                        return NotFound();
                    }

                    cvd.IdLoaiCongVan = cv.IdLoaiCongVan;
                    cvd.IdNoiNhan = cv.IdNoiNhan;
                    cvd.NgayDi = cv.NgayDi;
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
                    if (!_context.CongVanDi.Any(e => e.ID == id))
                        return NotFound();
                    else
                        throw;
                }
            }

           
            return View(cv);
        }


        public IActionResult Delete(int id)
        {
            var cv = _context.CongVanDi.Find(id);
            if (cv == null) return NotFound();

            _context.CongVanDi.Remove(cv);
            _context.SaveChanges();
            return RedirectToAction("Index");
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
            var cv = await _context.CongVanDi.FindAsync(id);
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(cv?.TepDinhKem ?? "");
            var fileName = cv?.TepDinhKem?.Substring(cv.TepDinhKem.LastIndexOf('\\') + 1);
            return File(bytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }
        
    }
    
}
