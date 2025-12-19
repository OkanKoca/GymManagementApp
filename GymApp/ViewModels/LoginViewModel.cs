using System.ComponentModel.DataAnnotations;

namespace GymApp.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "E-posta zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [Display(Name = "E-posta")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Þifre zorunludur.")]
        [DataType(DataType.Password)]
        [Display(Name = "Þifre")]
        public string Password { get; set; }
        public bool RememberMe { get; set; }
    }
}
