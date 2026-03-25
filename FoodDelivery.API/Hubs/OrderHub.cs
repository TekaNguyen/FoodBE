using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer; // 👈 1. Nhớ thêm thư viện này

namespace FoodDelivery.API.Hubs
{
    public class OrderHub : Hub
    {
        // ---------------------------------------------------------
        // 🟢 DÀNH CHO KHÁCH & BẾP (Không cần Login hoặc Login lỏng lẻo)
        // ---------------------------------------------------------
        public async Task JoinOrderGroup(string orderId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Order-{orderId}");
        }

        public async Task LeaveOrderGroup(string orderId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Order-{orderId}");
        }

        public async Task JoinKitchenChannel()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "KitchenChannel");
            Console.WriteLine($"👨‍🍳 [KITCHEN] Bếp đã online: {Context.ConnectionId}");
        }

        // ---------------------------------------------------------
        // 🔴 DÀNH CHO ADMIN (Yêu cầu bảo mật cao)
        // ---------------------------------------------------------

        // 👇 2. ĐÂY LÀ CHÌA KHÓA KHẮC PHỤC LỖI:
        // AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme
        // Dòng này ra lệnh: "Đừng tìm Cookie nữa, hãy dùng Token JWT đi!"
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "SuperAdmin,Admin,Staff")]
        public async Task JoinAdminChannel()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "AdminChannel");

            var username = Context.User?.Identity?.Name ?? "Admin";
            Console.WriteLine($"👮 [ADMIN] {username} đã kết nối Dashboard.");
        }

        // ---------------------------------------------------------
        // 🛠 LOG DEBUG (Giúp bạn biết Server nhận được gì)
        // ---------------------------------------------------------
        public override async Task OnConnectedAsync()
        {
            var user = Context.User;
            // Nếu True: Token đã được chấp nhận
            Console.WriteLine($"--> [SignalR] Connection: {Context.ConnectionId} | Authenticated: {user?.Identity?.IsAuthenticated}");

            if (user?.Identity?.IsAuthenticated == true)
            {
                foreach (var claim in user.Claims)
                {
                    // Kiểm tra xem server đọc được role là "role" hay "http://schemas..."
                    Console.WriteLine($"    Claim: {claim.Type} : {claim.Value}");
                }
            }
            await base.OnConnectedAsync();
        }
    }
}