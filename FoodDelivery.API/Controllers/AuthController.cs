using FoodDelivery.API.DTOs;
using FoodDelivery.API.Common;
using FoodDelivery.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FoodDelivery.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration) : ControllerBase
    {
        // ==========================================
        // 1. API ĐĂNG KÝ (REGISTER)
        // ==========================================
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto request)
        {
            // 1. Kiểm tra Email tồn tại
            var existingUser = await userManager.FindByEmailAsync(request.Email);
            if (existingUser != null) return BadRequest("Email này đã được sử dụng.");

            // 2. Tạo User object
            var user = new AppUser
            {
                UserName = request.Email,
                Email = request.Email,
                FullName = request.FullName
            };

            // 3. Insert vào DB
            var result = await userManager.CreateAsync(user, request.Password);

            if (result.Succeeded)
            {
                // 4. LOGIC TỰ ĐỘNG CẤP QUYỀN (Đã tối ưu so sánh chuỗi)
                // 👇 SỬA DÒNG NÀY: Dùng string.Equals với OrdinalIgnoreCase
                if (string.Equals(request.Email, "admin@food.com", StringComparison.OrdinalIgnoreCase))
                {
                    if (!await roleManager.RoleExistsAsync(AppRoles.SuperAdmin))
                        await roleManager.CreateAsync(new IdentityRole(AppRoles.SuperAdmin));

                    await userManager.AddToRoleAsync(user, AppRoles.SuperAdmin);
                }
                else
                {
                    // Người thường -> Customer
                    await userManager.AddToRoleAsync(user, AppRoles.Customer);
                }

                return Ok(new { message = $"Đăng ký thành công! Chào mừng {request.FullName}" });
            }

            return BadRequest(result.Errors);
        }

        // ==========================================
        // 2. API ĐĂNG NHẬP (LOGIN)
        // ==========================================
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto request)
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user == null) return Unauthorized(new { message = "Tài khoản không tồn tại." });

            var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, false);

            if (result.Succeeded)
            {
                // Tạo Token
                var tokenString = await GenerateJwtToken(user);

                // 👇 QUAN TRỌNG: Trả về 'token' (chữ thường) để khớp với admin.html
                return Ok(new { token = tokenString, email = user.Email, fullName = user.FullName });
            }

            return Unauthorized(new { message = "Mật khẩu không đúng." });
        }

        // ==========================================
        // 3. API TẠO ROLE (Chạy 1 lần để Init DB)
        // ==========================================
        [HttpPost("create-roles")]
        public async Task<IActionResult> CreateRoles()
        {
            string[] roleNames = [AppRoles.SuperAdmin, AppRoles.Merchant, AppRoles.Staff, AppRoles.Shipper, AppRoles.Customer];

            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
            return Ok("Đã khởi tạo xong danh sách Role chuẩn!");
        }
        // ==========================================
        // 4. API PHÂN QUYỀN (Chỉ SuperAdmin mới được dùng)
        // ==========================================
        [HttpPost("assign-role")]
        // 👇 QUAN TRỌNG: Mở dòng này sau khi đã tạo được Admin đầu tiên để bảo mật hệ thống
        // [Authorize(Roles = "SuperAdmin")] 
        public async Task<IActionResult> AssignRole(string email, string roleName)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null) return NotFound("Không tìm thấy User có email này");

            if (!await roleManager.RoleExistsAsync(roleName))
                return BadRequest($"Role '{roleName}' không tồn tại.");

            // Xóa các role cũ (nếu muốn 1 user chỉ có 1 role) - Tuỳ logic dự án
            // await userManager.RemoveFromRolesAsync(user, await userManager.GetRolesAsync(user));

            await userManager.AddToRoleAsync(user, roleName);

            return Ok(new { message = $"Đã cấp quyền {roleName} thành công cho {email}" });
        }

        // ==========================================
        // HÀM PHỤ: TẠO JWT TOKEN
        // ==========================================
        private async Task<string> GenerateJwtToken(AppUser user)
        {
            // 👇 SỬA ĐỔI QUAN TRỌNG: Đọc đúng section "Jwt" trong appsettings.json
            // (Khớp với file cấu hình chúng ta vừa sửa lúc nãy)
            var jwtSection = configuration.GetSection("Jwt");
            var secretKey = jwtSection["Key"];
            var issuer = jwtSection["Issuer"];
            var audience = jwtSection["Audience"];

            if (string.IsNullOrEmpty(secretKey)) throw new Exception("Chưa cấu hình Jwt:Key trong appsettings.json");

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Email, user.Email!),
                new(JwtRegisteredClaimNames.Sub, user.Email!), // Dùng Email làm Sub cho dễ nhìn
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Lấy Role nạp vào Token
            var userRoles = await userManager.GetRolesAsync(user);
            foreach (var role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7), // Token sống 7 ngày
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

//using FoodDelivery.API.DTOs;
//using FoodDelivery.API.Common;
//using FoodDelivery.Domain.Entities;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.IdentityModel.Tokens;
//using System.IdentityModel.Tokens.Jwt;
//using System.Security.Claims;
//using System.Text;
//using Microsoft.AspNetCore.Authorization; // Thêm thư viện này cho [Authorize]

//namespace FoodDelivery.API.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class AuthController(
//        UserManager<AppUser> userManager,
//        SignInManager<AppUser> signInManager,
//        RoleManager<IdentityRole> roleManager,
//        IConfiguration configuration) : ControllerBase
//    {
//        // ==========================================
//        // 1. API ĐĂNG KÝ (REGISTER)
//        // ==========================================
//        [HttpPost("register")]
//        public async Task<IActionResult> Register(RegisterDto request)
//        {
//            // 1. Kiểm tra Email tồn tại
//            var existingUser = await userManager.FindByEmailAsync(request.Email);
//            if (existingUser != null) return BadRequest("Email này đã được sử dụng.");

//            // 2. Tạo User object
//            var user = new AppUser
//            {
//                UserName = request.Email,
//                Email = request.Email,
//                FullName = request.FullName
//            };

//            // 3. Insert vào DB
//            var result = await userManager.CreateAsync(user, request.Password);

//            if (result.Succeeded)
//            {
//                // 4. Mặc định gán quyền Customer
//                await userManager.AddToRoleAsync(user, AppRoles.Customer);
//                return Ok(new { message = $"Đăng ký thành công! Chào mừng {request.FullName}" });
//            }

//            // Trả về lỗi chi tiết nếu thất bại (VD: Mật khẩu yếu)
//            return BadRequest(result.Errors);
//        }

//        // ==========================================
//        // 2. API ĐĂNG NHẬP (LOGIN)
//        // ==========================================
//        [HttpPost("login")]
//        public async Task<IActionResult> Login(LoginDto request)
//        {
//            var user = await userManager.FindByEmailAsync(request.Email);
//            if (user == null) return Unauthorized("Tài khoản hoặc mật khẩu không đúng.");

//            // Tham số thứ 3 (lockoutOnFailure): false -> Không khóa nick nếu sai nhiều lần (để true nếu muốn bảo mật cao)
//            var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, false);

//            if (result.Succeeded)
//            {
//                var tokenString = await GenerateJwtToken(user);
//                return Ok(new { Token = tokenString });
//            }

//            return Unauthorized("Tài khoản hoặc mật khẩu không đúng.");
//        }

//        // ==========================================
//        // 3. API TẠO ROLE (Chạy 1 lần để Init DB)
//        // ==========================================
//        [HttpPost("create-roles")]
//        public async Task<IActionResult> CreateRoles()
//        {
//            string[] roleNames = [AppRoles.SuperAdmin, AppRoles.Merchant, AppRoles.Staff, AppRoles.Shipper, AppRoles.Customer];

//            foreach (var roleName in roleNames)
//            {
//                if (!await roleManager.RoleExistsAsync(roleName))
//                {
//                    await roleManager.CreateAsync(new IdentityRole(roleName));
//                }
//            }
//            return Ok("Đã khởi tạo xong danh sách Role chuẩn!");
//        }

//        // ==========================================
//        // 4. API PHÂN QUYỀN (Chỉ SuperAdmin mới được dùng)
//        // ==========================================
//        [HttpPost("assign-role")]
//        // 👇 QUAN TRỌNG: Mở dòng này sau khi đã tạo được Admin đầu tiên để bảo mật hệ thống
//        // [Authorize(Roles = "SuperAdmin")] 
//        public async Task<IActionResult> AssignRole(string email, string roleName)
//        {
//            var user = await userManager.FindByEmailAsync(email);
//            if (user == null) return NotFound("Không tìm thấy User có email này");

//            if (!await roleManager.RoleExistsAsync(roleName))
//                return BadRequest($"Role '{roleName}' không tồn tại.");

//            // Xóa các role cũ (nếu muốn 1 user chỉ có 1 role) - Tuỳ logic dự án
//            // await userManager.RemoveFromRolesAsync(user, await userManager.GetRolesAsync(user));

//            await userManager.AddToRoleAsync(user, roleName);

//            return Ok(new { message = $"Đã cấp quyền {roleName} thành công cho {email}" });
//        }

//        // ==========================================
//        // HÀM PHỤ: TẠO JWT TOKEN
//        // ==========================================
//        private async Task<string> GenerateJwtToken(AppUser user)
//        {
//            // Lấy Config từ appsettings.json
//            var secretKey = configuration["JwtSettings:Secret"] ?? throw new InvalidOperationException("Secret Key missing");
//            var issuer = configuration["JwtSettings:Issuer"];
//            var audience = configuration["JwtSettings:Audience"];

//            var claims = new List<Claim>
//            {
//                new(ClaimTypes.NameIdentifier, user.Id),
//                new(ClaimTypes.Email, user.Email!),
//                new(JwtRegisteredClaimNames.Sub, user.UserName!),
//                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // ID Token duy nhất
//            };

//            // 👇 Đã sửa: Chỉ giữ lại 1 vòng lặp lấy Role
//            var userRoles = await userManager.GetRolesAsync(user);
//            foreach (var role in userRoles)
//            {
//                claims.Add(new Claim(ClaimTypes.Role, role));
//            }

//            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
//            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

//            var token = new JwtSecurityToken(
//                issuer: issuer,
//                audience: audience,
//                claims: claims,
//                expires: DateTime.Now.AddDays(7), // Token sống 7 ngày
//                signingCredentials: credentials);

//            return new JwtSecurityTokenHandler().WriteToken(token);
//        }
//    }
//}
