using System.ComponentModel.DataAnnotations;

namespace FitReserve.ViewModels;

public class LoginGirisViewModel
{
    [Required(ErrorMessage = "Rol secimi zorunludur.")]
    public string Rol { get; set; } = string.Empty;

    [Required(ErrorMessage = "Eposta zorunludur.")]
    [EmailAddress(ErrorMessage = "Gecerli bir eposta giriniz.")]
    public string Eposta { get; set; } = string.Empty;

    [Required(ErrorMessage = "Sifre zorunludur.")]
    public string Sifre { get; set; } = string.Empty;
}
