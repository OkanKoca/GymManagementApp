using GymApp.Data;
using GymApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace GymApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class TrainerAvailabilityController : Controller
    {
        private readonly GymDbContext _db;

        public TrainerAvailabilityController(GymDbContext db) => _db = db;

        public IActionResult Index()
        {
            var data = _db.TrainerAvailabilities.Include(a => a.Trainer).ToList();
            return View(data);
        }

        public IActionResult Create()
        {
            ViewBag.Trainers = _db.Trainers.ToList();
            ViewBag.Days = Enum.GetValues(typeof(DayOfWeek));
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(TrainerAvailability a)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Trainers = _db.Trainers.ToList();
                ViewBag.Days = Enum.GetValues(typeof(DayOfWeek));
                return View(a);
            }

            _db.TrainerAvailabilities.Add(a);
            _db.SaveChanges();
            TempData["Success"] = "Müsaitlik başarıyla eklendi.";
            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            var a = _db.TrainerAvailabilities.Find(id);
            if (a == null) return NotFound();
            ViewBag.Trainers = _db.Trainers.ToList();
            ViewBag.Days = Enum.GetValues(typeof(DayOfWeek));
            return View(a);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(TrainerAvailability a)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Trainers = _db.Trainers.ToList();
                ViewBag.Days = Enum.GetValues(typeof(DayOfWeek));
                return View(a);
            }
            _db.TrainerAvailabilities.Update(a);
            _db.SaveChanges();
            TempData["Success"] = "Müsaitlik başarıyla güncellendi.";
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var a = _db.TrainerAvailabilities.Include(x => x.Trainer).FirstOrDefault(x => x.Id == id);
            if (a == null) return NotFound();
            return View(a);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var a = _db.TrainerAvailabilities.Find(id);
            if (a == null) return NotFound();
            _db.TrainerAvailabilities.Remove(a);
            _db.SaveChanges();
            TempData["Success"] = "Müsaitlik başarıyla silindi.";
            return RedirectToAction("Index");
        }
    }
}
