using GymApp.Data;
using GymApp.Models;
using GymApp.ViewModels;
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
      var data = _db.TrainerAvailabilities
          .Include(a => a.Trainer)
  .OrderBy(a => a.Trainer.FullName)
        .ThenBy(a => a.Day)
    .ThenBy(a => a.From)
.ToList();
   return View(data);
   }

  public IActionResult Create()
        {
  ViewBag.Trainers = _db.Trainers.OrderBy(t => t.FullName).ToList();
 return View();
      }

        [HttpPost]
 [ValidateAntiForgeryToken]
        public IActionResult Create(TrainerAvailability a)
   {
  // Navigation property'yi ModelState'den çıkar
   ModelState.Remove("Trainer");

 if (!ModelState.IsValid)
   {
ViewBag.Trainers = _db.Trainers.OrderBy(t => t.FullName).ToList();
    return View(a);
     }

      // Bitiş saati başlangıçtan sonra olmalı
            if (a.To <= a.From)
       {
            ModelState.AddModelError("To", "Bitiş saati başlangıç saatinden sonra olmalıdır.");
    ViewBag.Trainers = _db.Trainers.OrderBy(t => t.FullName).ToList();
    return View(a);
   }

   // Aynı eğitmen ve gün için çakışma kontrolü
            var hasConflict = _db.TrainerAvailabilities
   .Any(ta => ta.TrainerId == a.TrainerId && 
 ta.Day == a.Day &&
       ((a.From >= ta.From && a.From < ta.To) ||
  (a.To > ta.From && a.To <= ta.To) ||
          (a.From <= ta.From && a.To >= ta.To)));

       if (hasConflict)
   {
         ModelState.AddModelError("", "Bu eğitmenin seçilen gün ve saatlerde zaten bir müsaitlik kaydı var.");
   ViewBag.Trainers = _db.Trainers.OrderBy(t => t.FullName).ToList();
        return View(a);
    }

   _db.TrainerAvailabilities.Add(a);
            _db.SaveChanges();
      TempData["Success"] = "Müsaitlik başarıyla eklendi.";
     return RedirectToAction("Index");
    }

 // Toplu müsaitlik ekleme sayfası
        public IActionResult BulkCreate()
    {
    ViewBag.Trainers = _db.Trainers.OrderBy(t => t.FullName).ToList();
       return View(new BulkAvailabilityViewModel { From = new TimeSpan(9, 0, 0), To = new TimeSpan(18, 0, 0) });
        }

      // Toplu müsaitlik ekleme işlemi
        [HttpPost]
  [ValidateAntiForgeryToken]
        public IActionResult BulkCreate(BulkAvailabilityViewModel model)
        {
            if (model.SelectedDays == null || !model.SelectedDays.Any())
     {
         ModelState.AddModelError("SelectedDays", "En az bir gün seçmelisiniz.");
     }

    if (model.To <= model.From)
            {
        ModelState.AddModelError("To", "Bitiş saati başlangıç saatinden sonra olmalıdır.");
          }

         if (!ModelState.IsValid)
            {
   ViewBag.Trainers = _db.Trainers.OrderBy(t => t.FullName).ToList();
            return View(model);
            }

            var addedCount = 0;
   var skippedCount = 0;
         var skippedDays = new List<string>();

       foreach (var day in model.SelectedDays)
    {
         // Çakışma kontrolü
        var hasConflict = _db.TrainerAvailabilities
         .Any(ta => ta.TrainerId == model.TrainerId && 
    ta.Day == day &&
     ((model.From >= ta.From && model.From < ta.To) ||
   (model.To > ta.From && model.To <= ta.To) ||
      (model.From <= ta.From && model.To >= ta.To)));

  if (hasConflict)
         {
   skippedCount++;
      skippedDays.Add(GetDayName(day));
            continue;
                }

           var availability = new TrainerAvailability
         {
TrainerId = model.TrainerId,
  Day = day,
          From = model.From,
     To = model.To
      };

        _db.TrainerAvailabilities.Add(availability);
       addedCount++;
      }

   _db.SaveChanges();

            if (addedCount > 0 && skippedCount == 0)
  {
          TempData["Success"] = $"{addedCount} gün için müsaitlik başarıyla eklendi.";
   }
       else if (addedCount > 0 && skippedCount > 0)
  {
    TempData["Success"] = $"{addedCount} gün için müsaitlik eklendi. {skippedCount} gün zaten kayıtlı olduğu için atlandı ({string.Join(", ", skippedDays)}).";
      }
   else
     {
  TempData["Error"] = "Seçilen tüm günlerde zaten müsaitlik kaydı mevcut.";
            }

  return RedirectToAction("Index");
   }

        private string GetDayName(DayOfWeek day)
        {
            return day switch
      {
  DayOfWeek.Monday => "Pazartesi",
       DayOfWeek.Tuesday => "Salı",
          DayOfWeek.Wednesday => "Çarşamba",
DayOfWeek.Thursday => "Perşembe",
      DayOfWeek.Friday => "Cuma",
          DayOfWeek.Saturday => "Cumartesi",
 DayOfWeek.Sunday => "Pazar",
     _ => day.ToString()
       };
        }

 public IActionResult Edit(int id)
   {
 var a = _db.TrainerAvailabilities.Find(id);
        if (a == null) return NotFound();
  ViewBag.Trainers = _db.Trainers.OrderBy(t => t.FullName).ToList();
 return View(a);
      }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(TrainerAvailability a)
        {
  // Navigation property'yi ModelState'den çıkar
 ModelState.Remove("Trainer");

  if (!ModelState.IsValid)
          {
        ViewBag.Trainers = _db.Trainers.OrderBy(t => t.FullName).ToList();
   return View(a);
   }

   // Bitiş saati başlangıçtan sonra olmalı
      if (a.To <= a.From)
     {
          ModelState.AddModelError("To", "Bitiş saati başlangıç saatinden sonra olmalıdır.");
  ViewBag.Trainers = _db.Trainers.OrderBy(t => t.FullName).ToList();
          return View(a);
            }

     // Aynı eğitmen ve gün için çakışma kontrolü (kendisi hariç)
        var hasConflict = _db.TrainerAvailabilities
      .Any(ta => ta.Id != a.Id &&
        ta.TrainerId == a.TrainerId && 
    ta.Day == a.Day &&
  ((a.From >= ta.From && a.From < ta.To) ||
   (a.To > ta.From && a.To <= ta.To) ||
  (a.From <= ta.From && a.To >= ta.To)));

  if (hasConflict)
    {
         ModelState.AddModelError("", "Bu eğitmenin seçilen gün ve saatlerde zaten bir müsaitlik kaydı var.");
    ViewBag.Trainers = _db.Trainers.OrderBy(t => t.FullName).ToList();
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
