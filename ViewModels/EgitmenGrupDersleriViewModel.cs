using System.Collections.Generic;
using FitReserve.Models;

namespace FitReserve.ViewModels;

public class EgitmenGrupDersleriViewModel
{
    public List<GrupDersi> Dersler { get; set; } = new();
    public List<GrupDersRezervasyon> Rezervasyonlar { get; set; } = new();
    public List<Uye> Uyeler { get; set; } = new();
}
