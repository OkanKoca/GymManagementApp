using GymApp.Data;
using GymApp.Models;
using GymApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Controllers
{
    [Authorize]
    public class AppointmentController : Controller
    {
        private readonly GymDbContext _context;
        private readonly UserManager<Member> _userManager;

        public AppointmentController(GymDbContext context, UserManager<Member> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Üyenin kendi randevularýný görüntüleme
        public async Task<IActionResult> Index()
        {
         var userId = _userManager.GetUserId(User);

            // Sadece aktif randevularý göster (Pending ve Approved)
            var appointments = await _context.Appointments
      .Include(a => a.Trainer)
     .Include(a => a.Service)
.Include(a => a.Member)
            .Where(a => a.MemberId == userId && 
           (a.Status == "Pending" || a.Status == "Approved"))
        .OrderByDescending(a => a.StartAt)
            .ToListAsync();

 return View(appointments);
        }

        // Üyenin geçmiþ randevularýný görüntüleme
        public async Task<IActionResult> History()
    {
            var userId = _userManager.GetUserId(User);

            // Tamamlanmýþ, iptal edilmiþ ve reddedilmiþ randevularý göster
          var pastAppointments = await _context.Appointments
.Include(a => a.Trainer)
           .Include(a => a.Service)
      .Include(a => a.Member)
       .Where(a => a.MemberId == userId && 
         (a.Status == "Completed" || a.Status == "Canceled" || a.Status == "Rejected"))
            .OrderByDescending(a => a.StartAt)
           .ToListAsync();

       return View(pastAppointments);
     }

        // Admin için tüm randevularý görüntüleme
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> All(int? trainerId, DateTime? date, string? status)
        {
            var query = _context.Appointments
            .Include(a => a.Trainer)
            .Include(a => a.Service)
            .Include(a => a.Member)
            .AsQueryable();

            // LINQ filtreleme
            if (trainerId.HasValue)
            {
                query = query.Where(a => a.TrainerId == trainerId.Value);
            }

            if (date.HasValue)
            {
                var startDate = DateTime.SpecifyKind(date.Value.Date, DateTimeKind.Utc);
                var endDate = startDate.AddDays(1);
                query = query.Where(a => a.StartAt >= startDate && a.StartAt < endDate);
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(a => a.Status == status);
            }

            var appointments = await query.OrderByDescending(a => a.StartAt).ToListAsync();

            ViewBag.Trainers = new SelectList(await _context.Trainers.ToListAsync(), "Id", "FullName", trainerId);
            ViewBag.SelectedTrainerId = trainerId;
            ViewBag.SelectedDate = date?.ToString("yyyy-MM-dd");
            ViewBag.SelectedStatus = status;

            return View(appointments);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Trainers = new SelectList(
            await _context.Trainers.Include(t => t.Gym).ToListAsync(),
                "Id",
                "FullName"
            );

            ViewBag.Services = new SelectList(
            await _context.Services.ToListAsync(),
                "Id",
                "Name"
            );

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AppointmentCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                var service = await _context.Services.FindAsync(model.ServiceId);

                if (service == null)
                {
                    ModelState.AddModelError("ServiceId", "Geçersiz hizmet seçimi.");
                    await LoadViewBags();
                    return View(model);
                }

                var startAt = model.AppointmentDate.Date.Add(model.AppointmentTime);
                var endAt = startAt.AddMinutes(service.DurationMinutes);

                // UTC'ye çevir
                startAt = DateTime.SpecifyKind(startAt, DateTimeKind.Utc);
                endAt = DateTime.SpecifyKind(endAt, DateTimeKind.Utc);

                // Çakýþma kontrolü - ayný saatte randevu var mý?
                var hasConflict = await _context.Appointments
                .AnyAsync(a => a.TrainerId == model.TrainerId &&
                    a.Status != "Canceled" &&
                    a.Status != "Rejected" &&
                    ((startAt >= a.StartAt && startAt < a.EndAt) ||
                    (endAt > a.StartAt && endAt <= a.EndAt) ||
                    (startAt <= a.StartAt && endAt >= a.EndAt))
                );

                if (hasConflict)
                {
                    ModelState.AddModelError("", "Seçilen saatte eðitmenin baþka bir randevusu bulunmaktadýr.");
                    await LoadViewBags();
                    return View(model);
                }

                // Eðitmen müsaitlik kontrolü
                var dayOfWeek = startAt.DayOfWeek;
                var timeOfDay = model.AppointmentTime;
                var endTimeOfDay = timeOfDay.Add(TimeSpan.FromMinutes(service.DurationMinutes));

                var isAvailable = await _context.TrainerAvailabilities
                         .AnyAsync(ta => ta.TrainerId == model.TrainerId &&
                    ta.Day == dayOfWeek &&
                    ta.From <= timeOfDay &&
                               ta.To >= endTimeOfDay);

                if (!isAvailable)
                {
                    ModelState.AddModelError("", "Eðitmen seçilen gün ve saatte müsait deðildir.");
                    await LoadViewBags();
                    return View(model);
                }

                var appointment = new Appointment
                {
                    MemberId = userId!,
                    TrainerId = model.TrainerId,
                    ServiceId = model.ServiceId,
                    StartAt = startAt,
                    EndAt = endAt,
                    Status = "Pending",
                    Notes = model.Notes,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Randevu talebiniz oluþturuldu. Onay bekleniyor.";
                return RedirectToAction(nameof(Index));
            }

            await LoadViewBags();
            return View(model);
        }

        // Randevu Onaylama (Admin)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }

            appointment.Status = "Approved";
            await _context.SaveChangesAsync();

            TempData["Success"] = "Randevu onaylandý.";
            return RedirectToAction(nameof(All));
        }

        // Randevu Reddetme (Admin)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }

            appointment.Status = "Rejected";
            await _context.SaveChangesAsync();

            TempData["Success"] = "Randevu reddedildi.";
            return RedirectToAction(nameof(All));
        }

        // Randevu Ýptal (Üye kendi randevusunu iptal edebilir)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = _userManager.GetUserId(User);
            var appointment = await _context.Appointments.FindAsync(id);

            if (appointment == null)
            {
                return NotFound();
            }

            // Admin veya randevu sahibi iptal edebilir
            if (appointment.MemberId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            appointment.Status = "Canceled";
            await _context.SaveChangesAsync();

            TempData["Success"] = "Randevu iptal edildi.";
            return RedirectToAction(nameof(Index));
        }

        // Randevu Tamamlama (Admin)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }

            appointment.Status = "Completed";
            await _context.SaveChangesAsync();

            TempData["Success"] = "Randevu tamamlandý olarak iþaretlendi.";
            return RedirectToAction(nameof(All));
        }

        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User);
            var appointment = await _context.Appointments
            .Include(a => a.Trainer)
            .ThenInclude(t => t.Gym)
            .Include(a => a.Service)
            .Include(a => a.Member)
            .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
            {
                return NotFound();
            }

            // Admin veya randevu sahibi görebilir
            if (appointment.MemberId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            return View(appointment);
        }

        // Eðitmenin müsait saatlerini getir (AJAX için)
        [HttpGet]
        public async Task<IActionResult> GetAvailableSlots(int trainerId, DateTime date)
        {
            var dayOfWeek = date.DayOfWeek;

            var availability = await _context.TrainerAvailabilities
                 .Where(ta => ta.TrainerId == trainerId && ta.Day == dayOfWeek)
           .ToListAsync();

            var utcDate = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
            var existingAppointments = await _context.Appointments
            .Where(a => a.TrainerId == trainerId &&
            a.StartAt.Date == utcDate.Date &&
            a.Status != "Canceled" &&
            a.Status != "Rejected")
            .Select(a => new { a.StartAt, a.EndAt })
            .ToListAsync();

            return Json(new { availability, existingAppointments });
        }

        private async Task LoadViewBags()
        {
            ViewBag.Trainers = new SelectList(
            await _context.Trainers.Include(t => t.Gym).ToListAsync(),
               "Id",
                "FullName"
            );

            ViewBag.Services = new SelectList(
            await _context.Services.ToListAsync(),
                "Id",
                "Name"
            );
        }
    }
}
