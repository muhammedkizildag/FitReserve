using FitReserve.Models;

namespace FitReserve.ViewModels;

public class AdminEgitmenlerViewModel
{
    public List<EgitmenKayitTalebi> BekleyenKayitTalepleri { get; set; } = new();
    public List<Egitmen> Egitmenler { get; set; } = new();
}
