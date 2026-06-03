namespace FitReserve.Models;

public class Bildirim
{
    public int Id { get; set; }
    public string Baslik { get; set; } = string.Empty;
    public string Mesaj { get; set; } = string.Empty;
    public DateTime GonderimTarihi { get; set; }
    public bool OkunduMu { get; set; }
    public int? AliciUyeId { get; set; }
    public int? AliciEgitmenId { get; set; }
}
