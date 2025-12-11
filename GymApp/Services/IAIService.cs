using GymApp.ViewModels;

namespace GymApp.Services
{
    public interface IAIService
    {
      Task<AIRecommendationViewModel> GetRecommendationsAsync(AIRecommendationViewModel model);
        Task<string?> SavePhotoAsync(IFormFile photo);
    }
}
