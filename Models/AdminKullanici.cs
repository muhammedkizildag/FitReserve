namespace FitReserve.Models;

public class AdminKullanici
{
    public int Id { get; set; }
    public string Eposta { get; set; } = string.Empty;
    public string Sifre { get; set; } = string.Empty;
    public bool AktifMi { get; set; } = true;
}
