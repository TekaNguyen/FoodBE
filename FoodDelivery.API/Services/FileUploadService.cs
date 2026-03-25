namespace FoodDelivery.API.Services;
public class FileUploadService(IWebHostEnvironment env) : IFileUploadService
{
    // Danh sách các đuôi file ảnh cho phép
    private readonly string[] _allowedExtensions = [".jpg", ".jpeg", ".png", ".gif", ".webp"];
    private readonly string[] _allowedMimeTypes = ["image/jpeg", "image/png", "image/gif", "image/webp"];

    public async Task<string> UploadFileAsync(IFormFile file, string subFolder)
    {
        if (file == null || file.Length == 0) return string.Empty;

        // 1. Kiểm tra dung lượng file (Ví dụ: tối đa 5MB)
        if (file.Length > 5 * 1024 * 1024)
            throw new Exception("Dung lượng file không được vượt quá 5MB.");

        // 2. Kiểm tra đuôi file (Extension)
        var extension = Path.GetExtension(file.FileName).ToLower();
        if (!_allowedExtensions.Contains(extension))
            throw new Exception("Định dạng file không được phép. Chỉ chấp nhận .jpg, .png, .gif, .webp");

        // 3. Kiểm tra loại nội dung (MIME Type) - Tăng thêm 1 tầng bảo mật
        if (!_allowedMimeTypes.Contains(file.ContentType.ToLower()))
            throw new Exception("Loại file không hợp lệ.");

        // --- Bắt đầu quá trình lưu file nếu vượt qua các bước kiểm tra ---

        var rootPath = env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var folderPath = Path.Combine(rootPath, "uploads", subFolder);

        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

        // Tạo tên file ngẫu nhiên để tránh ghi đè và ẩn danh tên file gốc
        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(folderPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return $"/uploads/{subFolder}/{fileName}";
    }

    public void DeleteFile(string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath)) return;
        var rootPath = env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var fullPath = Path.Combine(rootPath, relativePath.TrimStart('/'));
        if (File.Exists(fullPath)) File.Delete(fullPath);
    }
}