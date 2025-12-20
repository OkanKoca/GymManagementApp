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
        private readonly IGeminiImageService _imageService;
        private readonly UserManager<Member> _userManager;

        public AIController(IAIService aiService, UserManager<Member> userManager, IGeminiImageService imageService)
        {
            _aiService = aiService;
            _userManager = userManager;
            _imageService = imageService;
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
                return View(model);

            // 1) Text önerileri
            var result = await _aiService.GetRecommendationsAsync(model);

            // 2) Foto geldiyse görsel simülasyon
            if (result.UserPhoto != null && result.UserPhoto.Length > 0)
            {
                // Basic guardrails (projede abuse olmasýn)
                const long maxBytes = 3 * 1024 * 1024; // 3MB
                if (result.UserPhoto.Length > maxBytes)
                {
                    result.FutureImageError = "Fotoðraf çok büyük. Lütfen 3MB altý bir görsel yükle.";
                    return View(result);
                }

                var contentType = result.UserPhoto.ContentType?.ToLowerInvariant() ?? "";
                var allowed = contentType is "image/jpeg" or "image/png" or "image/webp";
                if (!allowed)
                {
                    result.FutureImageError = "Sadece JPG / PNG / WEBP kabul ediliyor.";
                    return View(result);
                }

                byte[] bytes;
                using (var ms = new MemoryStream())
                {
                    await result.UserPhoto.CopyToAsync(ms);
                    bytes = ms.ToArray();
                }

                // prompt: “geleceði kesin gösterme” yerine “simülasyon / konsept” dili
                var imagePrompt = BuildFutureImagePrompt(result);

                var (dataUrl, error) = await _imageService.GenerateFutureImageAsync(bytes, contentType, imagePrompt);
                result.FutureImageDataUrl = dataUrl;
                result.FutureImageError = error;
            }

            return View(result);
        }

        // controller içine helper:
        private static string BuildFutureImagePrompt(AIRecommendationViewModel m)
        {
            var goal = (m.Goal ?? "").ToLowerInvariant();

            string bodyChange;
            string timeframe;

            if (goal.Contains("kilo ver"))
            {
                bodyChange = "noticeably leaner physique: smaller waist, less belly fat, slightly more defined chest/arms, athletic but realistic";
                timeframe = "8-12 months";
            }
            else if (goal.Contains("kas") || goal.Contains("güç"))
            {
                bodyChange = "more muscular and athletic: broader shoulders, fuller chest, more defined biceps/triceps, slightly leaner waist, realistic muscle gain";
                timeframe = "5-6 months";
            }
            else if (goal.Contains("kilo al"))
            {
                bodyChange = "slightly bigger and stronger build: fuller chest and arms, broader shoulders, healthy weight gain (not fat-focused), still realistic";
                timeframe = "6-8 months";
            }
            else
            {
                bodyChange = "body recomposition: slightly lower body fat and slightly higher muscle definition, athletic and healthy look";
                timeframe = "8-9 months";
            }

            return $@"
                EDIT the uploaded photo realistically.

                Keep the SAME PERSON identity (face, eyes, beard/hair, age) and keep the photo style realistic.
                Keep the same pose and framing as close as possible, but apply a CLEAR and NOTICEABLE fitness transformation.

                Transformation goal: {bodyChange}
                Timeframe: after consistently following the plan for about {timeframe}.

                Make the change visible in the BODY only:
                - reduce body fat and waist size noticeably
                - increase muscle definition in shoulders, chest, arms
                - improve overall athletic V-shape
                Keep skin tone natural, keep lighting natural.

                IMPORTANT:
                - The result must look meaningfully different from the original (not a near-identical copy).
                - Do NOT change the person's face identity or make them look like someone else.
                - Do NOT add any text or logos.
                Photorealistic output.";
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
