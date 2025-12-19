using GymApp.Data;
using GymApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentsApiController : ControllerBase
    {
        private readonly GymDbContext _context;

        public AppointmentsApiController(GymDbContext context)
        {
            _context = context;
        }

        // GET: api/AppointmentsApi - Tüm randevularý listele (filtreleme ile)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetAppointments([FromQuery] int? trainerId = null, [FromQuery] DateTime? date = null, [FromQuery] string? status = null)
        {
            var query = _context.Appointments
            .Include(a => a.Member)
            .Include(a => a.Trainer)
            .Include(a => a.Service)
            .AsQueryable();

            // LINQ ile filtreleme
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

            var appointments = await query
            .OrderByDescending(a => a.StartAt)
            .Select(a => new
            {
                a.Id,
                MemberName = a.Member.FullName,
                MemberEmail = a.Member.Email,
                TrainerName = a.Trainer.FullName,
                ServiceName = a.Service.Name,
                ServicePrice = a.Service.Price,
                ServiceDuration = a.Service.DurationMinutes,
                StartAt = a.StartAt.ToString("yyyy-MM-dd HH:mm"),
                EndAt = a.EndAt.ToString("yyyy-MM-dd HH:mm"),
                a.Status,
                a.Notes,
                CreatedAt = a.CreatedAt.ToString("yyyy-MM-dd HH:mm")
            })
             .ToListAsync();

            return Ok(appointments);
        }

        // GET: api/AppointmentsApi/5 - Belirli bir randevuyu getir
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetAppointment(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Member)
                .Include(a => a.Trainer)
             .ThenInclude(t => t.Gym)
                       .Include(a => a.Service)
           .Where(a => a.Id == id)
            .Select(a => new
            {
                a.Id,
                Member = new
                {
                    a.Member.Id,
                    a.Member.FullName,
                    a.Member.Email
                },
                Trainer = new
                {
                    a.Trainer.Id,
                    a.Trainer.FullName,
                    a.Trainer.Expertise,
                    GymName = a.Trainer.Gym.Name
                },
                Service = new
                {
                    a.Service.Id,
                    a.Service.Name,
                    a.Service.Price,
                    a.Service.DurationMinutes
                },
                StartAt = a.StartAt.ToString("yyyy-MM-dd HH:mm"),
                EndAt = a.EndAt.ToString("yyyy-MM-dd HH:mm"),
                a.Status,
                a.Notes,
                CreatedAt = a.CreatedAt.ToString("yyyy-MM-dd HH:mm")
            })
                .FirstOrDefaultAsync();

            if (appointment == null)
            {
                return NotFound(new { message = "Randevu bulunamadý." });
            }

            return Ok(appointment);
        }

        // GET: api/AppointmentsApi/member/{memberId} - Üyenin randevularýný getir
        [HttpGet("member/{memberId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetMemberAppointments(string memberId)
        {
            var appointments = await _context.Appointments
            .Include(a => a.Trainer)
            .Include(a => a.Service)
            .Where(a => a.MemberId == memberId)
            .OrderByDescending(a => a.StartAt)
            .Select(a => new
            {
                a.Id,
                TrainerName = a.Trainer.FullName,
                ServiceName = a.Service.Name,
                StartAt = a.StartAt.ToString("yyyy-MM-dd HH:mm"),
                EndAt = a.EndAt.ToString("yyyy-MM-dd HH:mm"),
                a.Status
            })
            .ToListAsync();

            return Ok(appointments);
        }

        // GET: api/AppointmentsApi/trainer/{trainerId} - Antrenörün randevularýný getir
        [HttpGet("trainer/{trainerId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetTrainerAppointments(int trainerId)
        {
            var appointments = await _context.Appointments
            .Include(a => a.Member)
            .Include(a => a.Service)
            .Where(a => a.TrainerId == trainerId)
            .OrderByDescending(a => a.StartAt)
            .Select(a => new
            {
                a.Id,
                MemberName = a.Member.FullName,
                ServiceName = a.Service.Name,
                StartAt = a.StartAt.ToString("yyyy-MM-dd HH:mm"),
                EndAt = a.EndAt.ToString("yyyy-MM-dd HH:mm"),
                a.Status
            })
            .ToListAsync();

            return Ok(appointments);
        }
    }
}
