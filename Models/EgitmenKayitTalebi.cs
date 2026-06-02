namespace FitReserve.Models;

public class EgitmenKayitTalebi
{
    public int Id { get; set; }
    public string AdSoyad { get; set; } = string.Empty;
    public string UzmanlikAlani { get; set; } = string.Empty;
    public string Eposta { get; set; } = string.Empty;
    public string Sifre { get; set; } = string.Empty;
    public string Telefon { get; set; } = string.Empty;
    public DateTime TalepTarihi { get; set; }
    public string Durum { get; set; } = "Bekliyor";
}
