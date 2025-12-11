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
            var gyms = _db.Gyms.Include(g => g.Trainers).ToList();
            return View(gyms);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create() => View();

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Gym gym)
        {
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
            if (!ModelState.IsValid) return View(gym);
            _db.Gyms.Update(gym);
            _db.SaveChanges();
            TempData["Success"] = "Spor salonu başarıyla güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var gym = _db.Gyms.Find(id);
            if (gym == null) return NotFound();
            return View(gym);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var gym = _db.Gyms.Find(id);
            if (gym == null) return NotFound();
            _db.Gyms.Remove(gym);
            _db.SaveChanges();
            TempData["Success"] = "Spor salonu başarıyla silindi.";
            return RedirectToAction(nameof(Index));
        }
    }
}
