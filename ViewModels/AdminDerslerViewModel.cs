using FitReserve.Models;

namespace FitReserve.ViewModels;

public class AdminDerslerViewModel
{
    public List<Ders> GrupDersleri { get; set; } = new();
    public List<GrupDersi> HaftalikGrupDersleri { get; set; } = new();
    public List<OzelDersTalebi> HaftalikOzelDersler { get; set; } = new();
    public DateTime HaftaBaslangici { get; set; }
    public DateTime HaftaBitisi { get; set; }
}
