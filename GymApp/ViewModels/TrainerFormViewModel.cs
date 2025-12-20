using GymApp.Models;
using System.ComponentModel.DataAnnotations;

namespace GymApp.ViewModels
{
    public class TrainerFormViewModel
    {
        public int Id { get; set; }

   [Required(ErrorMessage = "Ad soyad zorunludur.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Ad soyad 2-100 karakter arasýnda olmalýdýr.")]
        [Display(Name = "Ad Soyad")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Uzmanlýk alaný zorunludur.")]
        [StringLength(100, ErrorMessage = "Uzmanlýk alaný en fazla 100 karakter olabilir.")]
   [Display(Name = "Uzmanlýk Alaný")]
    public string Expertise { get; set; } = string.Empty;

      [Display(Name = "Biyografi")]
        [StringLength(1000, ErrorMessage = "Biyografi en fazla 1000 karakter olabilir.")]
        public string? Bio { get; set; }

    [Display(Name = "Deneyim (Yýl)")]
        [Range(0, 50, ErrorMessage = "Deneyim 0-50 yýl arasýnda olmalýdýr.")]
      public int? ExperienceYears { get; set; }

        [Display(Name = "Telefon")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarasý giriniz.")]
        [StringLength(20)]
      public string? Phone { get; set; }

        [Display(Name = "E-posta")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
      [StringLength(100)]
    public string? Email { get; set; }

        [Display(Name = "Fotoðraf")]
        [StringLength(500)]
        public string? PhotoUrl { get; set; }

        [Required(ErrorMessage = "Salon seçimi zorunludur.")]
        [Display(Name = "Salon")]
      public int GymId { get; set; }

        [Display(Name = "Sunduðu Hizmetler")]
        public List<int> SelectedServiceIds { get; set; } = new List<int>();

        // Dropdown için
        public List<Gym> Gyms { get; set; } = new List<Gym>();
        public List<Service> AvailableServices { get; set; } = new List<Service>();

        public Trainer ToTrainer()
        {
          return new Trainer
            {
              Id = Id,
           FullName = FullName,
                Expertise = Expertise,
             Bio = Bio,
          ExperienceYears = ExperienceYears,
   Phone = Phone,
  Email = Email,
                PhotoUrl = PhotoUrl,
          GymId = GymId
  };
        }

        public static TrainerFormViewModel FromTrainer(Trainer trainer)
        {
            return new TrainerFormViewModel
            {
         Id = trainer.Id,
           FullName = trainer.FullName,
             Expertise = trainer.Expertise,
       Bio = trainer.Bio,
                ExperienceYears = trainer.ExperienceYears,
             Phone = trainer.Phone,
                Email = trainer.Email,
  PhotoUrl = trainer.PhotoUrl,
         GymId = trainer.GymId,
             SelectedServiceIds = trainer.TrainerServices?.Select(ts => ts.ServiceId).ToList() ?? new List<int>()
            };
        }
    }
}
