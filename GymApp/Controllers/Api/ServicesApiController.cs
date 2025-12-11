using GymApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServicesApiController : ControllerBase
    {
   private readonly GymDbContext _context;

     public ServicesApiController(GymDbContext context)
        {
       _context = context;
     }

// GET: api/ServicesApi - Tüm hizmetleri listele
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetServices()
 {
      var services = await _context.Services
        .Select(s => new
      {
        s.Id,
   s.Name,
     s.DurationMinutes,
        s.Price
   })
            .ToListAsync();

return Ok(services);
     }

     // GET: api/ServicesApi/5 - Belirli bir hizmeti getir
 [HttpGet("{id}")]
   public async Task<ActionResult<object>> GetService(int id)
        {
    var service = await _context.Services
         .Where(s => s.Id == id)
    .Select(s => new
         {
       s.Id,
            s.Name,
        s.DurationMinutes,
  s.Price
            })
    .FirstOrDefaultAsync();

 if (service == null)
   {
      return NotFound(new { message = "Hizmet bulunamadý." });
      }

         return Ok(service);
     }
    }
}
