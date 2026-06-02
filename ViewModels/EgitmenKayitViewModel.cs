using System.ComponentModel.DataAnnotations;

namespace FitReserve.ViewModels;

public class EgitmenKayitViewModel
{
    [Required(ErrorMessage = "Ad soyad zorunludur.")]
    public string AdSoyad { get; set; } = string.Empty;

    [Required(ErrorMessage = "Uzmanlik alani zorunludur.")]
    public string UzmanlikAlani { get; set; } = string.Empty;

    [Required(ErrorMessage = "Eposta zorunludur.")]
    [EmailAddress(ErrorMessage = "Gecerli bir eposta giriniz.")]
    public string Eposta { get; set; } = string.Empty;

    public string Telefon { get; set; } = string.Empty;

    [Required(ErrorMessage = "Sifre zorunludur.")]
    public string Sifre { get; set; } = string.Empty;
}
