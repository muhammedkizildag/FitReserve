using System.ComponentModel.DataAnnotations;

namespace FitReserve.ViewModels;

public class UyeProfilViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Ad soyad zorunludur.")]
    public string AdSoyad { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-posta adresi zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta giriniz.")]
    public string Eposta { get; set; } = string.Empty;

    public string Telefon { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    public string? Sifre { get; set; }
}
