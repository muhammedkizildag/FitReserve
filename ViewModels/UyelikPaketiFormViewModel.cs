using System.ComponentModel.DataAnnotations;

namespace FitReserve.ViewModels;

public class UyelikPaketiFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Paket adi zorunludur.")]
    public string Ad { get; set; } = string.Empty;

    public string Aciklama { get; set; } = string.Empty;

    [Range(1, 3650, ErrorMessage = "Sure en az 1 gun olmalidir.")]
    public int SureGun { get; set; }

    [Range(0, 100000, ErrorMessage = "Fiyat negatif olamaz.")]
    public decimal Fiyat { get; set; }
}
