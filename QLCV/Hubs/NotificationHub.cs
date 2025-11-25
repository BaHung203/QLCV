using Microsoft.AspNetCore.SignalR;
using WebApp.Data;
using Microsoft.EntityFrameworkCore;
using WebApp.Libs;

namespace WebApp.Hubs
{
    public class NotificationHub : Hub
    {
        private readonly AppDbContext _context;

        public NotificationHub(AppDbContext context)
        {
            _context = context;
        }

        public async Task UpdateDocumentCount(int incoming, int outgoing, int employeeCount)
        {
            await Clients.All.SendAsync("UpdateDocumentCount", incoming, outgoing, employeeCount);
        }

        public async Task GetDocumentCount()
        {
            int employeeCount = await _context.nhanVien.CountAsync();
            int incomingCount = await _context.CongVan.CountAsync(c => c.LoaiCongVan == LoaiCongVan.CongVanDen);
            int outgoingCount = await _context.CongVan.CountAsync(c => c.LoaiCongVan == LoaiCongVan.CongVanDi);
            await Clients.Caller.SendAsync("UpdateDocumentCount", incomingCount, outgoingCount, employeeCount);
        }
        public async Task SendNotification(string title, string content)
        {
            await Clients.All.SendAsync("ReceiveNotification", title, content);
        }
        // public override async Task OnConnectedAsync()
        // {
        //    await Clients.Caller.SendAsync("ReceiveNotification", "? Test k?t n?i SignalR th�nh c�ng!", 1);
        //    await base.OnConnectedAsync();
        // }

    }
}