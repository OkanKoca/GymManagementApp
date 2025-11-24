using GymApp.Data;
using GymApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace GymApp.Controllers
{
    //[Authorize(Roles = "Admin")]
    public class GymController : Controller
    {
        private readonly GymDbContext _db;
        public GymController(GymDbContext db) => _db = db;

        public IActionResult Index() => View(_db.Gyms.ToList());
        public IActionResult Create() => View();

        [HttpPost]
        public IActionResult Create(Gym gym)
        {
            if (!ModelState.IsValid) return View(gym);
            _db.Gyms.Add(gym);
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            var gym = _db.Gyms.Find(id);
            if (gym == null) return NotFound();
            return View(gym);
        }

        [HttpPost]
        public IActionResult Edit(Gym gym)
        {
            if (!ModelState.IsValid) return View(gym);
            _db.Gyms.Update(gym);
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(int id)
        {
            var gym = _db.Gyms.Find(id);
            if (gym == null) return NotFound();
            _db.Gyms.Remove(gym);
            _db.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }

}
