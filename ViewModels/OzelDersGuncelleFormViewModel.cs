using System.ComponentModel.DataAnnotations;

namespace FitReserve.ViewModels;

public class OzelDersGuncelleFormViewModel
{
    public int Id { get; set; }
    public string DersAdi { get; set; } = string.Empty;
    public string UyeAdi { get; set; } = string.Empty;
    public string EgitmenAdi { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tarih ve saat zorunludur.")]
    public DateTime IstenenTarih { get; set; }
}
