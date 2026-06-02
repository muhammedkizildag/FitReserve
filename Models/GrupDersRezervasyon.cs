using System;

namespace FitReserve.Models;

public class GrupDersRezervasyon
{
    public int Id { get; set; }
    public int UyeId { get; set; }
    public int GrupDersiId { get; set; }
    public DateTime RezervasyonTarihi { get; set; }
}
