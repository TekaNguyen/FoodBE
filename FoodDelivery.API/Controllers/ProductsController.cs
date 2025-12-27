using FoodDelivery.API.DTOs;
using FoodDelivery.Domain.Entities;
using FoodDelivery.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using CsvHelper;
using FoodDelivery.API.Services;

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
            int pageSize = 10;
            var products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(products);
        }

        // ==========================================
        // 2. GET DETAIL (FULL OPTION: Size/Topping)
        // ==========================================
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDetailDto>> GetProductById(int id)
        {
            // 👇 Query "Full Option": Lấy Món -> Kèm Group -> Kèm Option con
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

            // 👇 Map sang DTO (Dùng cú pháp C# 12 Collection Expressions)
            var productDto = new ProductDetailDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                ImageUrl = product.ImageUrl,
                StockQuantity = product.StockQuantity,

                // 🟢 THAY ĐỔI: Dùng [.. ] thay vì .ToList()
                OptionGroups = [.. product.OptionGroups.Select(g => new OptionGroupDto
                {
                    Id = g.Id,
                    Name = g.Name,
                    IsRequired = g.IsRequired,
                    AllowMultiple = g.AllowMultiple,
                    
                    // 🟢 THAY ĐỔI: Dùng [.. ] cho cả danh sách con
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
        // 3. CREATE PRODUCT (Admin)
        // ==========================================
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateProduct([FromForm] CreateProductDto request)
        {
            string imagePath = await SaveImage(request.ImageFile);

            var product = new Product
            {
                Name = request.Name,
                Description = request.Description ?? "",
                Price = request.Price,
                CategoryId = request.CategoryId,
                ImageUrl = string.IsNullOrEmpty(imagePath) ? "/images/default.png" : imagePath,
                StockQuantity = 0, // Mặc định 0
                IsActive = true
                // ⚠️ Không gán CreatedAt, để Database tự lo
            };

            context.Products.Add(product);
            await context.SaveChangesAsync();

            // Trả về ID vừa tạo
            return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, new { id = product.Id, name = product.Name });
        }

        // ==========================================
        // 4. UPDATE PRODUCT (Admin)
        // ==========================================
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateProduct(int id, [FromForm] UpdateProductDto request)
        {
            var product = await context.Products.FindAsync(id);
            if (product == null) return NotFound("Không tìm thấy món ăn");

            // Cập nhật thông tin
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

            // Xử lý ảnh mới
            if (request.ImageFile != null)
            {
                // Xóa ảnh cũ (nếu không phải ảnh default)
                if (!string.IsNullOrEmpty(product.ImageUrl) && !product.ImageUrl.Contains("default.png"))
                {
                    DeleteImageFile(product.ImageUrl);
                }
                product.ImageUrl = await SaveImage(request.ImageFile);
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
        // 6. DELETE PRODUCT
        // ==========================================
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await context.Products.FindAsync(id);
            if (product == null) return NotFound("Không tìm thấy món ăn");

            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                DeleteImageFile(product.ImageUrl);
            }

            context.Products.Remove(product);
            await context.SaveChangesAsync();

            return Ok(new { message = "Đã xóa sản phẩm và ảnh thành công!" });
        }

        // ==========================================
        // 7. IMPORT CSV (Fixed DateTime issue)
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

                        // 👇 Cập nhật: KHÔNG GÁN CreatedAt, để SQL tự xử lý
                        // CreatedAt = DateTime.UtcNow, <--- XÓA DÒNG NÀY

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

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(extension)) return "";

            var fileName = $"{Guid.NewGuid()}{extension}";
            var uploadFolder = Path.Combine(env.WebRootPath, "images");
            var filePath = Path.Combine(uploadFolder, fileName);

            if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/images/{fileName}";
        }

        private void DeleteImageFile(string imageUrl)
        {
            try
            {
                var relativePath = imageUrl.TrimStart('/');
                var filePath = Path.Combine(env.WebRootPath, relativePath);

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
            catch
            {
                // Ignored
            }
        }

        [HttpPost("{id}/upload-image")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> UploadImage(int id, IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("Vui lòng chọn ảnh.");

            var product = await context.Products.FindAsync(id);
            if (product == null) return NotFound("Sản phẩm không tồn tại.");

            // Tạo thư mục nếu chưa có
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "products");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            // Tạo tên file duy nhất
            var fileName = $"prod_{id}_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            // Lưu file vào ổ cứng server
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Cập nhật đường dẫn vào database
            product.ImageUrl = $"/uploads/products/{fileName}";
            await context.SaveChangesAsync();

            return Ok(new { imageUrl = product.ImageUrl });
        }

        [Route("api/[controller]")]
        [ApiController]
        // Sử dụng Primary Constructor để tiêm context và service vào (không cần gạch dưới _)
        public class ProductController(AppDbContext context, IFileUploadService fileUploadService) : ControllerBase
        {
            [HttpPost]
            [Authorize(Roles = "Admin")]
            public async Task<IActionResult> CreateProduct([FromForm] ProductCreateDto dto)
            {
                try
                {
                    string? imageUrl = null;

                    if (dto.ImageFile != null)
                    {
                        // Sử dụng fileUploadService trực tiếp từ constructor
                        imageUrl = await fileUploadService.UploadFileAsync(dto.ImageFile, "products");
                    }

                    var product = new Product
                    {
                        Name = dto.Name,
                        Price = dto.Price,
                        // Sử dụng null-coalescing để tránh gán null vào trường không cho phép null trong DB
                        ImageUrl = imageUrl ?? "",
                        CategoryId = dto.CategoryId,
                        Description = dto.Description ?? ""
                    };

                    context.Products.Add(product);
                    await context.SaveChangesAsync();

                    return Ok(new { message = "Thêm sản phẩm thành công", data = product });
                }
                catch (Exception ex)
                {
                    // Trả về đối tượng JSON để Frontend dễ xử lý hơn là chuỗi thuần
                    return BadRequest(new { error = ex.Message });
                }
            }
        }
    }
}

