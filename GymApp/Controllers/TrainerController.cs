using GymApp.Data;
using GymApp.Models;
using GymApp.ViewModels;
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
                .Include(t => t.TrainerServices)
                    .ThenInclude(ts => ts.Service)
                .ToList();
            return View(trainers);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            var viewModel = new TrainerFormViewModel
            {
                Gyms = _db.Gyms.ToList(),
                AvailableServices = _db.Services.Where(s => s.IsActive).ToList()
            };
            return View(viewModel);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(TrainerFormViewModel viewModel)
        {
            // Navigation property'leri ModelState'den çıkar
            ModelState.Remove("Gyms");
            ModelState.Remove("AvailableServices");

            if (!ModelState.IsValid)
            {
                viewModel.Gyms = _db.Gyms.ToList();
                viewModel.AvailableServices = _db.Services.Where(s => s.IsActive).ToList();
                return View(viewModel);
            }

            var trainer = viewModel.ToTrainer();
            _db.Trainers.Add(trainer);
            _db.SaveChanges();

            // Seçilen hizmetleri ekle
            if (viewModel.SelectedServiceIds != null && viewModel.SelectedServiceIds.Any())
            {
                foreach (var serviceId in viewModel.SelectedServiceIds)
                {
                    _db.TrainerServices.Add(new TrainerService
                    {
                        TrainerId = trainer.Id,
                        ServiceId = serviceId
                    });
                }
                _db.SaveChanges();
            }

            TempData["Success"] = "Eğitmen başarıyla eklendi.";
            return RedirectToAction("Index");
        }


        [Authorize(Roles = "Admin")]
        public IActionResult Edit(int id)
        {
            var trainer = _db.Trainers
                .Include(t => t.TrainerServices)
                .FirstOrDefault(t => t.Id == id);

            if (trainer == null) return NotFound();

            var viewModel = TrainerFormViewModel.FromTrainer(trainer);
            viewModel.Gyms = _db.Gyms.ToList();
            viewModel.AvailableServices = _db.Services.Where(s => s.IsActive).ToList();

            return View(viewModel);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(TrainerFormViewModel viewModel)
        {
            // Navigation property'leri ModelState'den çıkar
            ModelState.Remove("Gyms");
            ModelState.Remove("AvailableServices");

            if (!ModelState.IsValid)
            {
                viewModel.Gyms = _db.Gyms.ToList();
                viewModel.AvailableServices = _db.Services.Where(s => s.IsActive).ToList();
                return View(viewModel);
            }

            var trainer = viewModel.ToTrainer();
            _db.Trainers.Update(trainer);

            // Mevcut hizmet ilişkilerini sil
            var existingServices = _db.TrainerServices.Where(ts => ts.TrainerId == trainer.Id);
            _db.TrainerServices.RemoveRange(existingServices);

            // Yeni hizmet ilişkilerini ekle
            if (viewModel.SelectedServiceIds != null && viewModel.SelectedServiceIds.Any())
            {
                foreach (var serviceId in viewModel.SelectedServiceIds)
                {
                    _db.TrainerServices.Add(new TrainerService
                    {
                        TrainerId = trainer.Id,
                        ServiceId = serviceId
                    });
                }
            }

            _db.SaveChanges();
            TempData["Success"] = "Eğitmen başarıyla güncellendi.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var t = _db.Trainers
                .Include(x => x.Gym)
                .Include(x => x.TrainerServices)
                    .ThenInclude(ts => ts.Service)
                .FirstOrDefault(x => x.Id == id);
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
