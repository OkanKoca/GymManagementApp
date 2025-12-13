using GymApp.Models;
using GymApp.Services;
using GymApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GymApp.Controllers
{
    [Authorize]
    public class AIController : Controller
    {
        private readonly IAIService _aiService;
        private readonly UserManager<Member> _userManager;

        public AIController(IAIService aiService, UserManager<Member> userManager)
        {
            _aiService = aiService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Recommendations()
        {
            var user = await _userManager.GetUserAsync(User);
            var model = new AIRecommendationViewModel();

            // Kullanýcý bilgilerini önceden doldur
            if (user != null)
            {
                model.Height = user.Height;
                model.Weight = user.Weight;
                model.Age = user.Age;
                model.Gender = user.Gender;
                model.BodyType = user.BodyType;
                model.Goal = user.FitnessGoal;
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Recommendations(AIRecommendationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _aiService.GetRecommendationsAsync(model);
            return View(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveToProfile(AIRecommendationViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            // Kullanýcý profilini güncelle
            if (model.Height.HasValue) user.Height = model.Height;
            if (model.Weight.HasValue) user.Weight = model.Weight;
            if (model.Age.HasValue) user.Age = model.Age;
            if (!string.IsNullOrEmpty(model.Gender)) user.Gender = model.Gender;
            if (!string.IsNullOrEmpty(model.BodyType)) user.BodyType = model.BodyType;
            if (!string.IsNullOrEmpty(model.Goal)) user.FitnessGoal = model.Goal;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["Success"] = "Bilgileriniz profilinize kaydedildi.";
            }

            return RedirectToAction(nameof(Recommendations));
        }
    }
}
