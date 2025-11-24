using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using GymApp.Models;
using Microsoft.EntityFrameworkCore;

namespace GymApp.Data
{
    public class GymDbContext : IdentityDbContext<Member>
    {
        public GymDbContext(DbContextOptions<GymDbContext> options) : base(options) { }

        public DbSet<Gym> Gyms { get; set; }
        public DbSet<Trainer> Trainers { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<TrainerAvailability> TrainerAvailabilities { get; set; }

    }
}
