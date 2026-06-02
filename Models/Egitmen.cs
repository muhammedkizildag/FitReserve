namespace FitReserve.Models;

public class Egitmen
{
    public int Id { get; set; }
    public string AdSoyad { get; set; } = string.Empty;
    public string UzmanlikAlani { get; set; } = string.Empty;
    public string Eposta { get; set; } = string.Empty;
    public string Sifre { get; set; } = string.Empty;
    public string Telefon { get; set; } = string.Empty;
    public bool AktifMi { get; set; } = true;
}
