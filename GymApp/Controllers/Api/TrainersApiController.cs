using GymApp.Data;
using GymApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrainersApiController : ControllerBase
    {
        private readonly GymDbContext _context;

        public TrainersApiController(GymDbContext context)
        {
            _context = context;
        }

        // GET: api/TrainersApi - Tüm antrenörleri listele
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetTrainers()
        {
            var trainers = await _context.Trainers
              .Include(t => t.Gym)
                   .Include(t => t.Availabilities)
                .Select(t => new
                {
                    t.Id,
                    t.FullName,
                    t.Expertise,
                    t.Bio,
                    t.ExperienceYears,
                    t.Phone,
                    t.Email,
                    t.PhotoUrl,
                    Gym = new
                    {
                        t.Gym.Id,
                        t.Gym.Name,
                        t.Gym.Address,
                        t.Gym.WorkingHours
                    },
                    Availabilities = t.Availabilities.Select(a => new
                    {
                        Day = a.Day.ToString(),
                        DayNumber = (int)a.Day,
                        From = a.From.ToString(@"hh\:mm"),
                        To = a.To.ToString(@"hh\:mm")
                    })
                })
                 .ToListAsync();

            return Ok(trainers);
        }

        // GET: api/TrainersApi/5 - Belirli bir antrenörü getir
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetTrainer(int id)
        {
            var trainer = await _context.Trainers
       .Include(t => t.Gym)
    .Include(t => t.Availabilities)
      .Include(t => t.TrainerServices)
     .ThenInclude(ts => ts.Service)
        .Where(t => t.Id == id)
           .Select(t => new
            {
                t.Id,
                t.FullName,
                t.Expertise,
                t.Bio,
                t.ExperienceYears,
                t.Phone,
                t.Email,
                t.PhotoUrl,
     Gym = new
          {
   t.Gym.Id,
        t.Gym.Name,
        t.Gym.Address,
           t.Gym.WorkingHours
   },
                Availabilities = t.Availabilities.Select(a => new
        {
           Day = a.Day.ToString(),
  DayNumber = (int)a.Day,
  From = a.From.ToString(@"hh\:mm"),
       To = a.To.ToString(@"hh\:mm")
    }),
     Services = t.TrainerServices.Select(ts => new
  {
  ts.Service.Id,
      ts.Service.Name,
           ts.Service.DurationMinutes,
       ts.Service.Price
   })
        })
     .FirstOrDefaultAsync();

      if (trainer == null)
{
     return NotFound(new { message = "Antrenör bulunamadý." });
   }

    return Ok(trainer);
        }

        // GET: api/TrainersApi/bydate?date=2024-01-15 - Belirli bir tarihte uygun antrenörler
        [HttpGet("bydate")]
        public async Task<ActionResult<IEnumerable<object>>> GetTrainersByDate([FromQuery] DateTime date)
        {
          var dayOfWeek = date.DayOfWeek;

    var trainers = await _context.Trainers
   .Include(t => t.Gym)
  .Include(t => t.Availabilities)
     .Where(t => t.Availabilities.Any(a => a.Day == dayOfWeek))
 .Select(t => new
          {
 t.Id,
     t.FullName,
      t.Expertise,
     Gym = new
        {
       t.Gym.Id,
           t.Gym.Name
      },
    AvailableHours = t.Availabilities
.Where(a => a.Day == dayOfWeek)
     .Select(a => new
      {
  From = a.From.ToString(@"hh\:mm"),
       To = a.To.ToString(@"hh\:mm")
   })
      })
     .ToListAsync();

     return Ok(trainers);
 }
    }
}
