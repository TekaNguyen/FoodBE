using FoodDelivery.Infrastructure.Persistence;
using FoodDelivery.API.DTOs;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using MimeKit;

namespace FoodDelivery.API.Services;

public class EmailService(AppDbContext context, ILogger<EmailService> logger) : IEmailService
{
    private readonly AppDbContext _context = context;
    private readonly ILogger<EmailService> _logger = logger;

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        // 1. Lấy cấu hình từ DB (Dynamic Setting)
        var settings = await _context.EmailSettings.FirstOrDefaultAsync();
        if (settings == null)
        {
            _logger.LogError("Email settings not configured in Database.");
            return;
        }

        try
        {
            // 2. Tạo nội dung email
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(settings.DisplayName, settings.Email));
            email.To.Add(new MailboxAddress("", toEmail));
            email.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = body };
            email.Body = builder.ToMessageBody();

            // 3. Kết nối SMTP và gửi
            using var smtp = new SmtpClient();

            // --- THÊM DÒNG NÀY Ở ĐÂY ---
            smtp.CheckCertificateRevocation = false;
            // ---------------------------

            await smtp.ConnectAsync(settings.Host, settings.Port, SecureSocketOptions.StartTls);

            await smtp.AuthenticateAsync(settings.Email, settings.Password);

            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);

            _logger.LogInformation("Email sent to {ToEmail}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {ToEmail}", toEmail);
            throw;
        }
    }

    public async Task SendTemplateEmailAsync(string toEmail, string templateKey, Dictionary<string, string> placeholders)
    {
        // 1. Tìm Template trong DB
        var template = await _context.EmailTemplates.FirstOrDefaultAsync(t => t.Key == templateKey);
        if (template == null)
        {
            // Dùng tham số {TemplateKey} trong chuỗi template
            _logger.LogError("Template {TemplateKey} not found.", templateKey);
            // Fallback: Gửi mail trắng hoặc báo lỗi
            return;
        }

        // 2. Thay thế biến (Token replacement)
        // Ví dụ: "Xin chào {{Name}}" -> "Xin chào Tùng"
        string subject = template.Subject;
        string body = template.Body;

        foreach (var item in placeholders)
        {
            body = body.Replace($"{{{{{item.Key}}}}}", item.Value); // Replace {{Key}}
        }

        // 3. Gọi hàm gửi cơ bản
        await SendEmailAsync(toEmail, subject, body);
    }
}