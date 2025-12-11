using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace GymApp.Models
{
    public class Member : IdentityUser
    {
        [Required(ErrorMessage = "Ad soyad zorunludur.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Ad soyad 2-100 karakter arasında olmalıdır.")]
        [Display(Name = "Ad Soyad")]
        public string FullName { get; set; }

        [Range(100, 250, ErrorMessage = "Boy 100-250 cm arasında olmalıdır.")]
        [Display(Name = "Boy (cm)")]
        public double? Height { get; set; }

        [Range(30, 300, ErrorMessage = "Kilo 30-300 kg arasında olmalıdır.")]
        [Display(Name = "Kilo (kg)")]
        public double? Weight { get; set; }

        [Display(Name = "Yaş")]
        [Range(10, 100, ErrorMessage = "Yaş 10-100 arasında olmalıdır.")]
        public int? Age { get; set; }

        [Display(Name = "Cinsiyet")]
        [StringLength(20)]
        public string? Gender { get; set; }

        [Display(Name = "Profil Fotoğrafı")]
        [StringLength(500)]
        public string? ProfilePhotoUrl { get; set; }

        [Display(Name = "Vücut Tipi")]
        [StringLength(50)]
        public string? BodyType { get; set; } // Ektomorf, Mezomorf, Endomorf

        [Display(Name = "Fitness Hedefi")]
        [StringLength(200)]
        public string? FitnessGoal { get; set; }

        [Display(Name = "Kayıt Tarihi")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
