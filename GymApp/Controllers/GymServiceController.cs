using GymApp.Data;
using GymApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class GymServiceController : Controller
    {
        private readonly GymDbContext _db;

        public GymServiceController(GymDbContext db)
        {
            _db = db;
        }

        // Belirli bir salonun hizmetlerini görüntüleme
        public async Task<IActionResult> Index(int gymId)
        {
            var gym = await _db.Gyms
           .Include(g => g.GymServices)
           .ThenInclude(gs => gs.Service)
    .FirstOrDefaultAsync(g => g.Id == gymId);

            if (gym == null)
            {
                return NotFound();
            }

            ViewBag.Gym = gym;
            ViewBag.AvailableServices = await _db.Services
                       .Where(s => s.IsActive && !gym.GymServices.Any(gs => gs.ServiceId == s.Id))
              .ToListAsync();

            return View(gym.GymServices.ToList());
        }

        // Salona hizmet ekleme
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int gymId, int serviceId, decimal? customPrice)
        {
            var gym = await _db.Gyms.FindAsync(gymId);
            var service = await _db.Services.FindAsync(serviceId);

            if (gym == null || service == null)
            {
                return NotFound();
            }

            // Zaten ekli mi kontrol et
            var exists = await _db.GymServices
       .AnyAsync(gs => gs.GymId == gymId && gs.ServiceId == serviceId);

            if (exists)
            {
                TempData["Error"] = "Bu hizmet zaten salona eklenmiþ.";
                return RedirectToAction("Index", new { gymId });
            }

            var gymService = new GymService
            {
                GymId = gymId,
                ServiceId = serviceId,
                CustomPrice = customPrice,
                IsActive = true
            };

            _db.GymServices.Add(gymService);
            await _db.SaveChangesAsync();

            TempData["Success"] = $"'{service.Name}' hizmeti salona baþarýyla eklendi.";
            return RedirectToAction("Index", new { gymId });
        }

        // Salondan hizmet kaldýrma
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int id)
        {
            var gymService = await _db.GymServices
          .Include(gs => gs.Service)
       .FirstOrDefaultAsync(gs => gs.Id == id);

            if (gymService == null)
            {
                return NotFound();
            }

            var gymId = gymService.GymId;
            var serviceName = gymService.Service?.Name;

            _db.GymServices.Remove(gymService);
            await _db.SaveChangesAsync();

            TempData["Success"] = $"'{serviceName}' hizmeti salondan kaldýrýldý.";
            return RedirectToAction("Index", new { gymId });
        }

        // Hizmet aktif/pasif durumu deðiþtirme
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var gymService = await _db.GymServices.FindAsync(id);

            if (gymService == null)
            {
                return NotFound();
            }

            gymService.IsActive = !gymService.IsActive;
            await _db.SaveChangesAsync();

            TempData["Success"] = gymService.IsActive
                ? "Hizmet aktif edildi."
                   : "Hizmet pasif edildi.";

            return RedirectToAction("Index", new { gymId = gymService.GymId });
        }

        // Özel fiyat güncelleme
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePrice(int id, decimal? customPrice)
        {
            var gymService = await _db.GymServices.FindAsync(id);

            if (gymService == null)
            {
                return NotFound();
            }

            gymService.CustomPrice = customPrice;
            await _db.SaveChangesAsync();

            TempData["Success"] = "Özel fiyat güncellendi.";
            return RedirectToAction("Index", new { gymId = gymService.GymId });
        }
    }
}
