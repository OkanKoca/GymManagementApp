namespace GymApp.Models
{
    public class Service
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int DurationMinutes { get; set; }
        public decimal Price { get; set; }
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    }
}
