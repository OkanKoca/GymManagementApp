namespace GymApp.Models
{
    public class Gym
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string WorkingHours { get; set; }
        public ICollection<Trainer> Trainers { get; set; } = new List<Trainer>();
    }
}
