using FoodDelivery.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using FoodDelivery.API.DTOs; // Thêm dòng này

namespace FoodDelivery.API.Controllers
{
    // Nếu bạn muốn lưu vào DB thì dùng Entity, còn không thì dùng List cứng trong Controller này cũng được
    // Nhưng chuẩn API First thì nên trả về từ Backend.

    [Route("api/[controller]")]
    [ApiController]
    public class CannedReplyController : ControllerBase
    {
        // Giả lập Database (Hoặc bạn có thể tạo bảng CannedReplies trong DB thực tế)
        // ✅ CÁCH MỚI (C# 12 / .NET 8): Dùng ngoặc vuông
        private static List<string> _replies =
        [
            "Xin chào! Tôi có thể giúp gì cho bạn?",
    "Đơn hàng của bạn đang được nhà bếp chuẩn bị.",
    "Shipper đã nhận đơn và đang trên đường giao.",
    "Cảm ơn bạn đã ủng hộ quán!",
    "Dạ món này bên em hôm nay tạm hết ạ."
        ];

        // 1. Lấy danh sách tin nhắn mẫu
        [HttpGet]
        public IActionResult GetReplies()
        {
            return Ok(_replies);
        }

        // 2. Thêm tin nhắn mẫu mới (Dành cho Admin cấu hình)
        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public IActionResult AddReply([FromBody] AddReplyRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Content)) return BadRequest("Nội dung không được để trống");
            _replies.Add(req.Content);
            return Ok(new { message = "Đã thêm tin nhắn mẫu" });
        }
    }
}