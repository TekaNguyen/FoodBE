using FoodDelivery.API.DTOs;
using FoodDelivery.Domain.Entities;
using FoodDelivery.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using CsvHelper;
// using FoodDelivery.API.Services; // Nếu không dùng Service riêng thì comment lại

namespace FoodDelivery.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController(AppDbContext context, IWebHostEnvironment env) : ControllerBase
    {
        // ==========================================
        // 1. GET ALL (Có Search, Filter, Pagination)
        // ==========================================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts(
            string? search,       // Tìm kiếm tên
            decimal? fromPrice,   // Giá từ
            decimal? toPrice,     // Giá đến
            string? sortBy,       // Sắp xếp
            int page = 1          // Trang số mấy
        )
        {
            var query = context.Products
                .Include(p => p.Category) // Kèm thông tin danh mục
                .AsNoTracking()           // ⚡ Tối ưu tốc độ (Chỉ đọc)
                .AsQueryable();

            // 1. Tìm kiếm (Case-insensitive)
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => EF.Functions.ILike(p.Name, $"%{search}%"));
            }

            // 2. Lọc giá
            if (fromPrice.HasValue) query = query.Where(p => p.Price >= fromPrice.Value);
            if (toPrice.HasValue) query = query.Where(p => p.Price <= toPrice.Value);

            // 3. Sắp xếp
            query = sortBy switch
            {
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                "newest" => query.OrderByDescending(p => p.CreatedAt), // Mới nhất
                _ => query.OrderBy(p => p.Name) // Mặc định A-Z
            };

            // 4. Phân trang
            int pageSize = 16;
            var products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(products);
        }

        // ==========================================
        // 2. GET DETAIL (FULL OPTION)
        // ==========================================
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDetailDto>> GetProductById(int id)
        {
            var product = await context.Products
                .Include(p => p.Category)
                .Include(p => p.OptionGroups)
                    .ThenInclude(g => g.ProductOptions)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound(new { message = "Không tìm thấy món ăn này!" });
            }

            // Map sang DTO
            var productDto = new ProductDetailDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                ImageUrl = product.ImageUrl,
                StockQuantity = product.StockQuantity,
                OptionGroups = [.. product.OptionGroups.Select(g => new OptionGroupDto
                {
                    Id = g.Id,
                    Name = g.Name,
                    IsRequired = g.IsRequired,
                    AllowMultiple = g.AllowMultiple,
                    Options = [.. g.ProductOptions.Select(o => new ProductOptionDto
                    {
                        Id = o.Id,
                        Name = o.Name,
                        PriceModifier = o.PriceModifier
                    })]
                })]
            };

            return Ok(productDto);
        }

        // ==========================================
        // 3. CREATE PRODUCT (Hỗ trợ nhiều ảnh & Thứ tự ảnh)
        // ==========================================
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateProduct([FromForm] ProductCreateDto request)
        {
            try
            {
                // 1. Tạo list để chứa các đường dẫn ảnh
                var uploadedImagePaths = new List<string>();

                // 2. Xử lý danh sách file 
                // QUAN TRỌNG: Dùng foreach để giữ đúng thứ tự ảnh đại diện (ảnh đầu tiên)
                if (request.ImageFiles != null && request.ImageFiles.Count > 0)
                {
                    foreach (var file in request.ImageFiles)
                    {
                        if (file.Length > 0)
                        {
                            string path = await SaveImage(file);
                            uploadedImagePaths.Add(path);
                        }
                    }
                }

                var product = new Product
                {
                    Name = request.Name,
                    Description = request.Description ?? "",
                    Price = request.Price,
                    CategoryId = request.CategoryId,

                    // 3. Nối các đường dẫn lại bằng dấu chấm phẩy ";"
                    ImageUrl = uploadedImagePaths.Count > 0
                        ? string.Join(";", uploadedImagePaths)
                        : "/images/default.png",

                    StockQuantity = 0, // Mặc định 0
                    IsActive = true
                };

                context.Products.Add(product);
                await context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, new { id = product.Id, name = product.Name });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Lỗi tạo sản phẩm", error = ex.Message });
            }
        }

        // ==========================================
        // 4. UPDATE PRODUCT (Cập nhật & Thêm ảnh vào Slide)
        // ==========================================
        // Trong ProductsController.cs

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateProduct(int id, [FromForm] UpdateProductDto request)
        {
            var product = await context.Products.FindAsync(id);
            if (product == null) return NotFound("Không tìm thấy món ăn");

            // Cập nhật thông tin cơ bản
            if (!string.IsNullOrEmpty(request.Name)) product.Name = request.Name;
            if (!string.IsNullOrEmpty(request.Description)) product.Description = request.Description;
            if (request.Price.HasValue) product.Price = request.Price.Value;
            if (request.CategoryId.HasValue) product.CategoryId = request.CategoryId.Value;

            // Cập nhật tồn kho
            if (request.StockQuantity.HasValue)
            {
                product.StockQuantity = request.StockQuantity.Value;
                if (product.StockQuantity > 0) product.IsActive = true;
            }

            // 👇 XỬ LÝ ẢNH (Đã sửa để khớp với List<IFormFile>)
            if (request.ImageFiles != null && request.ImageFiles.Count > 0)
            {
                var newPaths = new List<string>();

                // Bây giờ request.ImageFiles là List nên mới dùng foreach được
                foreach (var file in request.ImageFiles)
                {
                    if (file.Length > 0)
                    {
                        // Upload và lấy đường dẫn
                        string path = await SaveImage(file);
                        newPaths.Add(path);
                    }
                }

                if (newPaths.Count > 0)
                {
                    // Logic nối thêm ảnh vào danh sách cũ
                    if (string.IsNullOrEmpty(product.ImageUrl) || product.ImageUrl.Contains("default.png"))
                    {
                        product.ImageUrl = string.Join(";", newPaths);
                    }
                    else
                    {
                        product.ImageUrl += ";" + string.Join(";", newPaths);
                    }
                }
            }

            await context.SaveChangesAsync();
            return Ok(new { message = "Cập nhật thành công!", product });
        }

        // ==========================================
        // 5. UPDATE STOCK (Nhập kho nhanh - PATCH)
        // ==========================================
        [HttpPatch("stock/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStock(int id, [FromBody] int newQuantity)
        {
            var product = await context.Products.FindAsync(id);
            if (product == null) return NotFound("Không tìm thấy món!");

            product.StockQuantity = newQuantity;
            if (newQuantity > 0) product.IsActive = true;

            await context.SaveChangesAsync();

            return Ok(new
            {
                message = $"Đã cập nhật kho món '{product.Name}' lên {newQuantity} suất.",
                currentStock = newQuantity,
                isActive = product.IsActive
            });
        }

        // ==========================================
        // 6. DELETE PRODUCT (Xóa sạch ảnh liên quan)
        // ==========================================
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await context.Products.FindAsync(id);
            if (product == null) return NotFound("Không tìm thấy món ăn");

            // Xóa tất cả ảnh vật lý
            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                // Tách chuỗi ảnh để xóa từng cái
                var images = product.ImageUrl.Split(';');
                foreach (var img in images)
                {
                    DeleteImageFile(img);
                }
            }

            context.Products.Remove(product);
            await context.SaveChangesAsync();

            return Ok(new { message = "Đã xóa sản phẩm và ảnh thành công!" });
        }

        // ==========================================
        // 7. IMPORT CSV
        // ==========================================
        [HttpPost("import-csv")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> ImportProducts(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("Vui lòng chọn file CSV!");
            if (!file.FileName.EndsWith(".csv")) return BadRequest("Chỉ chấp nhận file định dạng .csv");

            try
            {
                using var stream = new StreamReader(file.OpenReadStream());
                using var csv = new CsvReader(stream, CultureInfo.InvariantCulture);

                var records = csv.GetRecords<ProductImportDto>().ToList();
                var productsToAdd = new List<Product>();

                foreach (var item in records)
                {
                    productsToAdd.Add(new Product
                    {
                        Name = item.Name,
                        Price = item.Price,
                        Description = item.Description ?? "",
                        CategoryId = item.CategoryId,
                        ImageUrl = string.IsNullOrEmpty(item.ImageUrl) ? "/images/default.png" : item.ImageUrl,
                        StockQuantity = 0,
                        IsActive = true
                    });
                }

                if (productsToAdd.Count > 0)
                {
                    await context.Products.AddRangeAsync(productsToAdd);
                    await context.SaveChangesAsync();
                }

                return Ok(new
                {
                    message = $"Đã nhập thành công {productsToAdd.Count} sản phẩm!",
                    data = productsToAdd
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi khi đọc file CSV: {ex.Message}");
            }
        }

        // ==========================================
        // PRIVATE HELPER METHODS
        // ==========================================
        private async Task<string> SaveImage(IFormFile? file)
        {
            if (file == null || file.Length == 0) return "";

            // Thêm ".jfif" vào mảng này
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".jfif" };
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(extension)) return "";

            var fileName = $"{Guid.NewGuid()}{extension}";
            var uploadFolder = Path.Combine(env.WebRootPath, "images");

            if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);

            var filePath = Path.Combine(uploadFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Trả về đường dẫn chuẩn để Frontend dùng (Bắt đầu bằng /images/)
            return $"/images/{fileName}";
        }

        private void DeleteImageFile(string imageUrl)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(imageUrl) || imageUrl.Contains("default.png")) return;

                // Xóa dấu / ở đầu nếu có để kết hợp đường dẫn
                var relativePath = imageUrl.TrimStart('/').Replace("/", "\\");
                var filePath = Path.Combine(env.WebRootPath, relativePath);

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
            catch
            {
                // Ignored - Lỗi xóa file không nên làm crash API
            }
        }
    }
}