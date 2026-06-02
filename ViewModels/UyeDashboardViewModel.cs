using System.Collections.Generic;
using FitReserve.Models;

namespace FitReserve.ViewModels;

public class UyeDashboardViewModel
{
    public Uye Uye { get; set; } = null!;
    public List<Bildirim> Bildirimler { get; set; } = new();
    public List<GrupDersi> GelecekDersler { get; set; } = new();
    public List<OzelDersTalebi> GelecekOzelDersler { get; set; } = new();
    public int OkunmamisBildirimSayisi { get; set; }
}
