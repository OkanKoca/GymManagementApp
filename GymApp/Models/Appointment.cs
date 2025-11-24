namespace GymApp.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        public string MemberId { get; set; }
        public Member Member { get; set; }
        public int TrainerId { get; set; }
        public Trainer Trainer { get; set; }
        public int ServiceId { get; set; }
        public Service Service { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        public string Status { get; set; } // e.g., Scheduled, Completed, Canceled
    }
}
