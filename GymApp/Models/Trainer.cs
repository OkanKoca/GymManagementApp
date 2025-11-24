namespace GymApp.Models
{
    public class Trainer
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Expertise { get; set; }
        public int GymId { get; set; }
        public Gym Gym { get; set; }
        public ICollection<TrainerAvailability> Availabilities { get; set; } = new List<TrainerAvailability>();
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
