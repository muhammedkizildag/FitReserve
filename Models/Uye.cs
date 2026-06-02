namespace FitReserve.Models;

public class Uye
{
    public int Id { get; set; }
    public string AdSoyad { get; set; } = string.Empty;
    public string Eposta { get; set; } = string.Empty;
    public string Sifre { get; set; } = string.Empty;
    public string Telefon { get; set; } = string.Empty;
    public string UyelikPaketiAdi { get; set; } = string.Empty;
    public DateTime KayitTarihi { get; set; }
    public bool AktifMi { get; set; } = true;
}
