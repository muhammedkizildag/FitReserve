using System.ComponentModel.DataAnnotations;

namespace FitReserve.ViewModels;

public class GrupDersiGuncelleFormViewModel
{
    public int Id { get; set; }
    public string DersAdi { get; set; } = string.Empty;
    public string EgitmenAdi { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tarih ve saat zorunludur.")]
    public DateTime BaslangicZamani { get; set; }

    [Range(1, 100, ErrorMessage = "Kontenjan en az 1 olmalidir.")]
    public int Kapasite { get; set; }
}
