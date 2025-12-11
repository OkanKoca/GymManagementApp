using System.ComponentModel.DataAnnotations;

namespace GymApp.Models
{
    public class Trainer
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ad soyad zorunludur.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Ad soyad 2-100 karakter arasında olmalıdır.")]
     [Display(Name = "Ad Soyad")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Uzmanlık alanı zorunludur.")]
  [StringLength(100, ErrorMessage = "Uzmanlık alanı en fazla 100 karakter olabilir.")]
    [Display(Name = "Uzmanlık Alanı")]
        public string Expertise { get; set; }

 [Display(Name = "Biyografi")]
 [StringLength(1000, ErrorMessage = "Biyografi en fazla 1000 karakter olabilir.")]
      public string? Bio { get; set; }

  [Display(Name = "Deneyim (Yıl)")]
   [Range(0, 50, ErrorMessage = "Deneyim 0-50 yıl arasında olmalıdır.")]
  public int? ExperienceYears { get; set; }

        [Display(Name = "Telefon")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
    [StringLength(20)]
        public string? Phone { get; set; }

        [Display(Name = "E-posta")]
      [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [StringLength(100)]
  public string? Email { get; set; }

        [Display(Name = "Fotoğraf")]
        [StringLength(500)]
        public string? PhotoUrl { get; set; }

        [Required(ErrorMessage = "Salon seçimi zorunludur.")]
     [Display(Name = "Salon")]
        public int GymId { get; set; }

        public Gym Gym { get; set; }

public ICollection<TrainerAvailability> Availabilities { get; set; } = new List<TrainerAvailability>();
   public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<TrainerService> TrainerServices { get; set; } = new List<TrainerService>();
    }
}
