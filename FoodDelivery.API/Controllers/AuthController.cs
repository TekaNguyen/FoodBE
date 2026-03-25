using FoodDelivery.API.DTOs;
using FoodDelivery.API.Common;
using FoodDelivery.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;

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
            var existingUser = await userManager.FindByEmailAsync(request.Email);
            if (existingUser != null) return BadRequest("Email này đã được sử dụng.");

            var user = new AppUser
            {
                UserName = request.Email,
                Email = request.Email,
                FullName = request.FullName
            };

            var result = await userManager.CreateAsync(user, request.Password);

            if (result.Succeeded)
            {
                // LOGIC CẤP QUYỀN
                // Nếu là email admin định sẵn -> SuperAdmin
                if (string.Equals(request.Email, "admin@food.com", StringComparison.OrdinalIgnoreCase))
                {
                    if (!await roleManager.RoleExistsAsync(AppRoles.SuperAdmin))
                        await roleManager.CreateAsync(new IdentityRole(AppRoles.SuperAdmin));

                    await userManager.AddToRoleAsync(user, AppRoles.SuperAdmin);
                }
                else
                {
                    // Mặc định tất cả người khác là Customer
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
                var roles = await userManager.GetRolesAsync(user);
                // Lấy role quan trọng nhất để frontend điều hướng
                var role = roles.Contains(AppRoles.SuperAdmin) ? AppRoles.SuperAdmin :
                           roles.Contains("Admin") ? "Admin" :
                           roles.Contains("Chef") ? "Chef" :
                           roles.FirstOrDefault() ?? "Customer";

                var tokenString = GenerateJwtToken(user, roles);

                return Ok(new
                {
                    token = tokenString,
                    user = new
                    {
                        id = user.Id,
                        email = user.Email,
                        fullName = user.FullName,
                        role // Trả về role để FE redirect (Vào bếp hay vào Dashboard)
                    }
                });
            }

            return Unauthorized(new { message = "Mật khẩu không đúng." });
        }

        // ==========================================
        // 3. API TẠO ROLE (Chạy 1 lần để Init DB)
        // ==========================================
        [HttpPost("create-roles")]
        public async Task<IActionResult> CreateRoles()
        {
            // 👇 ĐÃ BỔ SUNG: Thêm "Admin" và "Chef" vào danh sách khởi tạo
            string[] roleNames = [
                AppRoles.SuperAdmin,
                AppRoles.Merchant,
                AppRoles.Staff,
                AppRoles.Shipper,
                AppRoles.Customer,
                "Admin", // Quản lý chung
                "Chef"   // Bếp trưởng
            ];

            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
            return Ok("Đã khởi tạo xong danh sách Role (Bao gồm cả Chef & Admin)!");
        }

        // ==========================================
        // 4. API PHÂN QUYỀN (Dành cho SuperAdmin)
        // ==========================================
        [HttpPost("assign-role")]
        [Authorize(Roles = "SuperAdmin,Admin")] // 👇 BẢO MẬT: Chỉ Admin mới được cấp quyền
        public async Task<IActionResult> AssignRole(string email, string roleName)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null) return NotFound("Không tìm thấy User có email này");

            if (!await roleManager.RoleExistsAsync(roleName))
                return BadRequest($"Role '{roleName}' không tồn tại.");

            // 👇 QUAN TRỌNG: Xóa quyền cũ trước khi cấp quyền mới
            // Để tránh việc 1 người vừa là Khách vừa là Bếp (gây rối frontend)
            var currentRoles = await userManager.GetRolesAsync(user);
            await userManager.RemoveFromRolesAsync(user, currentRoles);

            await userManager.AddToRoleAsync(user, roleName);

            return Ok(new { message = $"Đã cấp quyền {roleName} thành công cho {email}" });
        }

        // ==========================================
        // HÀM PHỤ: TẠO JWT TOKEN
        // ==========================================
        private string GenerateJwtToken(AppUser user, IList<string> roles)
        {
            var jwtSection = configuration.GetSection("Jwt");
            var secretKey = jwtSection["Key"];
            var issuer = jwtSection["Issuer"];
            var audience = jwtSection["Audience"];

            if (string.IsNullOrEmpty(secretKey)) throw new Exception("Chưa cấu hình Jwt:Key trong appsettings.json");

            var claims = new List<Claim>
            {
                new("sub", user.Id),
                new("email", user.Email!),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (var role in roles)
            {
                claims.Add(new("role", role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        //[HttpGet("check-claims")]
        //public IActionResult CheckClaims()
        //{
        //    var userClaims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
        //    return Ok(new
        //    {
        //        Message = "Server đang nhìn thấy các thông tin này từ Token của bạn:",
        //        IsAuthenticated = User.Identity.IsAuthenticated,
        //        UserName = User.Identity.Name,
        //        Claims = userClaims
        //    });
        //}
    }
}