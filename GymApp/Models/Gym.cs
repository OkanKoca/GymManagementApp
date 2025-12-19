using System.ComponentModel.DataAnnotations;

namespace GymApp.Models
{
    public class Gym
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Salon adı zorunludur.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Salon adı 2-100 karakter arasında olmalıdır.")]
        [Display(Name = "Salon Adı")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Adres zorunludur.")]
        [StringLength(250, ErrorMessage = "Adres en fazla 250 karakter olabilir.")]
        [Display(Name = "Adres")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Çalışma saatleri zorunludur.")]
        [StringLength(50, ErrorMessage = "Çalışma saatleri en fazla 50 karakter olabilir.")]
        [Display(Name = "Çalışma Saatleri")]
        public string WorkingHours { get; set; }

        [Display(Name = "Telefon")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
        [StringLength(20)]
        public string? Phone { get; set; }

        [Display(Name = "E-posta")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [StringLength(100)]
        public string? Email { get; set; }

        [Display(Name = "Açıklama")]
        [StringLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir.")]
        public string? Description { get; set; }

        [Display(Name = "Fotoğraf")]
        [StringLength(500)]
        public string? PhotoUrl { get; set; }

        public ICollection<Trainer> Trainers { get; set; } = new List<Trainer>();
        public ICollection<GymService> GymServices { get; set; } = new List<GymService>();
    }
}
