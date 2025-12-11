using System.ComponentModel.DataAnnotations;

namespace GymApp.Models
{
    public class TrainerAvailability
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Eğitmen seçimi zorunludur.")]
        [Display(Name = "Eğitmen")]
        public int TrainerId { get; set; }
        public Trainer Trainer { get; set; }

        [Required(ErrorMessage = "Gün seçimi zorunludur.")]
        [Display(Name = "Gün")]
        public DayOfWeek Day { get; set; }

        [Required(ErrorMessage = "Başlangıç saati zorunludur.")]
        [DataType(DataType.Time)]
        [Display(Name = "Başlangıç Saati")]
        public TimeSpan From { get; set; }

        [Required(ErrorMessage = "Bitiş saati zorunludur.")]
        [DataType(DataType.Time)]
        [Display(Name = "Bitiş Saati")]
        public TimeSpan To { get; set; }
    }
}
