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
            public async Task<PagedResult<CongVanDetailModel>> GetAllAsync(string keyword, int page, int pageSize)
            {
                keyword = keyword?.Trim().ToLower() ?? string.Empty;

                var query = _context.CongVan
                    .Include(i => i.NoiPhatHanh)
                    .Include(x => x.XuLyCongVan)
                    .Where(w => w.LoaiCongVan == LoaiCongVan.CongVanDen)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(keyword))
                {
                    query = query.Where(c =>
                        (c.SoHieu ?? "").ToLower().Contains(keyword) ||
                        (c.NoiPhatHanh.TenNoiPhatHanh ?? "").ToLower().Contains(keyword) ||
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
                        NoiPhatHanh = c.NoiPhatHanh.TenNoiPhatHanh,
                        ViTri = c.ViTri,
                        NoiDung = c.NoiDung,
                        TepDinhKem = c.TepDinhKem,
                        NoiDungTep = c.NoiDungTep,
                        IdNoiPhatHanh = c.IdNoiPhatHanh,
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
                    .Include(x => x.NoiPhatHanh)     
                    .Include(x => x.XuLyCongVan)
                    .FirstOrDefaultAsync(x => x.ID == id);
            }


            public async Task CreateAsync(CongVanCreateModel cv)
            {
                try
                {
                    // bool isExist = await _context.CongVan.AnyAsync(x =>
                    //     x.SoHieu == cv.SoHieu &&
                    //     x.LoaiCongVan == LoaiCongVan.CongVanDen
                    // );

                    // if (isExist)
                    //     throw new InvalidOperationException("Số hiệu công văn đến đã tồn tại");

                    var cvd = new congVan
                    {   
                        LoaiCongVan = LoaiCongVan.CongVanDen,
                        IdNoiPhatHanh = cv.IdNoiPhatHanh,
                        Ngay = cv.Ngay,
                        SoHieu = cv.SoHieu,
                        ViTri = cv.ViTri,
                        NoiDung = cv.NoiDung,
                    };

                    if (cv.File != null && cv.File.Length > 0)
                    {
                        var (path, extractedText) = await UploadFileAsync(cv.File);
                        cvd.TepDinhKem = path;
                        cvd.NoiDungTep = extractedText;
                    }

                    _context.CongVan.Add(cvd);
                    await _context.SaveChangesAsync();

                    // LOG ĐÚNG CÁCH ĐỂ BẠN THẤY TRONG VISUAL STUDIO
                    System.Diagnostics.Debug.WriteLine("=== ĐANG CHUẨN BỊ GỬI SIGNALR ===");
                    System.Diagnostics.Debug.WriteLine($"Công văn: {cvd.SoHieu}, ID = {cvd.ID}");

                    var payload = new
                    {
                        tieuDe = "Công văn mới đến",
                        noiDung = $"Công văn số {cvd.SoHieu} vừa được thêm",
                        ngayTao = DateTime.Now.ToString("HH:mm dd/MM/yyyy"),
                        idCongVan = cvd.ID
                    };

                    await _hubContext.Clients.All.SendAsync("ReceiveNotification", payload);

                    System.Diagnostics.Debug.WriteLine("=== ĐÃ GỬI THÀNH CÔNG SIGNALR TỪ SERVICE! ===");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("LỖI TẠO CÔNG VĂN + SIGNALR: " + ex.ToString());
                    throw;
                }
            }


            public async Task UpdateAsync(int id, CongVanUpdateModel cv)
            {
                var cvd = await _context.CongVan
                .Include(x => x.XuLyCongVan)
                .FirstOrDefaultAsync(x => x.ID == id);
                if (cvd == null) 
                    throw new KeyNotFoundException("Không tìm thấy công văn");
                if (cvd.XuLyCongVan.Any())  
                throw new InvalidOperationException("Công văn đã được xử lý, không được chỉnh sửa");
                
                // bool isExist = await _context.CongVan.AnyAsync(x =>
                //     x.SoHieu == cv.SoHieu &&
                //     x.LoaiCongVan == LoaiCongVan.CongVanDen &&
                //     x.ID != id
                // );

                // if (isExist)
                //     throw new InvalidOperationException("Số hiệu công văn đến đã tồn tại");

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
                var cv = await _context.CongVan
                .Include(x => x.XuLyCongVan)
                .FirstOrDefaultAsync(x => x.ID == id);

                if (cv == null) throw new KeyNotFoundException("Không tìm thấy công văn");
                if (cv.XuLyCongVan.Any())
                    throw new InvalidOperationException("Công văn đã được xử lý, không được xóa");
                
                var xuly = _context.XuLyCongVan.Where(x => x.IdCongVan == id);
                _context.XuLyCongVan.RemoveRange(xuly);

                var thongBaos = _context.ThongBao.Where(t => t.IdCongVan == id);
                _context.ThongBao.RemoveRange(thongBaos);
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
            // public async Task<bool> IsSoHieuExistsAsync(string soHieu)
            // {
            //     return await _context.CongVan.AnyAsync(x =>
            //         x.SoHieu == soHieu &&
            //         x.LoaiCongVan == LoaiCongVan.CongVanDen
            //     );
            // }


        }
    }
