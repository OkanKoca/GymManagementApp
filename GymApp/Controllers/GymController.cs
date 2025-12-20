using GymApp.Data;
using GymApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Controllers
{
    public class GymController : Controller
    {
        private readonly GymDbContext _db;
        public GymController(GymDbContext db) => _db = db;

        public IActionResult Index()
        {
            var gyms = _db.Gyms
                .Include(g => g.Trainers)
                .Include(g => g.GymServices)
                .ToList();
            return View(gyms);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create() => View();

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Gym gym)
        {
            // Navigation property'leri ModelState'den çıkar
            ModelState.Remove("Trainers");
            ModelState.Remove("GymServices");

            if (!ModelState.IsValid) return View(gym);
            _db.Gyms.Add(gym);
            _db.SaveChanges();
            TempData["Success"] = "Spor salonu başarıyla eklendi.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Edit(int id)
        {
            var gym = _db.Gyms.Find(id);
            if (gym == null) return NotFound();
            return View(gym);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Gym gym)
        {
            // Navigation property'leri ModelState'den çıkar
            ModelState.Remove("Trainers");
            ModelState.Remove("GymServices");

            if (!ModelState.IsValid) return View(gym);
            _db.Gyms.Update(gym);
            _db.SaveChanges();
            TempData["Success"] = "Spor salonu başarıyla güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var gym = _db.Gyms
                .Include(g => g.Trainers)
                .ThenInclude(t => t.Appointments)
                .Include(g => g.GymServices)
                .ThenInclude(gs => gs.Service)
                .FirstOrDefault(g => g.Id == id);

            if (gym == null) return NotFound();

            // İlişkili verileri ViewBag'e aktar
            ViewBag.TrainerCount = gym.Trainers?.Count ?? 0;
            ViewBag.GymServiceCount = gym.GymServices?.Count ?? 0;

            // Eğitmenlere ait toplam randevu sayısı
            var totalAppointments = gym.Trainers?.Sum(t => t.Appointments?.Count ?? 0) ?? 0;
            ViewBag.TotalAppointmentCount = totalAppointments;
            ViewBag.HasRelatedData = ViewBag.TrainerCount > 0 || ViewBag.GymServiceCount > 0;

            return View(gym);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id, bool confirmCascade = false)
        {
            var gym = _db.Gyms
                .Include(g => g.Trainers)
                .ThenInclude(t => t.Appointments)
                .Include(g => g.Trainers)
                .ThenInclude(t => t.Availabilities)
                .Include(g => g.Trainers)
                .ThenInclude(t => t.TrainerServices)
                .Include(g => g.GymServices)
                .FirstOrDefault(g => g.Id == id);

            if (gym == null) return NotFound();

            var hasRelatedData = (gym.Trainers?.Any() ?? false) || (gym.GymServices?.Any() ?? false);

            // İlişkili veri varsa ve onay verilmemişse geri dön
            if (hasRelatedData && !confirmCascade)
            {
                TempData["Error"] = "İlişkili veriler bulunduğu için silme onayı gereklidir.";
                return RedirectToAction("Delete", new { id });
            }

            // Eğitmenlerin ilişkili verilerini sil
            if (gym.Trainers?.Any() ?? false)
            {
                foreach (var trainer in gym.Trainers)
                {
                    if (trainer.Appointments?.Any() ?? false)
                    {
                        _db.Appointments.RemoveRange(trainer.Appointments);
                    }
                    if (trainer.Availabilities?.Any() ?? false)
                    {
                        _db.TrainerAvailabilities.RemoveRange(trainer.Availabilities);
                    }
                    if (trainer.TrainerServices?.Any() ?? false)
                    {
                        _db.TrainerServices.RemoveRange(trainer.TrainerServices);
                    }
                }
                _db.Trainers.RemoveRange(gym.Trainers);
            }

            // İlişkili salon-hizmet bağlantılarını sil
            if (gym.GymServices?.Any() ?? false)
            {
                _db.GymServices.RemoveRange(gym.GymServices);
            }

            _db.Gyms.Remove(gym);
            _db.SaveChanges();
            TempData["Success"] = "Spor salonu ve ilişkili tüm veriler başarıyla silindi.";
            return RedirectToAction(nameof(Index));
        }
    }
}
