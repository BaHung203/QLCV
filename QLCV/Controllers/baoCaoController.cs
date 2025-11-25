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

namespace WebApp.Controllers
{
    public class baoCaoController : Controller
    {
        private readonly AppDbContext _context;

        public baoCaoController(AppDbContext context)
        {
            _context = context;
        }

        // ------------------ TRANG INDEX ---------------------
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var congVan = await _context.CongVan
                .Include(x => x.NoiPhatHanh)
                .Include(x => x.NoiNhan)
                .ToListAsync();

            ViewBag.SoDen = congVan.Count(x => x.LoaiCongVan == LoaiCongVan.CongVanDen);
            ViewBag.SoDi = congVan.Count(x => x.LoaiCongVan == LoaiCongVan.CongVanDi);

            ViewBag.ThangDen = congVan
                .Where(x => x.LoaiCongVan == LoaiCongVan.CongVanDen)
                .GroupBy(x => x.Ngay.Month)
                .Select(g => new { Thang = g.Key, SoLuong = g.Count() })
                .OrderBy(x => x.Thang)
                .ToList();

            ViewBag.ThangDi = congVan
                .Where(x => x.LoaiCongVan == LoaiCongVan.CongVanDi)
                .GroupBy(x => x.Ngay.Month)
                .Select(g => new { Thang = g.Key, SoLuong = g.Count() })
                .OrderBy(x => x.Thang)
                .ToList();

            return View(congVan);
        }


        // ------------------ XUẤT PDF (QuestPDF) ---------------------
        [HttpGet]
        public async Task<IActionResult> ExportPdf()
        {
            var data = await _context.CongVan
                .Include(x => x.NoiPhatHanh)
                .Include(x => x.NoiNhan)
                .ToListAsync();

            var pdfBytes = Document.Create(document =>
            {
                document.Page(page =>
                {
                    page.Margin(20);
                    page.Size(PageSizes.A4);

                    page.Header().Text("BÁO CÁO CÔNG VĂN")
                        .FontSize(20).Bold().AlignCenter();

                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.ConstantColumn(40);  // STT
                            cols.RelativeColumn();    // Số hiệu
                            cols.RelativeColumn();    // Loại
                            cols.RelativeColumn();    // Ngày
                            cols.RelativeColumn();    // Nơi phát hành
                            cols.RelativeColumn();    // Nơi nhận
                        });

                        // Header
                        table.Header(header =>
                        {
                            header.Cell().Text("STT").Bold();
                            header.Cell().Text("Số hiệu").Bold();
                            header.Cell().Text("Loại").Bold();
                            header.Cell().Text("Ngày").Bold();
                            header.Cell().Text("Phát hành").Bold();
                            header.Cell().Text("Nơi nhận").Bold();
                        });

                        int stt = 1;
                        foreach (var cv in data)
                        {
                            table.Cell().Text(stt++.ToString());
                            table.Cell().Text(cv.SoHieu ?? "");
                            table.Cell().Text(cv.LoaiCongVan.ToString());
                            table.Cell().Text(cv.Ngay.ToString("dd/MM/yyyy"));
                            table.Cell().Text(cv.NoiPhatHanh?.TenNoiPhatHanh ?? "");
                            table.Cell().Text(cv.NoiNhan?.TenNoiNhan ?? "");
                        }
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Trang ");
                        x.CurrentPageNumber();
                    });
                });
            }).GeneratePdf();

            return File(pdfBytes, "application/pdf", "BaoCaoCongVan.pdf");
        }



        // ------------------ XUẤT EXCEL (EPPlus) ---------------------
        [HttpGet]
        public async Task<IActionResult> ExportExcel()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var data = await _context.CongVan
                .Include(x => x.NoiPhatHanh)
                .Include(x => x.NoiNhan)
                .ToListAsync();

            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("BaoCaoCongVan");

            // Header
            ws.Cells["A1"].Value = "STT";
            ws.Cells["B1"].Value = "Số hiệu";
            ws.Cells["C1"].Value = "Loại";
            ws.Cells["D1"].Value = "Ngày";
            ws.Cells["E1"].Value = "Nơi phát hành";
            ws.Cells["F1"].Value = "Nơi nhận";

            using (var range = ws.Cells["A1:F1"])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            }

            int row = 2, stt = 1;
            foreach (var cv in data)
            {
                ws.Cells[row, 1].Value = stt++;
                ws.Cells[row, 2].Value = cv.SoHieu;
                ws.Cells[row, 3].Value = cv.LoaiCongVan.ToString();
                ws.Cells[row, 4].Value = cv.Ngay.ToString("dd/MM/yyyy");
                ws.Cells[row, 5].Value = cv.NoiPhatHanh?.TenNoiPhatHanh;
                ws.Cells[row, 6].Value = cv.NoiNhan?.TenNoiNhan;
                row++;
            }

            ws.Cells.AutoFitColumns();

            var excelBytes = package.GetAsByteArray();
            return File(excelBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "BaoCaoCongVan.xlsx");
        }
    }
}
