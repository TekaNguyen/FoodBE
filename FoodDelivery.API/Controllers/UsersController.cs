using FoodDelivery.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodDelivery.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // 👇 Primary Constructor: Khai báo dependency ngay tại dòng class
    public class UsersController(UserManager<AppUser> userManager) : ControllerBase
    {
        // 1. GET ALL
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            // 👇 Lưu ý: Bây giờ bạn dùng trực tiếp biến 'userManager' (không có dấu gạch dưới _)
            var users = await userManager.Users.ToListAsync();
            var userDtos = new List<object>();

            foreach (var user in users)
            {
                var roles = await userManager.GetRolesAsync(user);

                bool isDeleted = user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow;

                userDtos.Add(new
                {
                    id = user.Id,
                    email = user.Email,
                    fullName = user.FullName,
                    phoneNumber = user.PhoneNumber,
                    roles,
                    isDeleted
                });
            }
            return Ok(userDtos);
        }

        // 2. DELETE (Soft Delete)
        [HttpDelete("{id}")]
        public async Task<IActionResult> SoftDeleteUser(string id)
        {
            // 👇 Dùng 'userManager' trực tiếp
            var user = await userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy người dùng" });
            }

            var result = await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));

            if (result.Succeeded)
            {
                return Ok(new { message = $"Đã khóa tài khoản {user.Email} thành công" });
            }

            return BadRequest(new { message = "Lỗi khi khóa tài khoản" });
        }

        // 3. Assign Role
        [HttpPost("assign-role")]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleRequest request)
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user == null) return NotFound("User not found");

            var currentRoles = await userManager.GetRolesAsync(user);
            await userManager.RemoveFromRolesAsync(user, currentRoles);

            var result = await userManager.AddToRoleAsync(user, request.Role);

            if (result.Succeeded) return Ok(new { message = "Role updated" });
            return BadRequest("Failed to update role");
        }
    }
    public class AssignRoleRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}