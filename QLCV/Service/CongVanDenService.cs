using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using WebApp.Hubs;
using WebApp.Data;
using WebApp.ModelUI;
using WebApp.Models;
using QLCV.Models;
using WebApp.Libs;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using Xceed.Words.NET;
using Tesseract;
using System.Text;


namespace WebApp.Services
{
    public class CongVanDenService : ICongVanDenService
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IWebHostEnvironment _env;

        public CongVanDenService(AppDbContext context, IHubContext<NotificationHub> hubContext, IWebHostEnvironment env)
        {
            _context = context;
            _hubContext = hubContext;
            _env = env;
        }
        public async Task<PagedResult<CongVanDetailModel>> GetAllAsync(string keyword,int page, int pageSize)
        {
            keyword = keyword?.Trim().ToLower() ?? string.Empty;
            var query = _context.CongVan
                .Include(i => i.NoiPhatHanh)
                .Where(w => w.LoaiCongVan == LoaiCongVan.CongVanDen);

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(c =>
                c.SoHieu.ToLower().Contains(keyword) ||
                c.NoiPhatHanh.TenNoiPhatHanh.ToLower().Contains(keyword) ||
                c.ViTri.ToLower().Contains(keyword) ||
                c.NoiDung.ToLower().Contains(keyword) ||
                c.NoiDungTep.ToLower().Contains(keyword) 
                );
            }
            // Tổng số bản ghi
            var totalItems = await query.CountAsync();

            // Lấy dữ liệu cho trang hiện tại
            var items = await query
                .OrderByDescending(c => c.Ngay)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CongVanDetailModel
                {
                    ID = c.ID,
                    SoHieu = c.SoHieu,
                    Ngay = c.Ngay,
                    NoiPhatHanh = c.NoiPhatHanh.TenNoiPhatHanh,
                    ViTri = c.ViTri,
                    NoiDung = c.NoiDung,
                    TepDinhKem = c.TepDinhKem,
                    NoiDungTep = c.NoiDungTep,
                    IdNoiPhatHanh = c.IdNoiPhatHanh,
                    File = File.Exists(c.TepDinhKem) ? File.ReadAllBytes(c.TepDinhKem) : Array.Empty<byte>()
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
            return await _context.CongVan.FirstOrDefaultAsync(x => x.ID == id);
        }

        public async Task CreateAsync(CongVanCreateModel cv)
        {
            var cvd = new congVan
            {
                ID = cv.ID,
                LoaiCongVan = LoaiCongVan.CongVanDen,
                IdNoiPhatHanh = cv.IdNoiPhatHanh,
                Ngay = cv.Ngay,
                SoHieu = cv.SoHieu,
                ViTri = cv.ViTri,
                NoiDung = cv.NoiDung,
                NoiDungTep = cv.NoiDungTep,
            };

            var (path, extractedText) = await UploadFileAsync(cv.File);
                cvd.TepDinhKem = path;
                cvd.NoiDungTep = extractedText;
            _context.CongVan.Add(cvd);
            await _context.SaveChangesAsync();

            var tb = new ThongBao
            {
                TieuDe = "Công văn đến mới",
                NoiDung = $"Công văn số hiệu {cvd.SoHieu} vừa được thêm vào hệ thống.",
                NgayTao = DateTime.Now,
                DaXem = false,
                IdCongVan = cvd.ID
            };

            _context.ThongBao.Add(tb);
            await _context.SaveChangesAsync();

            // 🟢 Gửi thông báo real-time (nếu bạn muốn hiển thị ngay trên client)
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", tb.TieuDe, tb.NoiDung);
            await SendDocumentCountUpdate();
        }

        public async Task UpdateAsync(int id, CongVanUpdateModel cv)
        {
            var cvd = await _context.CongVan.FindAsync(id);
            if (cvd == null) 
                throw new KeyNotFoundException("Không tìm thấy công văn");

            cvd.LoaiCongVan = LoaiCongVan.CongVanDen;
            cvd.IdNoiPhatHanh = cv.IdNoiPhatHanh;
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
            if (cv == null) throw new KeyNotFoundException("Không tìm thấy công văn");

            var thongBaos = _context.ThongBao.Where(t => t.IdCongVan == id);
            _context.ThongBao.RemoveRange(thongBaos);
            _context.CongVan.Remove(cv);
            await _context.SaveChangesAsync();
            await SendDocumentCountUpdate();
        }

        public async Task<(string FilePath, string ExtractedText)> UploadFileAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0) return (string.Empty, string.Empty);

            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "File");
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            var fileName = Path.GetFileName(file.FileName);
            var filePath = Path.Combine(uploadPath, fileName);
            var relativePath = $"/File/{fileName}";

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            string extractedText = await ExtractTextFromFileAsync(filePath);
            return (relativePath, extractedText);
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
        public async Task<List<NoiPhatHanh>> GetNoiPhatHanhAsync()
        {
            return await _context.NoiPhatHanh
               .Select(p => new NoiPhatHanh
               {
                   ID = p.ID,
                   TenNoiPhatHanh = p.TenNoiPhatHanh
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
