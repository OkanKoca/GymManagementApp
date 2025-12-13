using System.ComponentModel.DataAnnotations;

namespace GymApp.Models
{
    public class TrainerService
    {
        public int Id { get; set; }

        [Required]
        public int TrainerId { get; set; }
        public Trainer Trainer { get; set; }

        [Required]
        public int ServiceId { get; set; }
        public Service Service { get; set; }
    }
}
