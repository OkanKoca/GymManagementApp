using System.ComponentModel.DataAnnotations;

namespace GymApp.Models
{
    public class Appointment
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Üye seçimi zorunludur.")]
        [Display(Name = "Üye")]
        public string MemberId { get; set; }
        public Member Member { get; set; }

        [Required(ErrorMessage = "Eğitmen seçimi zorunludur.")]
        [Display(Name = "Eğitmen")]
        public int TrainerId { get; set; }
        public Trainer Trainer { get; set; }

        [Required(ErrorMessage = "Hizmet seçimi zorunludur.")]
        [Display(Name = "Hizmet")]
        public int ServiceId { get; set; }
        public Service Service { get; set; }

        [Required(ErrorMessage = "Başlangıç zamanı zorunludur.")]
        [DataType(DataType.DateTime)]
        [Display(Name = "Başlangıç Zamanı")]
        public DateTime StartAt { get; set; }

        [Required(ErrorMessage = "Bitiş zamanı zorunludur.")]
        [DataType(DataType.DateTime)]
        [Display(Name = "Bitiş Zamanı")]
        public DateTime EndAt { get; set; }

        [Required(ErrorMessage = "Durum zorunludur.")]
        [StringLength(20, ErrorMessage = "Durum en fazla 20 karakter olabilir.")]
        [Display(Name = "Durum")]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected, Completed, Canceled

        [Display(Name = "Oluşturulma Tarihi")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Not")]
        [StringLength(500, ErrorMessage = "Not en fazla 500 karakter olabilir.")]
        public string? Notes { get; set; }
    }
}
