using Microsoft.AspNetCore.Mvc;

namespace FoodDelivery.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // IWebHostEnvironment giúp ta lấy đường dẫn tới thư mục gốc của server
    public class FilesController(IWebHostEnvironment env) : ControllerBase
    {
        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            // 1. Kiểm tra xem có file không
            if (file == null || file.Length == 0)
                return BadRequest("Vui lòng chọn file ảnh!");

            // 2. Tạo thư mục 'wwwroot/uploads' nếu chưa có
            // RootPath = E:\FoodDelivery\...\FoodDelivery.API\wwwroot
            var uploadsFolder = Path.Combine(env.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // 3. Tạo tên file độc nhất (tránh bị trùng đè file cũ)
            // Ví dụ: avatar.jpg -> 550e8400-e29b..._avatar.jpg
            var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // 4. Lưu file vào ổ cứng
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // 5. Tạo URL để trả về cho Client
            // URL: http://localhost:xxxx/uploads/ten-file.jpg
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var fileUrl = $"{baseUrl}/uploads/{uniqueFileName}";

            return Ok(new { url = fileUrl });
        }
    }
}