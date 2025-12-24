using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using QLCV.Models;
using WebApp.Data;
using Microsoft.EntityFrameworkCore;
using WebApp.Hubs;
using Microsoft.AspNetCore.SignalR;   // ← Dòng này chính là thủ phạm!

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
        // GET: /Home/TestNotification
        public async Task<IActionResult> TestSignalR([FromServices] IHubContext<NotificationHub> hubContext)
        {
            await hubContext.Clients.All.SendAsync("ReceiveNotification", new
            {
                tieuDe = "TEST THÀNH CÔNG!",
                noiDung = "SignalR đang hoạt động hoàn hảo!",
                ngayTao = DateTime.Now.ToString("HH:mm dd/MM/yyyy")
            });

            return Ok("Đã gửi thông báo test! Kiểm tra chuông thông báo trên trang nhé!");
        }
    }
}