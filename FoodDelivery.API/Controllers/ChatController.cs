using FoodDelivery.Domain.Entities;
using FoodDelivery.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
<<<<<<< HEAD
using FoodDelivery.API.DTOs; 
=======
using FoodDelivery.API.DTOs; // Nhớ đảm bảo đã tạo file AssignRequest.cs trong DTOs
>>>>>>> 3a66952c690791e0f7b9f8d0898e8c787cfc5a29

namespace FoodDelivery.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController(AppDbContext context) : ControllerBase
    {
<<<<<<< HEAD
        // ============================================================
        // 1. LẤY DANH SÁCH HỘI THOẠI
        // ============================================================
        [HttpGet("conversations")]
        [Authorize(Roles = "Admin,SuperAdmin,Staff")] // Staff cũng cần xem để chat
        public async Task<IActionResult> GetConversations()
        {
            var conversations = await context.Conversations
                .AsNoTracking() // ⚡ Tối ưu hiệu năng cho query chỉ đọc
=======
        // 1. Lấy danh sách hội thoại
        [HttpGet("conversations")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> GetConversations()
        {
            var conversations = await context.Conversations
>>>>>>> 3a66952c690791e0f7b9f8d0898e8c787cfc5a29
                .OrderByDescending(c => c.LastMessageAt)
                .Select(c => new
                {
                    GuestId = c.SessionId,
                    Nickname = c.UserId ?? "Khách ẩn danh",
                    Info = c.EmailOrPhone ?? "",
                    DisplayName = c.CustomName ?? c.UserId ?? "Khách lạ",
                    c.LastMessageAt,
<<<<<<< HEAD
                    AssignedTo = c.AssignedToUserName, // Người đang phụ trách
=======
                    // Bổ sung hiển thị người đang phụ trách (Assignment)
                    AssignedTo = c.AssignedToUserName,
>>>>>>> 3a66952c690791e0f7b9f8d0898e8c787cfc5a29
                    LastMessage = context.ChatMessages
                        .Where(m => m.ConversationId == c.Id)
                        .OrderByDescending(m => m.CreatedAt)
                        .Select(m => m.Content)
                        .FirstOrDefault() ?? "..."
                })
                .ToListAsync();

            return Ok(conversations);
        }

<<<<<<< HEAD
        // ============================================================
        // 2. LẤY CHI TIẾT TIN NHẮN
        // ============================================================
        [HttpGet("messages/{guestId}")]
        [Authorize(Roles = "Admin,SuperAdmin,Staff,Customer")] // 🔒 BẢO MẬT: Chỉ nhân viên mới xem được
        public async Task<IActionResult> GetMessages(string guestId)
        {
            var messages = await context.ChatMessages
                .AsNoTracking()
=======
        // 2. Lấy tin nhắn chi tiết
        [HttpGet("messages/{guestId}")]
        public async Task<IActionResult> GetMessages(string guestId)
        {
            var messages = await context.ChatMessages
>>>>>>> 3a66952c690791e0f7b9f8d0898e8c787cfc5a29
                .Include(m => m.Conversation)
                .Where(m => m.Conversation != null && m.Conversation.SessionId == guestId)
                .OrderBy(m => m.CreatedAt)
                .Select(m => new
                {
                    Sender = m.SenderId,
<<<<<<< HEAD
                    Type = m.SenderId == "Admin" ? "mine" : "theirs", // Logic hiển thị bên trái/phải
                    Text = m.Content ?? "",
                    Time = m.CreatedAt,
                    m.MessageType // TEXT hoặc IMAGE
=======
                    Type = m.SenderId == "Admin" ? "mine" : "theirs",
                    Text = m.Content ?? "",
                    Time = m.CreatedAt,
                    m.MessageType
>>>>>>> 3a66952c690791e0f7b9f8d0898e8c787cfc5a29
                })
                .ToListAsync();

            return Ok(messages);
        }

<<<<<<< HEAD
        // ============================================================
        // 3. ĐỔI TÊN GỢI NHỚ KHÁCH HÀNG
        // ============================================================
        [HttpPost("rename")]
        [Authorize(Roles = "Admin,SuperAdmin,Staff")]
=======
        // 3. Đổi tên ghi chú khách hàng
        [HttpPost("rename")]
        [Authorize(Roles = "Admin,SuperAdmin")]
>>>>>>> 3a66952c690791e0f7b9f8d0898e8c787cfc5a29
        public async Task<IActionResult> RenameUser([FromBody] RenameRequest request)
        {
            var conv = await context.Conversations.FirstOrDefaultAsync(c => c.SessionId == request.GuestId);
            if (conv == null) return NotFound("Không tìm thấy hội thoại.");

            conv.CustomName = request.NewName;
            await context.SaveChangesAsync();

            return Ok(new { message = "Cập nhật tên thành công!" });
        }

<<<<<<< HEAD
        // ============================================================
        // 4. XÓA HỘI THOẠI
        // ============================================================
        [HttpDelete("conversation/{guestId}")]
        [Authorize(Roles = "Admin,SuperAdmin,Staff")] // Chỉ Admin mới được xóa
=======
        // 4. Xóa hội thoại
        [HttpDelete("conversation/{guestId}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
>>>>>>> 3a66952c690791e0f7b9f8d0898e8c787cfc5a29
        public async Task<IActionResult> DeleteConversation(string guestId)
        {
            var conv = await context.Conversations.FirstOrDefaultAsync(c => c.SessionId == guestId);
            if (conv == null) return NotFound("Không tìm thấy hội thoại để xóa.");

<<<<<<< HEAD
            // Xóa hết tin nhắn con trước (Nếu DB chưa set Cascade Delete)
            var messages = context.ChatMessages.Where(m => m.ConversationId == conv.Id);
            context.ChatMessages.RemoveRange(messages);
            
=======
            var messages = context.ChatMessages.Where(m => m.ConversationId == conv.Id);
            context.ChatMessages.RemoveRange(messages);
>>>>>>> 3a66952c690791e0f7b9f8d0898e8c787cfc5a29
            context.Conversations.Remove(conv);

            await context.SaveChangesAsync();
            return Ok(new { message = "Đã xóa hội thoại thành công" });
        }

<<<<<<< HEAD
        // ============================================================
        // 5. UPLOAD ẢNH CHAT
        // ============================================================
        [HttpPost("upload")]
        [Authorize(Roles = "Admin,SuperAdmin,Staff, Customer")]
=======
        // 5. Upload ảnh chat
        [HttpPost("upload")]
        [Authorize(Roles = "Admin,SuperAdmin")]
>>>>>>> 3a66952c690791e0f7b9f8d0898e8c787cfc5a29
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("Vui lòng chọn file ảnh.");

<<<<<<< HEAD
            // 🛡️ SECURITY: Kiểm tra đuôi file để tránh upload file độc hại
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest("Chỉ cho phép upload file ảnh (jpg, png, gif, webp).");
            }

            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "chats");
            if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

            var fileName = $"{Guid.NewGuid()}{extension}";
=======
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "chats");
            if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
>>>>>>> 3a66952c690791e0f7b9f8d0898e8c787cfc5a29
            var filePath = Path.Combine(uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

<<<<<<< HEAD
            // Trả về đường dẫn tương đối
            return Ok(new { url = $"/uploads/chats/{fileName}" });
        }

        // ============================================================
        // 6. GÁN HỘI THOẠI (ASSIGNMENT)
        // ============================================================
=======
            return Ok(new { url = $"/uploads/chats/{fileName}" });
        }

        // 6. Gán hội thoại (Assignment) - Đã chỉnh sửa chuẩn
>>>>>>> 3a66952c690791e0f7b9f8d0898e8c787cfc5a29
        [HttpPost("assign")]
        [Authorize(Roles = "Admin,SuperAdmin,Staff")]
        public async Task<IActionResult> AssignConversation(
            [FromBody] AssignRequest req,
            [FromServices] IHubContext<FoodDelivery.API.Hubs.ChatHub> _hubContext)
        {
<<<<<<< HEAD
            // 🛠 FIX: Lấy UserID an toàn (check cả NameIdentifier, uid, id)
            var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                          ?? User.FindFirst("uid")?.Value 
                          ?? User.FindFirst("id")?.Value;
                          
            var adminName = User.Identity?.Name ?? "Nhân viên";

            if (string.IsNullOrEmpty(adminId)) return Unauthorized("Không xác định được danh tính người dùng.");
=======
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var adminName = User.Identity?.Name ?? "Admin";
>>>>>>> 3a66952c690791e0f7b9f8d0898e8c787cfc5a29

            var conv = await context.Conversations.FirstOrDefaultAsync(c => c.SessionId == req.GuestId);
            if (conv == null) return NotFound("Không tìm thấy hội thoại.");

<<<<<<< HEAD
            // Kiểm tra tranh chấp: Nếu đã có người khác nhận rồi
=======
            // Kiểm tra tranh chấp: Nếu đã có người nhận VÀ người đó không phải mình
>>>>>>> 3a66952c690791e0f7b9f8d0898e8c787cfc5a29
            if (!string.IsNullOrEmpty(conv.AssignedToUserId) && conv.AssignedToUserId != adminId)
            {
                return BadRequest($"Hội thoại này đã được {conv.AssignedToUserName} tiếp nhận rồi!");
            }

<<<<<<< HEAD
            // Cập nhật người phụ trách
=======
>>>>>>> 3a66952c690791e0f7b9f8d0898e8c787cfc5a29
            conv.AssignedToUserId = adminId;
            conv.AssignedToUserName = adminName;

            await context.SaveChangesAsync();

<<<<<<< HEAD
            // 📡 Bắn SignalR thông báo cho các Admin khác cập nhật giao diện
=======
            // Bắn SignalR
>>>>>>> 3a66952c690791e0f7b9f8d0898e8c787cfc5a29
            await _hubContext.Clients.Group("AdminChannel")
                .SendAsync("ConversationAssigned", req.GuestId, adminId, adminName);

            return Ok(new { message = $"Đã gán hội thoại cho {adminName}" });
        }
    }

<<<<<<< HEAD
    // --- DTO CLASSES ---
=======
    // --- DTO CLASSES (Đặt ở cuối file hoặc tách ra file riêng) ---
>>>>>>> 3a66952c690791e0f7b9f8d0898e8c787cfc5a29
    public class RenameRequest
    {
        public string GuestId { get; set; } = string.Empty;
        public string NewName { get; set; } = string.Empty;
    }

<<<<<<< HEAD
    // Nếu bạn chưa tạo file DTO riêng thì có thể dùng class này
    public class AssignRequest 
    { 
        public string GuestId { get; set; } = string.Empty; 
    }
=======
    // Nếu bạn chưa tách file AssignRequest.cs thì bỏ comment dòng dưới đây:
    // public class AssignRequest { public string GuestId { get; set; } = ""; }
>>>>>>> 3a66952c690791e0f7b9f8d0898e8c787cfc5a29
}