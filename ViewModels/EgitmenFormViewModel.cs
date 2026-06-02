using System.ComponentModel.DataAnnotations;

namespace FitReserve.ViewModels;

public class EgitmenFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Ad soyad zorunludur.")]
    public string AdSoyad { get; set; } = string.Empty;

    [Required(ErrorMessage = "Uzmanlik alani zorunludur.")]
    public string UzmanlikAlani { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Gecerli bir eposta giriniz.")]
    public string Eposta { get; set; } = string.Empty;

    public string Telefon { get; set; } = string.Empty;
}
