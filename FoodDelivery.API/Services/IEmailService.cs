namespace FoodDelivery.API.Services;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string body);

    // Hàm gửi theo Template (Nâng cao)
    Task SendTemplateEmailAsync(string toEmail, string templateKey, Dictionary<string, string> placeholders);
}