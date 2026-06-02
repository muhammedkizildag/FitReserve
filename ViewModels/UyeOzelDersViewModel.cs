using System.Collections.Generic;
using FitReserve.Models;

namespace FitReserve.ViewModels;

public class UyeOzelDersViewModel
{
    public List<Egitmen> Egitmenler { get; set; } = new();
    public List<OzelDersTalebi> Talepler { get; set; } = new();
    public Uye Uye { get; set; } = null!;
}
