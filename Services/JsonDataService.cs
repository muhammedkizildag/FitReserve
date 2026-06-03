using System.Text.Json;
using System.Linq;
using FitReserve.Models;

namespace FitReserve.Services;

public class JsonDataService
{
    public bool DersZamaniCakismasiVarMi(DateTime yeniBaslangic, int yeniSure, string egitmenAdi, int? haricTalepId = null)
    {
        // 1. Grup dersleriyle çakışma kontrolü
        var grupDersleri = Listele<GrupDersi>("grupDersleri.json")
            .Where(d => !d.IptalMi && d.EgitmenAdi.Equals(egitmenAdi, StringComparison.OrdinalIgnoreCase));
            
        foreach (var ders in grupDersleri)
        {
            var dersBitis = ders.BaslangicZamani.AddMinutes(ders.SureDakika);
            var yeniBitis = yeniBaslangic.AddMinutes(yeniSure);
            
            if (yeniBaslangic < dersBitis && ders.BaslangicZamani < yeniBitis)
            {
                return true;
            }
        }
        
        // 2. Onaylanmış özel derslerle çakışma kontrolü
        var ozelDersler = Listele<OzelDersTalebi>("ozelDersTalepleri.json")
            .Where(t => t.Durum == "Onaylandi" && t.EgitmenAdi.Equals(egitmenAdi, StringComparison.OrdinalIgnoreCase) && t.Id != haricTalepId);
            
        foreach (var ozel in ozelDersler)
        {
            var ozelBitis = ozel.IstenenTarih.AddMinutes(50);
            var yeniBitis = yeniBaslangic.AddMinutes(yeniSure);
            
            if (yeniBaslangic < ozelBitis && ozel.IstenenTarih < yeniBitis)
            {
                return true;
            }
        }
        
        return false;
    }
    private readonly string _dataKlasoru;
    private readonly JsonSerializerOptions _jsonAyarlar;

    public JsonDataService(IWebHostEnvironment ortam)
    {
        _dataKlasoru = Path.Combine(ortam.ContentRootPath, "Data");
        Directory.CreateDirectory(_dataKlasoru);

        _jsonAyarlar = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

        VeriSeedingYap();
    }

    public List<T> Listele<T>(string dosyaAdi)
    {
        var dosyaYolu = DosyaYoluOlustur(dosyaAdi);

        if (!File.Exists(dosyaYolu))
        {
            File.WriteAllText(dosyaYolu, "[]");
            return new List<T>();
        }

        var json = File.ReadAllText(dosyaYolu);
        return string.IsNullOrWhiteSpace(json)
            ? new List<T>()
            : JsonSerializer.Deserialize<List<T>>(json, _jsonAyarlar) ?? new List<T>();
    }

    public void Kaydet<T>(string dosyaAdi, List<T> veriler)
    {
        var dosyaYolu = DosyaYoluOlustur(dosyaAdi);
        var json = JsonSerializer.Serialize(veriler, _jsonAyarlar);
        File.WriteAllText(dosyaYolu, json);
    }

    private void VeriSeedingYap()
    {
        // 1. admin.json Seeding
        var adminYolu = DosyaYoluOlustur("admin.json");
        if (!File.Exists(adminYolu) || string.IsNullOrWhiteSpace(File.ReadAllText(adminYolu)) || File.ReadAllText(adminYolu) == "[]")
        {
            var varsayilanAdminler = new List<AdminKullanici>
            {
                new AdminKullanici { Id = 1, Eposta = "admin@fitreserve.com", Sifre = "admin123", AktifMi = true }
            };
            Kaydet("admin.json", varsayilanAdminler);
        }

        // 2. egitmenler.json Seeding
        var egitmenlerYolu = DosyaYoluOlustur("egitmenler.json");
        if (!File.Exists(egitmenlerYolu) || string.IsNullOrWhiteSpace(File.ReadAllText(egitmenlerYolu)) || File.ReadAllText(egitmenlerYolu) == "[]")
        {
            var varsayilanEgitmenler = new List<Egitmen>
            {
                new Egitmen { Id = 1, AdSoyad = "Selin Arslan", UzmanlikAlani = "Pilates", Eposta = "selin.arslan@fitreserve.com", Telefon = "0532 418 77 12", Sifre = "selinarslan", AktifMi = true },
                new Egitmen { Id = 2, AdSoyad = "Mina Akbas", UzmanlikAlani = "Yoga", Eposta = "mina.akbas@fitreserve.com", Telefon = "0507 263 84 19", Sifre = "minaakbas", AktifMi = true },
                new Egitmen { Id = 3, AdSoyad = "Taylan Ergin", UzmanlikAlani = "Cross Training", Eposta = "taylan.ergin@fitreserve.com", Telefon = "0553 691 20 74", Sifre = "taylanergin", AktifMi = true }
            };
            Kaydet("egitmenler.json", varsayilanEgitmenler);
        }

        // 3. uyelikPaketleri.json Seeding
        var paketlerYolu = DosyaYoluOlustur("uyelikPaketleri.json");
        if (!File.Exists(paketlerYolu) || string.IsNullOrWhiteSpace(File.ReadAllText(paketlerYolu)) || File.ReadAllText(paketlerYolu) == "[]")
        {
            var varsayilanPaketler = new List<UyelikPaketi>
            {
                new UyelikPaketi { Id = 1, Ad = "Standart Üyelik", Aciklama = "Haftada 3 gün grup derslerine katılım hakkı.", SureGun = 30, Fiyat = 750.0m, AktifMi = true },
                new UyelikPaketi { Id = 2, Ad = "Premium Üyelik", Aciklama = "Sınırsız grup dersleri ve 1e1 özel ders talebi hakkı.", SureGun = 30, Fiyat = 1500.0m, AktifMi = true }
            };
            Kaydet("uyelikPaketleri.json", varsayilanPaketler);
        }

        // 4. dersler.json Seeding
        var derslerYolu = DosyaYoluOlustur("dersler.json");
        if (!File.Exists(derslerYolu) || string.IsNullOrWhiteSpace(File.ReadAllText(derslerYolu)) || File.ReadAllText(derslerYolu) == "[]")
        {
            var varsayilanDersler = new List<Ders>
            {
                new Ders { Id = 1, Ad = "Pilates", Kategori = "Grup Dersi", EgitmenAdi = "Selin Arslan", SureDakika = 50, AktifMi = true },
                new Ders { Id = 2, Ad = "Yoga", Kategori = "Grup Dersi", EgitmenAdi = "Mina Akbas", SureDakika = 60, AktifMi = true },
                new Ders { Id = 3, Ad = "Cross Training", Kategori = "Grup Dersi", EgitmenAdi = "Taylan Ergin", SureDakika = 50, AktifMi = true }
            };
            Kaydet("dersler.json", varsayilanDersler);
        }
    }

    private string DosyaYoluOlustur(string dosyaAdi)
    {
        var guvenliDosyaAdi = Path.GetFileName(dosyaAdi);
        return Path.Combine(_dataKlasoru, guvenliDosyaAdi);
    }
}
