using Microsoft.AspNetCore.Identity;

namespace GymApp.Models
{
    public class Member : IdentityUser
    {
        public string FullName { get; set; }
        public double? Height { get; set; }
        public double? Weight { get; set; }
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
