namespace FitReserve.Models;

public class Ders
{
    public int Id { get; set; }
    public string Ad { get; set; } = string.Empty;
    public string Kategori { get; set; } = string.Empty;
    public string EgitmenAdi { get; set; } = string.Empty;
    public int SureDakika { get; set; }
    public bool AktifMi { get; set; } = true;
}
