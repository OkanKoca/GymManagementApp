using GymApp.Data;
using GymApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
            if (!ModelState.IsValid) return View(s);
            _db.Services.Update(s);
            _db.SaveChanges();
            TempData["Success"] = "Hizmet başarıyla güncellendi.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var s = _db.Services.Find(id);
            if (s == null) return NotFound();
            return View(s);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var s = _db.Services.Find(id);
            if (s == null) return NotFound();
            _db.Services.Remove(s);
            _db.SaveChanges();
            TempData["Success"] = "Hizmet başarıyla silindi.";
            return RedirectToAction("Index");
        }
    }
}
