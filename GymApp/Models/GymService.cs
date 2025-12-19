using System.ComponentModel.DataAnnotations;

namespace GymApp.Models
{
    public class GymService
    {
        public int Id { get; set; }

    [Required]
   [Display(Name = "Salon")]
        public int GymId { get; set; }
        public Gym Gym { get; set; } = null!;

        [Required]
   [Display(Name = "Hizmet")]
        public int ServiceId { get; set; }
        public Service Service { get; set; } = null!;

        [Display(Name = "Aktif")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Özel Fiyat")]
  [Range(0, 100000, ErrorMessage = "Fiyat 0-100000 arasýnda olmalýdýr.")]
        public decimal? CustomPrice { get; set; } // Salon için özel fiyat (opsiyonel)
    }
}
