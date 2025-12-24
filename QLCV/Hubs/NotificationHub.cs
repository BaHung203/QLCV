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

        // Gửi thông báo (dùng object để frontend dễ nhận)
        public async Task SendNotification(object notification)
        {
            await Clients.All.SendAsync("ReceiveNotification", notification);
        }

        // Gửi số liệu thống kê (giữ nguyên)
        public async Task UpdateDocumentCount(int incoming, int outgoing, int employeeCount)
        {
            await Clients.All.SendAsync("UpdateDocumentCount", incoming, outgoing, employeeCount);
        }

        // Client yêu cầu lấy số liệu hiện tại
        public async Task GetDocumentCount()
        {
            var employeeCount = await _context.nhanVien.CountAsync();
            var incomingCount = await _context.CongVan
                .CountAsync(c => c.LoaiCongVan == LoaiCongVan.CongVanDen);
            var outgoingCount = await _context.CongVan
                .CountAsync(c => c.LoaiCongVan == LoaiCongVan.CongVanDi);

            await Clients.Caller.SendAsync("UpdateDocumentCount", incomingCount, outgoingCount, employeeCount);
        }

        // === Group (rất quan trọng cho tương lai) ===
        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).SendAsync("UserJoined", Context.ConnectionId);
        }

        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }

        // Tự động gọi khi client kết nối (tùy chọn)
        public override async Task OnConnectedAsync()
        {
            // Ví dụ: tự động thêm vào nhóm theo phòng ban của user
            // var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            // var phongBan = await _context.Users...;
            // await Groups.AddToGroupAsync(Context.ConnectionId, $"PhongBan_{phongBanId}");

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // Có thể log hoặc xóa khỏi group nếu cần
            await base.OnDisconnectedAsync(exception);
        }
    }
}