using System.Collections.Generic;
using FitReserve.Models;

namespace FitReserve.ViewModels;

public class UyeGrupDersleriViewModel
{
    public List<GrupDersi> TumDersler { get; set; } = new();
    public List<int> KayitliDersIdleri { get; set; } = new();
    public Uye Uye { get; set; } = null!;
}
