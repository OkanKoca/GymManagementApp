using GymApp.Data;
using GymApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Controllers
{
    public class ServiceController : Controller
    {
        private readonly GymDbContext _db;
        public ServiceController(GymDbContext db) => _db = db;

        public IActionResult Index() => View(_db.Services.ToList());

        [Authorize(Roles = "Admin")]
        public IActionResult Create() => View();

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Service s)
        {
            // Navigation property'leri ModelState'den çıkar
            ModelState.Remove("Appointments");
            ModelState.Remove("TrainerServices");
            ModelState.Remove("GymServices");

            if (!ModelState.IsValid) return View(s);
            _db.Services.Add(s);
            _db.SaveChanges();
            TempData["Success"] = "Hizmet başarıyla eklendi.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Edit(int id)
        {
            var s = _db.Services.Find(id);
            if (s == null) return NotFound();
            return View(s);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Service s)
        {
            // Navigation property'leri ModelState'den çıkar
            ModelState.Remove("Appointments");
            ModelState.Remove("TrainerServices");
            ModelState.Remove("GymServices");

            if (!ModelState.IsValid) return View(s);
            _db.Services.Update(s);
            _db.SaveChanges();
            TempData["Success"] = "Hizmet başarıyla güncellendi.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var s = _db.Services
            .Include(x => x.Appointments)
            .ThenInclude(a => a.Member)
            .Include(x => x.TrainerServices)
            .ThenInclude(ts => ts.Trainer)
            .Include(x => x.GymServices)
            .ThenInclude(gs => gs.Gym)
            .FirstOrDefault(x => x.Id == id);

            if (s == null) return NotFound();

            // İlişkili verileri ViewBag'e aktar
            ViewBag.AppointmentCount = s.Appointments?.Count ?? 0;
            ViewBag.TrainerServiceCount = s.TrainerServices?.Count ?? 0;
            ViewBag.GymServiceCount = s.GymServices?.Count ?? 0;
            ViewBag.HasRelatedData = ViewBag.AppointmentCount > 0 || ViewBag.TrainerServiceCount > 0 || ViewBag.GymServiceCount > 0;

            return View(s);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id, bool confirmCascade = false)
        {
            var s = _db.Services
               .Include(x => x.Appointments)
       .Include(x => x.TrainerServices)
        .Include(x => x.GymServices)
              .FirstOrDefault(x => x.Id == id);

            if (s == null) return NotFound();

            var hasRelatedData = (s.Appointments?.Any() ?? false) ||
                       (s.TrainerServices?.Any() ?? false) ||
                   (s.GymServices?.Any() ?? false);

            // İlişkili veri varsa ve onay verilmemişse geri dön
            if (hasRelatedData && !confirmCascade)
            {
                TempData["Error"] = "İlişkili veriler bulunduğu için silme onayı gereklidir.";
                return RedirectToAction("Delete", new { id });
            }

            // İlişkili randevuları sil
            if (s.Appointments?.Any() ?? false)
            {
                _db.Appointments.RemoveRange(s.Appointments);
            }

            // İlişkili eğitmen-hizmet bağlantılarını sil
            if (s.TrainerServices?.Any() ?? false)
            {
                _db.TrainerServices.RemoveRange(s.TrainerServices);
            }

            // İlişkili salon-hizmet bağlantılarını sil
            if (s.GymServices?.Any() ?? false)
            {
                _db.GymServices.RemoveRange(s.GymServices);
            }

            _db.Services.Remove(s);
            _db.SaveChanges();
            TempData["Success"] = "Hizmet ve ilişkili tüm veriler başarıyla silindi.";
            return RedirectToAction("Index");
        }
    }
}
