using Microsoft.AspNetCore.SignalR;
using WebApp.Hubs;
using WebApp.Data;
using WebApp.Models;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Services
{
    public class ThongBaoService
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;

        public ThongBaoService(AppDbContext context, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task<List<ThongBao>> GetAllAsync()
        {
            return await _context.ThongBao
                .OrderByDescending(t => t.NgayTao)
                .ToListAsync();
        }

        public async Task AddAsync(string tieuDe, string noiDung, int? idCongVan = null)
        {
            var tb = new ThongBao
            {
                TieuDe = tieuDe,
                NoiDung = noiDung,
                NgayTao = DateTime.Now,
                DaXem = false,
                IdCongVan = idCongVan
            };

            _context.ThongBao.Add(tb);
            await _context.SaveChangesAsync();

            // ?? G?i thông báo realtime t?i client
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", new
            {
                tb.TieuDe,
                NgayTao = tb.NgayTao.ToString("dd/MM/yyyy HH:mm")
            });
        }

        public async Task MarkAsReadAsync(int id)
        {
            var tb = await _context.ThongBao.FindAsync(id);
            if (tb != null)
            {
                tb.DaXem = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}
