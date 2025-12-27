namespace FoodDelivery.API.Services;
    public interface IFileUploadService
    {
        Task<string> UploadFileAsync(IFormFile file, string subFolder);
        void DeleteFile(string relativePath);
    }
