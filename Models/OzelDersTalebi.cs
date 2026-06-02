namespace FitReserve.Models;

public class OzelDersTalebi
{
    public int Id { get; set; }
    public string UyeAdi { get; set; } = string.Empty;
    public string EgitmenAdi { get; set; } = string.Empty;
    public string DersAdi { get; set; } = string.Empty;
    public DateTime TalepTarihi { get; set; }
    public DateTime IstenenTarih { get; set; }
    public string Durum { get; set; } = "Bekliyor";
    public string Not { get; set; } = string.Empty;
}
