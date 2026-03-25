using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
<<<<<<< HEAD
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
=======

namespace FoodDelivery.API.Hubs
{
    // Để AllowAnonymous ở class level để Kitchen (không cần login) vẫn vào được
    // Ta sẽ chặn quyền ở từng hàm cụ thể sau
    public class OrderHub : Hub
    {
        // =========================================================
        // 🟢 1. DÀNH CHO KHÁCH HÀNG (THEO DÕI ĐƠN CỦA MÌNH)
        // =========================================================
        public async Task JoinOrderGroup(string orderId)
        {
            // Group này nhận tin: "Đơn hàng đã chuyển sang trạng thái Cooking/Delivering"
            string groupName = $"Order-{orderId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            // Console.WriteLine($"--> Khách theo dõi đơn: {orderId}");
>>>>>>> 3a66952c690791e0f7b9f8d0898e8c787cfc5a29
        }

        public async Task LeaveOrderGroup(string orderId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Order-{orderId}");
        }

<<<<<<< HEAD
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
=======
        // =========================================================
        // 🟠 2. DÀNH CHO NHÀ BẾP (KITCHEN MONITOR)
        // =========================================================
        // Không dùng [Authorize] để màn hình bếp (kitchen.html) dễ dàng kết nối
        public async Task JoinKitchenChannel()
        {
            // 👇 QUAN TRỌNG: Tên group phải là "Kitchen" (Khớp với OrdersController)
            await Groups.AddToGroupAsync(Context.ConnectionId, "Kitchen");
            Console.WriteLine($"👨‍🍳 [KITCHEN] Bếp đã online: {Context.ConnectionId}");
        }

        // =========================================================
        // 🔴 3. DÀNH CHO ADMIN QUẢN LÝ (DASHBOARD)
        // =========================================================
        // Admin cần bảo mật, phải có Token mới được nghe
        [Authorize(Roles = "SuperAdmin,Admin,Staff,Shipper")]
        public async Task JoinAdminOrderChannel()
        {
            // 👇 QUAN TRỌNG: Tên group phải là "AdminOrders" (Khớp với OrdersController)
            await Groups.AddToGroupAsync(Context.ConnectionId, "AdminOrders");

            var username = Context.User?.Identity?.Name ?? "Admin";
            Console.WriteLine($"👮 [ADMIN] {username} đã vào xem đơn hàng.");
        }
    }
}

//using Microsoft.AspNetCore.SignalR;
//using Microsoft.AspNetCore.Authorization; // 👇 Thêm cái này để check quyền Admin

//namespace FoodDelivery.API.Hubs
//{
//    public class OrderHub : Hub
//    {
//        // =========================================================
//        // 🟢 DÀNH CHO KHÁCH HÀNG (Giữ nguyên cấu trúc cũ)
//        // =========================================================

//        // 1. Khách hàng tham gia vào "phòng" của đơn hàng cụ thể
//        public async Task JoinOrderGroup(string orderId)
//        {
//            string groupName = $"Order-{orderId}";

//            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

//            Console.WriteLine($"--> User {Context.ConnectionId} đã tham gia vào nhóm: {groupName}");
//        }

//        // 2. Hàm rời nhóm
//        public async Task LeaveOrderGroup(string orderId)
//        {
//            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Order-{orderId}");
//        }

//        // =========================================================
//        // 🔴 DÀNH CHO ADMIN / BẾP (Bổ sung cho Phase 3)
//        // =========================================================

//        // 3. Admin/Bếp tham gia kênh "Tổng đài" để nghe ngóng TẤT CẢ đơn
//        // Chỉ cho phép user có quyền Admin, Staff hoặc Shipper gọi hàm này
//        [Authorize(Roles = "Admin,Staff,Shipper,SuperAdmin")]
//        public async Task JoinAdminChannel()
//        {
//            string adminGroup = "AdminChannel";

//            await Groups.AddToGroupAsync(Context.ConnectionId, adminGroup);

//            // Log tên User (lấy từ Token) để biết ai vừa vào trực tổng đài
//            var username = Context.User?.Identity?.Name ?? "Admin ẩn danh";
//            Console.WriteLine($"--> QUẢN LÝ: {username} ({Context.ConnectionId}) đã tham gia kênh AdminChannel");
//        }
//    }
//}
>>>>>>> 3a66952c690791e0f7b9f8d0898e8c787cfc5a29
