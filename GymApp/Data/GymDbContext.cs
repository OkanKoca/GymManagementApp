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
        public DbSet<TrainerService> TrainerServices { get; set; }
        public DbSet<GymService> GymServices { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // TrainerService - Many to Many ilişki
            modelBuilder.Entity<TrainerService>()
                .HasOne(ts => ts.Trainer)
                .WithMany(t => t.TrainerServices)
                .HasForeignKey(ts => ts.TrainerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TrainerService>()
                .HasOne(ts => ts.Service)
                .WithMany(s => s.TrainerServices)
                .HasForeignKey(ts => ts.ServiceId)
                .OnDelete(DeleteBehavior.Cascade);

            // GymService - Many to Many ilişki (Salon-Hizmet)
            modelBuilder.Entity<GymService>()
                .HasOne(gs => gs.Gym)
                .WithMany(g => g.GymServices)
                .HasForeignKey(gs => gs.GymId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GymService>()
                .HasOne(gs => gs.Service)
                .WithMany(s => s.GymServices)
                .HasForeignKey(gs => gs.ServiceId)
                .OnDelete(DeleteBehavior.Cascade);

            // Trainer - Gym ilişkisi
            modelBuilder.Entity<Trainer>()
                .HasOne(t => t.Gym)
                .WithMany(g => g.Trainers)
                .HasForeignKey(t => t.GymId)
                .OnDelete(DeleteBehavior.Restrict);

            // Appointment ilişkileri
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Member)
                .WithMany(m => m.Appointments)
                .HasForeignKey(a => a.MemberId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Trainer)
                .WithMany(t => t.Appointments)
                .HasForeignKey(a => a.TrainerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Service)
                .WithMany(s => s.Appointments)
                .HasForeignKey(a => a.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
