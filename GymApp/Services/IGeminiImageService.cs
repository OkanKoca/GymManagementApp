namespace GymApp.Services
{
    public interface IGeminiImageService
    {
        Task<(string? dataUrl, string? error)> GenerateFutureImageAsync(
            byte[] userPhoto,
            string mimeType,
            string prompt);
    }
}
