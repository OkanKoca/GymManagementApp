using System.ComponentModel.DataAnnotations;

namespace GymApp.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Ad soyad zorunludur.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Ad soyad 2-100 karakter arasýnda olmalýdýr.")]
        [Display(Name = "Ad Soyad")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "E-posta zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [Display(Name = "E-posta")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Þifre zorunludur.")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Þifre en az 3 karakter olmalýdýr.")]
        [Display(Name = "Þifre")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Þifre tekrarý zorunludur.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Þifreler eþleþmiyor.")]
        [Display(Name = "Þifre Tekrarý")]
        public string ConfirmPassword { get; set; }

        [Range(100, 250, ErrorMessage = "Boy 100-250 cm arasýnda olmalýdýr.")]
        [Display(Name = "Boy (cm)")]
        public double? Height { get; set; }

        [Range(30, 300, ErrorMessage = "Kilo 30-300 kg arasýnda olmalýdýr.")]
        [Display(Name = "Kilo (kg)")]
        public double? Weight { get; set; }

        [Range(10, 100, ErrorMessage = "Yaþ 10-100 arasýnda olmalýdýr.")]
        [Display(Name = "Yaþ")]
        public int? Age { get; set; }

        [Display(Name = "Cinsiyet")]
        public string? Gender { get; set; }

        [Display(Name = "Fitness Hedefi")]
        [StringLength(200)]
        public string? FitnessGoal { get; set; }
    }
}
