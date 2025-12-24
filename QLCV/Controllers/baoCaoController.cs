using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Models;
using WebApp.Libs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.IO;
using Microsoft.Extensions.Logging;

namespace WebApp.Controllers
{
    public class BaoCaoController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<BaoCaoController> _logger;

        public BaoCaoController(AppDbContext context, ILogger<BaoCaoController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ------------------ MODEL CHO BỘ LỌC ---------------------
        public class BaoCaoFilterModel
        {
            public int? Nam { get; set; } = DateTime.Now.Year;
            public int? Thang { get; set; }
            public LoaiCongVan? LoaiCongVan { get; set; }
            public DateTime? TuNgay { get; set; }
            public DateTime? DenNgay { get; set; }
            public int Page { get; set; } = 1;
            public int PageSize { get; set; } = 20;
        }

        // ------------------ TRANG INDEX VỚI BỘ LỌC & PHÂN TRANG ---------------------
        [HttpGet]
        public async Task<IActionResult> Index(BaoCaoFilterModel filter)
        {
            try
            {
                // Xây dựng query với bộ lọc
                var query = BuildFilterQuery(filter);

                // Lấy tổng số bản ghi cho phân trang
                var totalItems = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalItems / (double)filter.PageSize);

                // Lấy dữ liệu với phân trang
                var congVan = await query
                    .Include(x => x.NoiPhatHanh)
                    .Include(x => x.NoiNhan)
                    .OrderByDescending(x => x.Ngay)
                    .ThenByDescending(x => x.ID)
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync();

                // Thống kê tổng quan
                await LoadThongKeTongQuan(filter);
                
                // Thống kê theo tháng cho biểu đồ
                await LoadThongKeTheoThang(filter.Nam ?? DateTime.Now.Year);

                // Truyền dữ liệu ra View
                ViewBag.Filter = filter;
                ViewBag.CurrentPage = filter.Page;
                ViewBag.TotalPages = totalPages;
                ViewBag.PageSize = filter.PageSize;
                ViewBag.TotalItems = totalItems;

                // Danh sách năm có dữ liệu
                var availableYears = await _context.CongVan
                    .Select(x => x.Ngay.Year)
                    .Distinct()
                    .OrderByDescending(x => x)
                    .ToListAsync();
                ViewBag.AvailableYears = availableYears;

                return View(congVan);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải trang báo cáo");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải dữ liệu báo cáo";
                return View(new List<congVan>());
            }
        }

        // ------------------ XÂY DỰNG QUERY VỚI BỘ LỌC ---------------------
        private IQueryable<congVan> BuildFilterQuery(BaoCaoFilterModel filter)
        {
            var query = _context.CongVan.AsQueryable();

            // Lọc theo loại công văn
            if (filter.LoaiCongVan.HasValue)
            {
                query = query.Where(x => x.LoaiCongVan == filter.LoaiCongVan.Value);
            }

            // Lọc theo năm
            if (filter.Nam.HasValue)
            {
                query = query.Where(x => x.Ngay.Year == filter.Nam.Value);
            }

            // Lọc theo tháng
            if (filter.Thang.HasValue)
            {
                query = query.Where(x => x.Ngay.Month == filter.Thang.Value);
            }

            // Lọc theo khoảng thời gian
            if (filter.TuNgay.HasValue)
            {
                query = query.Where(x => x.Ngay >= filter.TuNgay.Value);
            }

            if (filter.DenNgay.HasValue)
            {
                query = query.Where(x => x.Ngay <= filter.DenNgay.Value);
            }

            return query;
        }

        // ------------------ TẢI THỐNG KÊ TỔNG QUAN ---------------------
        private async Task LoadThongKeTongQuan(BaoCaoFilterModel filter)
        {
            try
            {
                var query = BuildFilterQuery(filter);

                // Đếm tổng số công văn theo loại
                ViewBag.TongCongVan = await query.CountAsync();
                ViewBag.SoCongVanDen = await query.CountAsync(x => x.LoaiCongVan == LoaiCongVan.CongVanDen);
                ViewBag.SoCongVanDi = await query.CountAsync(x => x.LoaiCongVan == LoaiCongVan.CongVanDi);

                // Thống kê top 5 nơi phát hành/nhận
                ViewBag.TopNoiPhatHanh = await query
                    .Where(x => x.NoiPhatHanh != null)
                    .GroupBy(x => x.NoiPhatHanh.TenNoiPhatHanh)
                    .Select(g => new { Ten = g.Key, SoLuong = g.Count() })
                    .OrderByDescending(x => x.SoLuong)
                    .Take(5)
                    .ToListAsync();

                ViewBag.TopNoiNhan = await query
                    .Where(x => x.NoiNhan != null)
                    .GroupBy(x => x.NoiNhan.TenNoiNhan)
                    .Select(g => new { Ten = g.Key, SoLuong = g.Count() })
                    .OrderByDescending(x => x.SoLuong)
                    .Take(5)
                    .ToListAsync();

                // Thống kê theo quý
                var currentYear = filter.Nam ?? DateTime.Now.Year;
                var quyThongKe = await query
                    .Where(x => x.Ngay.Year == currentYear)
                    .GroupBy(x => ((x.Ngay.Month - 1) / 3) + 1) // Tính quý: (tháng-1)/3 + 1
                    .Select(g => new { Quy = g.Key, SoLuong = g.Count() })
                    .OrderBy(x => x.Quy)
                    .ToListAsync();

                // Đảm bảo có đủ 4 quý
                var quyData = new List<object>();
                for (int quy = 1; quy <= 4; quy++)
                {
                    var data = quyThongKe.FirstOrDefault(x => x.Quy == quy);
                    quyData.Add(new { Quy = quy, SoLuong = data?.SoLuong ?? 0 });
                }
                ViewBag.ThongKeQuy = quyData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải thống kê tổng quan");
                // Đặt giá trị mặc định nếu có lỗi
                ViewBag.TongCongVan = 0;
                ViewBag.SoCongVanDen = 0;
                ViewBag.SoCongVanDi = 0;
                ViewBag.TopNoiPhatHanh = new List<object>();
                ViewBag.TopNoiNhan = new List<object>();
                ViewBag.ThongKeQuy = new List<object>();
            }
        }

        // ------------------ TẢI THỐNG KÊ THEO THÁNG CHO BIỂU ĐỒ ---------------------
        private async Task LoadThongKeTheoThang(int nam)
        {
            try
            {
                var query = _context.CongVan.Where(x => x.Ngay.Year == nam);

                // Lấy dữ liệu thô
                var rawDen = await query
                    .Where(x => x.LoaiCongVan == LoaiCongVan.CongVanDen)
                    .GroupBy(x => x.Ngay.Month)
                    .Select(g => new { Thang = g.Key, SoLuong = g.Count() })
                    .ToListAsync();

                var rawDi = await query
                    .Where(x => x.LoaiCongVan == LoaiCongVan.CongVanDi)
                    .GroupBy(x => x.Ngay.Month)
                    .Select(g => new { Thang = g.Key, SoLuong = g.Count() })
                    .ToListAsync();

                // Bổ sung đầy đủ 12 tháng
                var thangDenData = new List<object>();
                var thangDiData = new List<object>();
                var thangLabels = new List<string>();

                for (int thang = 1; thang <= 12; thang++)
                {
                    var den = rawDen.FirstOrDefault(x => x.Thang == thang);
                    var di = rawDi.FirstOrDefault(x => x.Thang == thang);

                    thangDenData.Add(new { Thang = thang, SoLuong = den?.SoLuong ?? 0 });
                    thangDiData.Add(new { Thang = thang, SoLuong = di?.SoLuong ?? 0 });
                    thangLabels.Add($"Tháng {thang}");
                }

                ViewBag.ThangDen = thangDenData;
                ViewBag.ThangDi = thangDiData;
                ViewBag.ThangLabels = thangLabels;
                ViewBag.NamThongKe = nam;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải thống kê theo tháng");
                // Đặt giá trị mặc định
                ViewBag.ThangDen = new List<object>();
                ViewBag.ThangDi = new List<object>();
                ViewBag.ThangLabels = new List<string>();
                ViewBag.NamThongKe = nam;
            }
        }

        // ------------------ XUẤT PDF VỚI BỘ LỌC ---------------------
        [HttpGet]
        public async Task<IActionResult> ExportPdf([FromQuery] BaoCaoFilterModel filter)
        {
            try
            {
                var query = BuildFilterQuery(filter);
                
                var data = await query
                    .Include(x => x.NoiPhatHanh)
                    .Include(x => x.NoiNhan)
                    .OrderByDescending(x => x.Ngay)
                    .ThenByDescending(x => x.ID)
                    .ToListAsync();

                if (!data.Any())
                {
                    TempData["ErrorMessage"] = "Không có dữ liệu để xuất báo cáo";
                    return RedirectToAction(nameof(Index));
                }

                var fileName = $"BaoCaoCongVan_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

                var pdfBytes = Document.Create(document =>
                {
                    document.Page(page =>
                    {
                        page.Margin(20);
                        page.Size(PageSizes.A4);
                        page.DefaultTextStyle(x => x.FontSize(10));

                        // HEADER
                        page.Header().Column(col =>
                        {
                            col.Item().Text("BÁO CÁO CÔNG VĂN")
                                .FontSize(16).Bold().AlignCenter();
                            
                            col.Item().PaddingTop(5).Text(text =>
                            {
                                text.Span("Ngày xuất: ").Bold();
                                text.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
                            });
                        });

                        // CONTENT - TABLE VỚI BORDER
                        page.Content().PaddingVertical(10).Table(table =>
                        {
                            // Định nghĩa cột
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(30);  // STT
                                columns.RelativeColumn(1.5f); // Số hiệu
                                columns.RelativeColumn(1);    // Loại
                                columns.RelativeColumn(1.2f); // Ngày
                                columns.RelativeColumn(2);    // Nơi phát hành
                                columns.RelativeColumn(2);    // Nơi nhận
                            });

                            // TABLE HEADER VỚI BORDER
                            table.Header(header =>
                            {
                                // STT
                                header.Cell()
                                    .Background(Colors.Grey.Lighten3)
                                    .Border(1)
                                    .PaddingVertical(5)
                                    .PaddingHorizontal(2)
                                    .AlignCenter()
                                    .Text("STT").Bold();
                                
                                // Số hiệu
                                header.Cell()
                                    .Background(Colors.Grey.Lighten3)
                                    .Border(1)
                                    .PaddingVertical(5)
                                    .PaddingHorizontal(2)
                                    .AlignCenter()
                                    .Text("Số hiệu").Bold();
                                
                                // Loại
                                header.Cell()
                                    .Background(Colors.Grey.Lighten3)
                                    .Border(1)
                                    .PaddingVertical(5)
                                    .PaddingHorizontal(2)
                                    .AlignCenter()
                                    .Text("Loại").Bold();
                                
                                // Ngày
                                header.Cell()
                                    .Background(Colors.Grey.Lighten3)
                                    .Border(1)
                                    .PaddingVertical(5)
                                    .PaddingHorizontal(2)
                                    .AlignCenter()
                                    .Text("Ngày").Bold();
                                
                                // Nơi phát hành
                                header.Cell()
                                    .Background(Colors.Grey.Lighten3)
                                    .Border(1)
                                    .PaddingVertical(5)
                                    .PaddingHorizontal(2)
                                    .AlignCenter()
                                    .Text("Nơi phát hành").Bold();
                                
                                // Nơi nhận
                                header.Cell()
                                    .Background(Colors.Grey.Lighten3)
                                    .Border(1)
                                    .PaddingVertical(5)
                                    .PaddingHorizontal(2)
                                    .AlignCenter()
                                    .Text("Nơi nhận").Bold();
                            });

                            // TABLE ROWS VỚI BORDER
                            int stt = 1;
                            foreach (var cv in data)
                            {
                                // STT
                                table.Cell()
                                    .Border(1)
                                    .BorderColor(Colors.Grey.Lighten1)
                                    .PaddingVertical(5)
                                    .PaddingHorizontal(2)
                                    .AlignCenter()
                                    .Text(stt++.ToString());
                                
                                // Số hiệu
                                table.Cell()
                                    .Border(1)
                                    .BorderColor(Colors.Grey.Lighten1)
                                    .PaddingVertical(5)
                                    .PaddingHorizontal(2)
                                    .AlignCenter()
                                    .Text(cv.SoHieu ?? "");
                                
                                // Loại
                                table.Cell()
                                    .Border(1)
                                    .BorderColor(Colors.Grey.Lighten1)
                                    .PaddingVertical(5)
                                    .PaddingHorizontal(2)
                                    .AlignCenter()
                                    .Text(cv.LoaiCongVan.ToString());
                                
                                // Ngày
                                table.Cell()
                                    .Border(1)
                                    .BorderColor(Colors.Grey.Lighten1)
                                    .PaddingVertical(5)
                                    .PaddingHorizontal(2)
                                    .AlignCenter()
                                    .Text(cv.Ngay.ToString("dd/MM/yyyy"));
                                
                                // Nơi phát hành
                                table.Cell()
                                    .Border(1)
                                    .BorderColor(Colors.Grey.Lighten1)
                                    .PaddingVertical(5)
                                    .PaddingHorizontal(2)
                                    .AlignLeft()
                                    .Text(cv.NoiPhatHanh?.TenNoiPhatHanh ?? "");
                                
                                // Nơi nhận
                                table.Cell()
                                    .Border(1)
                                    .BorderColor(Colors.Grey.Lighten1)
                                    .PaddingVertical(5)
                                    .PaddingHorizontal(2)
                                    .AlignLeft()
                                    .Text(cv.NoiNhan?.TenNoiNhan ?? "");
                            }
                        });

                        // FOOTER
                        page.Footer().AlignCenter().Column(col =>
                        {
                            col.Item().Text($"Tổng số: {data.Count} công văn").FontSize(9);
                            col.Item().Text(text =>
                            {
                                text.Span("Trang ");
                                text.CurrentPageNumber();
                                text.Span(" / ");
                                text.TotalPages();
                            });
                        });
                    });
                }).GeneratePdf();

                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xuất PDF");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xuất báo cáo PDF";
                return RedirectToAction(nameof(Index));
            }
        }

        // ------------------ XUẤT EXCEL VỚI BỘ LỌC ---------------------
        [HttpGet]
        public async Task<IActionResult> ExportExcel([FromQuery] BaoCaoFilterModel filter)
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                // Sử dụng cùng bộ lọc
                var query = BuildFilterQuery(filter);

                var data = await query
                    .Include(x => x.NoiPhatHanh)
                    .Include(x => x.NoiNhan)
                    .OrderByDescending(x => x.Ngay)
                    .ThenByDescending(x => x.ID)
                    .ToListAsync();

                if (!data.Any())
                {
                    TempData["ErrorMessage"] = "Không có dữ liệu để xuất báo cáo";
                    return RedirectToAction(nameof(Index), new { filter.Nam, filter.Thang, filter.LoaiCongVan });
                }

                using var package = new ExcelPackage();
                
                // Worksheet 1: Danh sách công văn
                var wsData = package.Workbook.Worksheets.Add("DanhSachCongVan");

                // Header
                wsData.Cells["A1"].Value = "STT";
                wsData.Cells["B1"].Value = "Số hiệu";
                wsData.Cells["C1"].Value = "Loại";
                wsData.Cells["D1"].Value = "Ngày";
                wsData.Cells["E1"].Value = "Nơi phát hành";
                wsData.Cells["F1"].Value = "Nơi nhận";

                using (var range = wsData.Cells["A1:G1"])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }

                // Data
                int row = 2, stt = 1;
                foreach (var cv in data)
                {
                    wsData.Cells[row, 1].Value = stt++;
                    wsData.Cells[row, 2].Value = cv.SoHieu;
                    wsData.Cells[row, 3].Value = cv.LoaiCongVan.ToString();
                    wsData.Cells[row, 4].Value = cv.Ngay;
                    wsData.Cells[row, 4].Style.Numberformat.Format = "dd/MM/yyyy";
                    wsData.Cells[row, 5].Value = cv.NoiPhatHanh?.TenNoiPhatHanh;
                    wsData.Cells[row, 6].Value = cv.NoiNhan?.TenNoiNhan;
                    row++;
                }

                // Tự động điều chỉnh độ rộng cột
                wsData.Cells[wsData.Dimension.Address].AutoFitColumns();

                // Worksheet 2: Thống kê
                var wsThongKe = package.Workbook.Worksheets.Add("ThongKe");

                // Thống kê tổng quan
                wsThongKe.Cells["A1"].Value = "THỐNG KÊ CÔNG VĂN";
                wsThongKe.Cells["A1:D1"].Merge = true;
                wsThongKe.Cells["A1"].Style.Font.Bold = true;
                wsThongKe.Cells["A1"].Style.Font.Size = 14;
                wsThongKe.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                wsThongKe.Cells["A3"].Value = "Tổng số công văn:";
                wsThongKe.Cells["B3"].Value = data.Count;
                wsThongKe.Cells["A4"].Value = "Công văn đến:";
                wsThongKe.Cells["B4"].Value = data.Count(x => x.LoaiCongVan == LoaiCongVan.CongVanDen);
                wsThongKe.Cells["A5"].Value = "Công văn đi:";
                wsThongKe.Cells["B5"].Value = data.Count(x => x.LoaiCongVan == LoaiCongVan.CongVanDi);

                // Format thống kê
                for (int i = 3; i <= 5; i++)
                {
                    wsThongKe.Cells[$"A{i}"].Style.Font.Bold = true;
                    wsThongKe.Cells[$"B{i}"].Style.Font.Bold = true;
                }

                // Tạo tên file động
                var fileName = $"BaoCaoCongVan_{DateTime.Now:yyyyMMdd_HHmmss}";
                if (filter.LoaiCongVan.HasValue)
                {
                    fileName += $"_{filter.LoaiCongVan.Value}";
                }
                if (filter.Nam.HasValue)
                {
                    fileName += $"_{filter.Nam.Value}";
                }
                fileName += ".xlsx";

                var excelBytes = package.GetAsByteArray();
                return File(excelBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                     "file.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xuất Excel");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xuất báo cáo Excel";
                return RedirectToAction(nameof(Index));
            }
        }

        // ------------------ API CHO BIỂU ĐỒ ---------------------
        [HttpGet]
        public async Task<IActionResult> GetChartData([FromQuery] int nam)
        {
            try
            {
                var thongKe = await _context.CongVan
                    .Where(x => x.Ngay.Year == nam)
                    .GroupBy(x => new { x.Ngay.Month, x.LoaiCongVan })
                    .Select(g => new
                    {
                        Thang = g.Key.Month,
                        Loai = g.Key.LoaiCongVan,
                        SoLuong = g.Count()
                    })
                    .ToListAsync();

                var result = new
                {
                    Labels = Enumerable.Range(1, 12).Select(m => $"Tháng {m}").ToArray(),
                    CongVanDen = Enumerable.Range(1, 12)
                        .Select(m => thongKe
                            .Where(x => x.Thang == m && x.Loai == LoaiCongVan.CongVanDen)
                            .Sum(x => x.SoLuong))
                        .ToArray(),
                    CongVanDi = Enumerable.Range(1, 12)
                        .Select(m => thongKe
                            .Where(x => x.Thang == m && x.Loai == LoaiCongVan.CongVanDi)
                            .Sum(x => x.SoLuong))
                        .ToArray()
                };

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy dữ liệu biểu đồ");
                return Json(new { error = "Có lỗi xảy ra" });
            }
        }
    }
}