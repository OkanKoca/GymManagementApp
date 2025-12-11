using System.ComponentModel.DataAnnotations;

namespace GymApp.ViewModels
{
    public class AIRecommendationViewModel
    {
        [Display(Name = "Boy (cm)")]
     [Range(100, 250, ErrorMessage = "Boy 100-250 cm arasýnda olmalýdýr.")]
        public double? Height { get; set; }

        [Display(Name = "Kilo (kg)")]
    [Range(30, 300, ErrorMessage = "Kilo 30-300 kg arasýnda olmalýdýr.")]
        public double? Weight { get; set; }

     [Display(Name = "Yaþ")]
        [Range(10, 100, ErrorMessage = "Yaþ 10-100 arasýnda olmalýdýr.")]
        public int? Age { get; set; }

        [Display(Name = "Cinsiyet")]
        public string? Gender { get; set; }

    [Display(Name = "Hedef")]
[StringLength(200)]
        public string? Goal { get; set; } // Kilo vermek, kas yapmak, fit kalmak vb.

     [Display(Name = "Saðlýk Durumu / Kýsýtlamalar")]
        [StringLength(500)]
        public string? HealthConditions { get; set; }

        [Display(Name = "Aktivite Seviyesi")]
        public string? ActivityLevel { get; set; } // Sedanter, Hafif Aktif, Aktif, Çok Aktif

    [Display(Name = "Vücut Tipi")]
  public string? BodyType { get; set; } // Ektomorf, Mezomorf, Endomorf

        // Fotoðraf yükleme
        [Display(Name = "Fotoðraf")]
     public IFormFile? Photo { get; set; }

        public string? UploadedPhotoUrl { get; set; }

        // Sonuçlar
 public string? ExerciseRecommendation { get; set; }
        public string? DietRecommendation { get; set; }
        public string? GeneralAdvice { get; set; }
      public double? BMI { get; set; }
        public string? BMICategory { get; set; }

        // Kalori hesaplama sonuçlarý
        public double? BMR { get; set; } // Bazal Metabolizma Hýzý
        public double? TDEE { get; set; } // Günlük Kalori Ýhtiyacý
        public double? TargetCalories { get; set; } // Hedefe göre kalori
    }
}
