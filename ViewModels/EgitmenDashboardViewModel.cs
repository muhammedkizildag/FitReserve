using System;
using System.Collections.Generic;
using FitReserve.Models;

namespace FitReserve.ViewModels;

public class EgitmenDashboardViewModel
{
    public Egitmen Egitmen { get; set; } = new();
    public List<GrupDersi> GelecekDersler { get; set; } = new();
    public List<OzelDersTalebi> GelecekOzelDersler { get; set; } = new();
    public List<Bildirim> Bildirimler { get; set; } = new();
    public int BugunkuDersSayisi { get; set; }
    public int BekleyenOzelDersSayisi { get; set; }
    public int ToplamKatilimciSayisi { get; set; }
    public int OkunmamisBildirimSayisi { get; set; }
}
