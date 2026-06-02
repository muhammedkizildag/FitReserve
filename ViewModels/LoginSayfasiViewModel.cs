using FitReserve.Models;

namespace FitReserve.ViewModels;

public class LoginSayfasiViewModel
{
    public LoginGirisViewModel Giris { get; set; } = new();
    public UyeKayitViewModel UyeKayit { get; set; } = new();
    public EgitmenKayitViewModel EgitmenKayit { get; set; } = new();
    public List<UyelikPaketi> UyelikPaketleri { get; set; } = new();
}
