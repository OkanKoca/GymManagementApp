using GymApp.Data;
using GymApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Controllers
{
    public class TrainerController : Controller
    {
        private readonly GymDbContext _db;

        public TrainerController(GymDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            var trainers = _db.Trainers
                .Include(t => t.Gym)
                .Include(t => t.Availabilities)
                .ToList();
            return View(trainers);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewBag.Gyms = _db.Gyms.ToList();
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Trainer t)
        {
            // Navigation property'leri ModelState'den çıkar
            ModelState.Remove("Gym");

            if (!ModelState.IsValid)
            {
                ViewBag.Gyms = _db.Gyms.ToList();
                return View(t);
            }

            _db.Trainers.Add(t);
            _db.SaveChanges();
            TempData["Success"] = "Eğitmen başarıyla eklendi.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Edit(int id)
        {
            var t = _db.Trainers.Find(id);
            if (t == null) return NotFound();

            ViewBag.Gyms = _db.Gyms.ToList();
            return View(t);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Trainer t)
        {
            // Navigation property'leri ModelState'den çıkar
            ModelState.Remove("Gym");

            if (!ModelState.IsValid)
            {
                ViewBag.Gyms = _db.Gyms.ToList();
                return View(t);
            }

            _db.Trainers.Update(t);
            _db.SaveChanges();
            TempData["Success"] = "Eğitmen başarıyla güncellendi.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var t = _db.Trainers.Include(x => x.Gym).FirstOrDefault(x => x.Id == id);
            if (t == null) return NotFound();
            return View(t);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var t = _db.Trainers.Find(id);
            if (t == null) return NotFound();

            // İlişkili verileri kontrol et
            var hasAppointments = _db.Appointments.Any(a => a.TrainerId == id);
            if (hasAppointments)
            {
                TempData["Error"] = "Bu eğitmenin randevuları bulunduğu için silinemez.";
                return RedirectToAction("Index");
            }

            _db.Trainers.Remove(t);
            _db.SaveChanges();
            TempData["Success"] = "Eğitmen başarıyla silindi.";
            return RedirectToAction("Index");
        }
    }
}
