using Microsoft.AspNetCore.Mvc;
using WebApp.Services;
using System.Threading.Tasks;

namespace WebApp.Controllers
{
    public class ThongBaoController : Controller
    {
        private readonly ThongBaoService _service;

        public ThongBaoController(ThongBaoService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index()
        {
            var list = await _service.GetAllAsync();
            return View(list);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            await _service.MarkAsReadAsync(id);
            return Json(new { success = true });
        }
        [HttpGet]
        public async Task<IActionResult> GetThongBao()
        {
            var thongbaos = await _service.GetAllAsync();

            var list = thongbaos
                .OrderByDescending(t => t.NgayTao)
                .Take(10)
                .Select(t => new
                {
                    t.IdThongBao,
                    t.TieuDe,
                    NgayTao = t.NgayTao != DateTime.MinValue
                    ? t.NgayTao.ToString("dd/MM/yyyy HH:mm")
                    : "",
                    t.DaXem
                })
                .ToList();

            return Json(list);
        }
    }
}
