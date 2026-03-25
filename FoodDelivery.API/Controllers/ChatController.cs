using FoodDelivery.Domain.Entities;
using FoodDelivery.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using FoodDelivery.API.DTOs; 

namespace FoodDelivery.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController(AppDbContext context) : ControllerBase
    {
        // ============================================================
        // 1. LẤY DANH SÁCH HỘI THOẠI
        // ============================================================
        [HttpGet("conversations")]
        [Authorize(Roles = "Admin,SuperAdmin,Staff")] // Staff cũng cần xem để chat
        public async Task<IActionResult> GetConversations()
        {
            var conversations = await context.Conversations
                .AsNoTracking() // ⚡ Tối ưu hiệu năng cho query chỉ đọc
                .OrderByDescending(c => c.LastMessageAt)
                .Select(c => new
                {
                    GuestId = c.SessionId,
                    Nickname = c.UserId ?? "Khách ẩn danh",
                    Info = c.EmailOrPhone ?? "",
                    DisplayName = c.CustomName ?? c.UserId ?? "Khách lạ",
                    c.LastMessageAt,
                    AssignedTo = c.AssignedToUserName, // Người đang phụ trách
                    LastMessage = context.ChatMessages
                        .Where(m => m.ConversationId == c.Id)
                        .OrderByDescending(m => m.CreatedAt)
                        .Select(m => m.Content)
                        .FirstOrDefault() ?? "..."
                })
                .ToListAsync();

            return Ok(conversations);
        }

        // ============================================================
        // 2. LẤY CHI TIẾT TIN NHẮN
        // ============================================================
        [HttpGet("messages/{guestId}")]
        [Authorize(Roles = "Admin,SuperAdmin,Staff,Customer")] // 🔒 BẢO MẬT: Chỉ nhân viên mới xem được
        public async Task<IActionResult> GetMessages(string guestId)
        {
            var messages = await context.ChatMessages
                .AsNoTracking()
                .Include(m => m.Conversation)
                .Where(m => m.Conversation != null && m.Conversation.SessionId == guestId)
                .OrderBy(m => m.CreatedAt)
                .Select(m => new
                {
                    Sender = m.SenderId,
                    Type = m.SenderId == "Admin" ? "mine" : "theirs", // Logic hiển thị bên trái/phải
                    Text = m.Content ?? "",
                    Time = m.CreatedAt,
                    m.MessageType // TEXT hoặc IMAGE
                })
                .ToListAsync();

            return Ok(messages);
        }

        // ============================================================
        // 3. ĐỔI TÊN GỢI NHỚ KHÁCH HÀNG
        // ============================================================
        [HttpPost("rename")]
        [Authorize(Roles = "Admin,SuperAdmin,Staff")]
        public async Task<IActionResult> RenameUser([FromBody] RenameRequest request)
        {
            var conv = await context.Conversations.FirstOrDefaultAsync(c => c.SessionId == request.GuestId);
            if (conv == null) return NotFound("Không tìm thấy hội thoại.");

            conv.CustomName = request.NewName;
            await context.SaveChangesAsync();

            return Ok(new { message = "Cập nhật tên thành công!" });
        }

        // ============================================================
        // 4. XÓA HỘI THOẠI
        // ============================================================
        [HttpDelete("conversation/{guestId}")]
        [Authorize(Roles = "Admin,SuperAdmin,Staff")] // Chỉ Admin mới được xóa
        public async Task<IActionResult> DeleteConversation(string guestId)
        {
            var conv = await context.Conversations.FirstOrDefaultAsync(c => c.SessionId == guestId);
            if (conv == null) return NotFound("Không tìm thấy hội thoại để xóa.");

            // Xóa hết tin nhắn con trước (Nếu DB chưa set Cascade Delete)
            var messages = context.ChatMessages.Where(m => m.ConversationId == conv.Id);
            context.ChatMessages.RemoveRange(messages);
            
            context.Conversations.Remove(conv);

            await context.SaveChangesAsync();
            return Ok(new { message = "Đã xóa hội thoại thành công" });
        }

        // ============================================================
        // 5. UPLOAD ẢNH CHAT
        // ============================================================
        [HttpPost("upload")]
        [Authorize(Roles = "Admin,SuperAdmin,Staff, Customer")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("Vui lòng chọn file ảnh.");

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
            var filePath = Path.Combine(uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Trả về đường dẫn tương đối
            return Ok(new { url = $"/uploads/chats/{fileName}" });
        }

        // ============================================================
        // 6. GÁN HỘI THOẠI (ASSIGNMENT)
        // ============================================================
        [HttpPost("assign")]
        [Authorize(Roles = "Admin,SuperAdmin,Staff")]
        public async Task<IActionResult> AssignConversation(
            [FromBody] AssignRequest req,
            [FromServices] IHubContext<FoodDelivery.API.Hubs.ChatHub> _hubContext)
        {
            // 🛠 FIX: Lấy UserID an toàn (check cả NameIdentifier, uid, id)
            var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                          ?? User.FindFirst("uid")?.Value 
                          ?? User.FindFirst("id")?.Value;
                          
            var adminName = User.Identity?.Name ?? "Nhân viên";

            if (string.IsNullOrEmpty(adminId)) return Unauthorized("Không xác định được danh tính người dùng.");

            var conv = await context.Conversations.FirstOrDefaultAsync(c => c.SessionId == req.GuestId);
            if (conv == null) return NotFound("Không tìm thấy hội thoại.");

            // Kiểm tra tranh chấp: Nếu đã có người khác nhận rồi
            if (!string.IsNullOrEmpty(conv.AssignedToUserId) && conv.AssignedToUserId != adminId)
            {
                return BadRequest($"Hội thoại này đã được {conv.AssignedToUserName} tiếp nhận rồi!");
            }

            // Cập nhật người phụ trách
            conv.AssignedToUserId = adminId;
            conv.AssignedToUserName = adminName;

            await context.SaveChangesAsync();

            // 📡 Bắn SignalR thông báo cho các Admin khác cập nhật giao diện
            await _hubContext.Clients.Group("AdminChannel")
                .SendAsync("ConversationAssigned", req.GuestId, adminId, adminName);

            return Ok(new { message = $"Đã gán hội thoại cho {adminName}" });
        }
    }

    // --- DTO CLASSES ---
    public class RenameRequest
    {
        public string GuestId { get; set; } = string.Empty;
        public string NewName { get; set; } = string.Empty;
    }

    // Nếu bạn chưa tạo file DTO riêng thì có thể dùng class này
    public class AssignRequest 
    { 
        public string GuestId { get; set; } = string.Empty; 
    }
}