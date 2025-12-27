using FoodDelivery.Domain.Entities;
using FoodDelivery.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using FoodDelivery.API.DTOs; // Nhớ đảm bảo đã tạo file AssignRequest.cs trong DTOs

namespace FoodDelivery.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController(AppDbContext context) : ControllerBase
    {
        // 1. Lấy danh sách hội thoại
        [HttpGet("conversations")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> GetConversations()
        {
            var conversations = await context.Conversations
                .OrderByDescending(c => c.LastMessageAt)
                .Select(c => new
                {
                    GuestId = c.SessionId,
                    Nickname = c.UserId ?? "Khách ẩn danh",
                    Info = c.EmailOrPhone ?? "",
                    DisplayName = c.CustomName ?? c.UserId ?? "Khách lạ",
                    c.LastMessageAt,
                    // Bổ sung hiển thị người đang phụ trách (Assignment)
                    AssignedTo = c.AssignedToUserName,
                    LastMessage = context.ChatMessages
                        .Where(m => m.ConversationId == c.Id)
                        .OrderByDescending(m => m.CreatedAt)
                        .Select(m => m.Content)
                        .FirstOrDefault() ?? "..."
                })
                .ToListAsync();

            return Ok(conversations);
        }

        // 2. Lấy tin nhắn chi tiết
        [HttpGet("messages/{guestId}")]
        public async Task<IActionResult> GetMessages(string guestId)
        {
            var messages = await context.ChatMessages
                .Include(m => m.Conversation)
                .Where(m => m.Conversation != null && m.Conversation.SessionId == guestId)
                .OrderBy(m => m.CreatedAt)
                .Select(m => new
                {
                    Sender = m.SenderId,
                    Type = m.SenderId == "Admin" ? "mine" : "theirs",
                    Text = m.Content ?? "",
                    Time = m.CreatedAt,
                    m.MessageType
                })
                .ToListAsync();

            return Ok(messages);
        }

        // 3. Đổi tên ghi chú khách hàng
        [HttpPost("rename")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> RenameUser([FromBody] RenameRequest request)
        {
            var conv = await context.Conversations.FirstOrDefaultAsync(c => c.SessionId == request.GuestId);
            if (conv == null) return NotFound("Không tìm thấy hội thoại.");

            conv.CustomName = request.NewName;
            await context.SaveChangesAsync();

            return Ok(new { message = "Cập nhật tên thành công!" });
        }

        // 4. Xóa hội thoại
        [HttpDelete("conversation/{guestId}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> DeleteConversation(string guestId)
        {
            var conv = await context.Conversations.FirstOrDefaultAsync(c => c.SessionId == guestId);
            if (conv == null) return NotFound("Không tìm thấy hội thoại để xóa.");

            var messages = context.ChatMessages.Where(m => m.ConversationId == conv.Id);
            context.ChatMessages.RemoveRange(messages);
            context.Conversations.Remove(conv);

            await context.SaveChangesAsync();
            return Ok(new { message = "Đã xóa hội thoại thành công" });
        }

        // 5. Upload ảnh chat
        [HttpPost("upload")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("Vui lòng chọn file ảnh.");

            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "chats");
            if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Ok(new { url = $"/uploads/chats/{fileName}" });
        }

        // 6. Gán hội thoại (Assignment) - Đã chỉnh sửa chuẩn
        [HttpPost("assign")]
        [Authorize(Roles = "Admin,SuperAdmin,Staff")]
        public async Task<IActionResult> AssignConversation(
            [FromBody] AssignRequest req,
            [FromServices] IHubContext<FoodDelivery.API.Hubs.ChatHub> _hubContext)
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var adminName = User.Identity?.Name ?? "Admin";

            var conv = await context.Conversations.FirstOrDefaultAsync(c => c.SessionId == req.GuestId);
            if (conv == null) return NotFound("Không tìm thấy hội thoại.");

            // Kiểm tra tranh chấp: Nếu đã có người nhận VÀ người đó không phải mình
            if (!string.IsNullOrEmpty(conv.AssignedToUserId) && conv.AssignedToUserId != adminId)
            {
                return BadRequest($"Hội thoại này đã được {conv.AssignedToUserName} tiếp nhận rồi!");
            }

            conv.AssignedToUserId = adminId;
            conv.AssignedToUserName = adminName;

            await context.SaveChangesAsync();

            // Bắn SignalR
            await _hubContext.Clients.Group("AdminChannel")
                .SendAsync("ConversationAssigned", req.GuestId, adminId, adminName);

            return Ok(new { message = $"Đã gán hội thoại cho {adminName}" });
        }
    }

    // --- DTO CLASSES (Đặt ở cuối file hoặc tách ra file riêng) ---
    public class RenameRequest
    {
        public string GuestId { get; set; } = string.Empty;
        public string NewName { get; set; } = string.Empty;
    }

    // Nếu bạn chưa tách file AssignRequest.cs thì bỏ comment dòng dưới đây:
    // public class AssignRequest { public string GuestId { get; set; } = ""; }
}