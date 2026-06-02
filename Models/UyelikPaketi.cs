namespace FitReserve.Models;

public class UyelikPaketi
{
    public int Id { get; set; }
    public string Ad { get; set; } = string.Empty;
    public string Aciklama { get; set; } = string.Empty;
    public int SureGun { get; set; }
    public decimal Fiyat { get; set; }
    public bool AktifMi { get; set; } = true;
}
