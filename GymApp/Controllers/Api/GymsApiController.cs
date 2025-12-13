using GymApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class GymsApiController : ControllerBase
    {
        private readonly GymDbContext _context;

        public GymsApiController(GymDbContext context)
        {
            _context = context;
        }

        // GET: api/GymsApi - Tüm spor salonlarýný listele
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetGyms()
        {
            var gyms = await _context.Gyms
        .Include(g => g.Trainers)
               .Select(g => new
               {
                   g.Id,
                   g.Name,
                   g.Address,
                   g.WorkingHours,
                   TrainerCount = g.Trainers.Count,
                   Trainers = g.Trainers.Select(t => new
                   {
                       t.Id,
                       t.FullName,
                       t.Expertise
                   })
               })
         .ToListAsync();

            return Ok(gyms);
        }

        // GET: api/GymsApi/5 - Belirli bir salonu getir
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetGym(int id)
        {
            var gym = await _context.Gyms
                .Include(g => g.Trainers)
          .ThenInclude(t => t.Availabilities)
         .Where(g => g.Id == id)
            .Select(g => new
            {
                g.Id,
                g.Name,
                g.Address,
                g.WorkingHours,
                Trainers = g.Trainers.Select(t => new
                {
                    t.Id,
                    t.FullName,
                    t.Expertise,
                    Availabilities = t.Availabilities.Select(a => new
                    {
                        Day = a.Day.ToString(),
                        From = a.From.ToString(@"hh\:mm"),
                        To = a.To.ToString(@"hh\:mm")
                    })
                })
            })
               .FirstOrDefaultAsync();

            if (gym == null)
            {
                return NotFound(new { message = "Spor salonu bulunamadý." });
            }

            return Ok(gym);
        }
    }
}
