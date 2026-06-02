using System.ComponentModel.DataAnnotations;

namespace FitReserve.ViewModels;

public class DersAcFormViewModel
{
    public int DersId { get; set; }
    public string DersAdi { get; set; } = string.Empty;
    public string EgitmenAdi { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tarih ve saat zorunludur.")]
    public DateTime BaslangicZamani { get; set; } = DateTime.Now.AddDays(1);

    [Range(1, 100, ErrorMessage = "Kontenjan en az 1 olmalidir.")]
    public int Kapasite { get; set; } = 12;
}
