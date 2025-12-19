using System.ComponentModel.DataAnnotations;

namespace GymApp.ViewModels
{
    public class BulkAvailabilityViewModel
    {
        [Required(ErrorMessage = "Eðitmen seçimi zorunludur.")]
     [Display(Name = "Eðitmen")]
  public int TrainerId { get; set; }

        [Required(ErrorMessage = "En az bir gün seçmelisiniz.")]
        [Display(Name = "Günler")]
        public List<DayOfWeek> SelectedDays { get; set; } = new List<DayOfWeek>();

        [Required(ErrorMessage = "Baþlangýç saati zorunludur.")]
  [DataType(DataType.Time)]
    [Display(Name = "Baþlangýç Saati")]
        public TimeSpan From { get; set; }

  [Required(ErrorMessage = "Bitiþ saati zorunludur.")]
   [DataType(DataType.Time)]
        [Display(Name = "Bitiþ Saati")]
    public TimeSpan To { get; set; }
    }
}
