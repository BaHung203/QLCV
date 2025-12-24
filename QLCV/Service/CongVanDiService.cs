using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using WebApp.Hubs;
using WebApp.Data;
using WebApp.Models;
using WebApp.ModelUI;
using WebApp.Libs;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using Xceed.Words.NET;
using Tesseract;
using System.Text;
namespace WebApp.Services
{
    public class CongVanDiService : ICongVanDiService
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext; 
        private readonly IWebHostEnvironment _env;

        public CongVanDiService(AppDbContext context, IHubContext<NotificationHub> hubContext, IWebHostEnvironment env)
        {
            _context = context;
            _hubContext = hubContext;
            _env = env;
        }

        public async Task<PagedResult<CongVanDetailModel>> GetAllAsync(string keyword, int page, int pageSize)
        {
            keyword = keyword?.Trim().ToLower() ?? string.Empty;

            var query = _context.CongVan
                .Include(i => i.NoiNhan)
                .Include(x => x.XuLyCongVan)
                .Where(w => w.LoaiCongVan == LoaiCongVan.CongVanDi)
                .AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(c =>
                    (c.SoHieu ?? "").ToLower().Contains(keyword) ||
                    (c.NoiNhan.TenNoiNhan ?? "").ToLower().Contains(keyword) ||
                    (c.ViTri ?? "").ToLower().Contains(keyword) ||
                    (c.NoiDung ?? "").ToLower().Contains(keyword) ||
                    (c.NoiDungTep ?? "").ToLower().Contains(keyword)
                );
            }

            var totalItems = await query.CountAsync();

            var items = await query
                .OrderByDescending(c => c.Ngay)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CongVanDetailModel
                {
                    ID = c.ID,
                    SoHieu = c.SoHieu,
                    Ngay = c.Ngay,
                    NoiNhan = c.NoiNhan.TenNoiNhan,
                    ViTri = c.ViTri,
                    NoiDung = c.NoiDung,
                    TepDinhKem = c.TepDinhKem,
                    NoiDungTep = c.NoiDungTep,
                    IdNoiNhan = c.IdNoiNhan,
                    File = File.Exists(c.TepDinhKem) ? File.ReadAllBytes(c.TepDinhKem) : Array.Empty<byte>(),
                    TrangThai = c.XuLyCongVan
                    .OrderByDescending(x => x.NgayXuLy)
                    .Select(x => x.TrangThai.ToString())
                    .FirstOrDefault() 
                    ?? "Chưa xử lý",
                })
                .ToListAsync();

            return new PagedResult<CongVanDetailModel>
            {
                Items = items,
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<congVan?> GetByIdAsync(int id)
        {
            return await _context.CongVan
            .Include(x => x.NoiNhan)
            .Include(x => x.XuLyCongVan)
            .FirstOrDefaultAsync(x => x.ID == id);
        }

        public async Task CreateAsync(CongVanCreateModel cv)
        {
            var cvd = new congVan
            {
                LoaiCongVan = LoaiCongVan.CongVanDi,
                IdNoiNhan = cv.IdNoiNhan,
                Ngay = cv.Ngay,
                SoHieu = cv.SoHieu,
                ViTri = cv.ViTri,
                NoiDung = cv.NoiDung
            };

            if (cv.File != null)
            {
                var (path, extractedText) = await UploadFileAsync(cv.File);
                cvd.TepDinhKem = path;
                cvd.NoiDungTep = extractedText;
            }

            _context.CongVan.Add(cvd);
            await _context.SaveChangesAsync();
            await SendDocumentCountUpdate();
        }


        public async Task UpdateAsync(int id, CongVanCreateModel cv)
        {
            var cvd = await _context.CongVan.FindAsync(id);
            if (cvd == null)
                throw new Exception("Không tìm thấy công văn đi.");

            cvd.LoaiCongVan = LoaiCongVan.CongVanDi;
            cvd.IdNoiNhan = cv.IdNoiNhan;
            cvd.Ngay = cv.Ngay;
            cvd.SoHieu = cv.SoHieu;
            cvd.ViTri = cv.ViTri;
            cvd.NoiDung = cv.NoiDung;
            cvd.NoiDungTep = cv.NoiDungTep;

            if (cv.File != null)
            {
                var (path, extractedText) = await UploadFileAsync(cv.File);
                cvd.TepDinhKem = path;
                cvd.NoiDungTep = extractedText;
            }
            _context.Update(cvd);
            await _context.SaveChangesAsync();
            await SendDocumentCountUpdate();
        }

        public async Task DeleteAsync(int id)
        {
            var cv = await _context.CongVan.FindAsync(id);
            if (cv == null)
                throw new Exception("Không tìm thấy công văn đi.");
            var xuly = _context.XuLyCongVan.Where(x => x.IdCongVan == id);
            _context.XuLyCongVan.RemoveRange(xuly);
            _context.CongVan.Remove(cv);
            await _context.SaveChangesAsync();
            await SendDocumentCountUpdate();
        }
        public async Task AddXuLyAsync(int idCongVan, int idNhanVien, TrangThaiXuLy trangThai, string? ghiChu)
        {
            var x = new XuLyCongVan
            {
                IdCongVan = idCongVan,
                IdNhanVien = idNhanVien,
                TrangThai = trangThai,
                GhiChu = ghiChu,
                NgayXuLy = DateTime.Now
            };

            _context.XuLyCongVan.Add(x);
            await _context.SaveChangesAsync();
        }
        public async Task<List<XuLyCongVan>> GetXuLyByCongVanIdAsync(int id)
        {
            return await _context.XuLyCongVan
                .Include(x => x.NhanVien)
                .Where(x => x.IdCongVan == id)
                .OrderByDescending(x => x.NgayXuLy)
                .ToListAsync();
        }
        public async Task<(string filePath, string extractedText)> UploadFileAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return (string.Empty, string.Empty);

            var uploadPath = Path.Combine(_env.WebRootPath, "File");
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            var fileName = Path.GetFileName(file.FileName);
            var filePath = Path.Combine(uploadPath, fileName);
            var relativePath = $"/File/{fileName}";

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Trích xuất nội dung
            var text = await ExtractTextFromFileAsync(filePath);

            return (relativePath, text);
        }


        public async Task<byte[]> DownloadAsync(int id)
        {
            var cv = await _context.CongVan.FindAsync(id);
            if (cv == null || string.IsNullOrEmpty(cv.TepDinhKem))
                return Array.Empty<byte>();

            var filePath = Path.Combine(_env.WebRootPath, cv.TepDinhKem.TrimStart('/', '\\'));
            if (!File.Exists(filePath))
                throw new FileNotFoundException("File không tồn tại", cv.TepDinhKem);

            return await File.ReadAllBytesAsync(filePath);
        }

        public async Task SendDocumentCountUpdate()
        {
            int incomingCount = await _context.CongVan.CountAsync(c => c.LoaiCongVan == LoaiCongVan.CongVanDen);
            int outgoingCount = await _context.CongVan.CountAsync(c => c.LoaiCongVan == LoaiCongVan.CongVanDi);
            await _hubContext.Clients.All.SendAsync("UpdateDocumentCount", incomingCount, outgoingCount);
        }
        public async Task<List<NoiNhan>> GetNoiNhanAsync()
        {
             return await _context.NoiNhan
                .Select(p => new NoiNhan {
                    ID = p.ID,
                    TenNoiNhan = p.TenNoiNhan
                })
                .ToListAsync();
        }
        public async Task<string> ExtractTextFromFileAsync(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLower();
            string extractedText = string.Empty;

            try
            {
                if (extension == ".pdf")
                {
                    // 🟢 Trích xuất nội dung PDF
                    using var reader = new iText.Kernel.Pdf.PdfReader(filePath);
                    using var doc = new iText.Kernel.Pdf.PdfDocument(reader);
                    var sb = new System.Text.StringBuilder();

                    for (int i = 1; i <= doc.GetNumberOfPages(); i++)
                    {
                        var pageText = iText.Kernel.Pdf.Canvas.Parser.PdfTextExtractor.GetTextFromPage(doc.GetPage(i));
                        sb.AppendLine(pageText);
                    }
                    extractedText = sb.ToString();
                }
                else if (extension == ".docx" || extension == ".doc")
                {
                    // 🟢 Trích xuất nội dung Word bằng Tika
                    using var document = DocX.Load(filePath);
                    extractedText = document.Text;
                }
                else if (extension == ".jpg" || extension == ".jpeg" || extension == ".png")
                {
                    // 🟢 Trích xuất text từ ảnh bằng OCR (Tesseract)
                    using var engine = new Tesseract.TesseractEngine(@"./tessdata", "eng", Tesseract.EngineMode.Default);
                    using var img = Tesseract.Pix.LoadFromFile(filePath);
                    using var page = engine.Process(img);
                    extractedText = page.GetText();
                }
            }
            catch (Exception ex)
            {
                extractedText = $"[Lỗi trích xuất nội dung: {ex.Message}]";
            }

            return extractedText;
        }
    }
}
