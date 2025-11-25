using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using QLCV.Models;
using WebApp.Data;
using Microsoft.EntityFrameworkCore;

namespace QLCV.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.employeeCount = await _context.nhanVien.CountAsync();
            ViewBag.IncomingCount = await _context.CongVan.CountAsync();
            ViewBag.OutgoingCount = await _context.CongVan.CountAsync();
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}