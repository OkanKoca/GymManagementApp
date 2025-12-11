using System.ComponentModel.DataAnnotations;

namespace GymApp.Models
{
    public class Service
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Hizmet adı zorunludur.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Hizmet adı 2-100 karakter arasında olmalıdır.")]
     [Display(Name = "Hizmet Adı")]
        public string Name { get; set; }

     [Display(Name = "Açıklama")]
        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        public string? Description { get; set; }

     [Required(ErrorMessage = "Hizmet türü zorunludur.")]
   [StringLength(50, ErrorMessage = "Hizmet türü en fazla 50 karakter olabilir.")]
        [Display(Name = "Hizmet Türü")]
        public string ServiceType { get; set; } = "Fitness"; // Fitness, Yoga, Pilates, Personal Training, vb.

     [Required(ErrorMessage = "Süre zorunludur.")]
       [Range(15, 480, ErrorMessage = "Süre 15-480 dakika arasında olmalıdır.")]
   [Display(Name = "Süre (Dakika)")]
        public int DurationMinutes { get; set; }

        [Required(ErrorMessage = "Fiyat zorunludur.")]
      [Range(0.01, 100000, ErrorMessage = "Fiyat 0.01-100000 arasında olmalıdır.")]
       [DataType(DataType.Currency)]
   [Display(Name = "Fiyat")]
        public decimal Price { get; set; }

      [Display(Name = "Aktif")]
        public bool IsActive { get; set; } = true;

        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<TrainerService> TrainerServices { get; set; } = new List<TrainerService>();
    }
}
