using FoodDelivery.API.Services;
using FoodDelivery.Domain.Entities;
using FoodDelivery.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class NewsletterController(AppDbContext context, IEmailService emailService) : ControllerBase
{
    private readonly AppDbContext _context = context;
    private readonly IEmailService _emailService = emailService;

    [HttpPost("subscribe")]
    public async Task<IActionResult> Subscribe([FromBody] string email)
    {
        if (await _context.NewsletterSubscriptions.AnyAsync(s => s.Email == email))
            return BadRequest("Email này đã đăng ký rồi.");

        var sub = new NewsletterSubscription { Email = email };
        _context.NewsletterSubscriptions.Add(sub);
        await _context.SaveChangesAsync();

        // Gửi mail cảm ơn (Chào mừng khách hàng)
        await _emailService.SendEmailAsync(email, "Chào mừng bạn!", "Cảm ơn bạn đã đăng ký nhận tin khuyến mãi từ Cơm Tấm Đêm.");

        return Ok("Đăng ký thành công!");
    }
}