namespace FitReserve.Models;

public class GrupDersi
{
    public int Id { get; set; }
    public string DersAdi { get; set; } = string.Empty;
    public string EgitmenAdi { get; set; } = string.Empty;
    public DateTime BaslangicZamani { get; set; }
    public int SureDakika { get; set; }
    public int Kapasite { get; set; }
    public int KatilimciSayisi { get; set; }
    public bool IptalMi { get; set; }
}
