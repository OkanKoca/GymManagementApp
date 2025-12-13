using System.ComponentModel.DataAnnotations;

namespace GymApp.ViewModels
{
    public class AppointmentCreateViewModel
    {
        [Required(ErrorMessage = "Eðitmen seçimi zorunludur.")]
        [Display(Name = "Eðitmen")]
        public int TrainerId { get; set; }

        [Required(ErrorMessage = "Hizmet seçimi zorunludur.")]
        [Display(Name = "Hizmet")]
        public int ServiceId { get; set; }

        [Required(ErrorMessage = "Randevu tarihi zorunludur.")]
        [DataType(DataType.Date)]
        [Display(Name = "Randevu Tarihi")]
        public DateTime AppointmentDate { get; set; }

        [Required(ErrorMessage = "Randevu saati zorunludur.")]
        [DataType(DataType.Time)]
        [Display(Name = "Randevu Saati")]
        public TimeSpan AppointmentTime { get; set; }

        [Display(Name = "Not")]
        [StringLength(500, ErrorMessage = "Not en fazla 500 karakter olabilir.")]
        public string? Notes { get; set; }
    }
}
