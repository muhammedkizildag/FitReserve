using System;
using System.Collections.Generic;
using System.Linq;
using FitReserve.Helpers;
using FitReserve.Models;
using FitReserve.Services;
using FitReserve.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FitReserve.Controllers;

public class UyeController : BaseController
{
    private const string UyelerDosyasi = "uyeler.json";
    private const string EgitmenlerDosyasi = "egitmenler.json";
    private const string UyelikPaketleriDosyasi = "uyelikPaketleri.json";
    private const string DerslerDosyasi = "dersler.json";
    private const string GrupDersleriDosyasi = "grupDersleri.json";
    private const string OzelDersTalepleriDosyasi = "ozelDersTalepleri.json";
    private const string BildirimlerDosyasi = "bildirimler.json";
    private const string GrupDersRezervasyonlariDosyasi = "grupDersRezervasyonlari.json";

    private readonly JsonDataService _jsonDataService;

    public UyeController(JsonDataService jsonDataService)
    {
        _jsonDataService = jsonDataService;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.HttpContext.Request.Cookies.ContainsKey("UyeId"))
        {
            TempData["HataMesaji"] = "Lütfen önce giriş yapınız.";
            context.Result = RedirectToAction("Index", "Login");
        }
        base.OnActionExecuting(context);
    }

    private Uye? AktifUyeGetir()
    {
        if (Request.Cookies.TryGetValue("UyeId", out var idStr) && int.TryParse(idStr, out var id))
        {
            return _jsonDataService.Listele<Uye>(UyelerDosyasi).FirstOrDefault(u => u.Id == id && u.AktifMi);
        }
        return null;
    }

    public IActionResult Dashboard()
    {
        var uye = AktifUyeGetir();
        if (uye is null)
        {
            HataMesaji("Lutfen giris yapiniz.");
            return RedirectToAction("Index", "Login");
        }

        var bildirimler = _jsonDataService.Listele<Bildirim>(BildirimlerDosyasi)
            .Where(b => b.AliciUyeId == uye.Id)
            .OrderByDescending(b => b.GonderimTarihi)
            .Take(5)
            .ToList();

        var rezervasyonlar = _jsonDataService.Listele<GrupDersRezervasyon>(GrupDersRezervasyonlariDosyasi)
            .Where(r => r.UyeId == uye.Id)
            .Select(r => r.GrupDersiId)
            .ToList();

        var grupDersleri = _jsonDataService.Listele<GrupDersi>(GrupDersleriDosyasi);
        var gelecekDersler = grupDersleri
            .Where(d => rezervasyonlar.Contains(d.Id) && d.BaslangicZamani >= DateTime.Now && !d.IptalMi)
            .OrderBy(d => d.BaslangicZamani)
            .ToList();

        var ozelDersler = _jsonDataService.Listele<OzelDersTalebi>(OzelDersTalepleriDosyasi);
        var gelecekOzelDersler = ozelDersler
            .Where(t => t.UyeAdi.Equals(uye.AdSoyad, StringComparison.OrdinalIgnoreCase) && t.IstenenTarih >= DateTime.Now && t.Durum == "Onaylandi")
            .OrderBy(t => t.IstenenTarih)
            .ToList();

        var okunmamisSayi = _jsonDataService.Listele<Bildirim>(BildirimlerDosyasi)
            .Count(b => b.AliciUyeId == uye.Id && !b.OkunduMu);

        var model = new UyeDashboardViewModel
        {
            Uye = uye,
            Bildirimler = bildirimler,
            GelecekDersler = gelecekDersler,
            GelecekOzelDersler = gelecekOzelDersler,
            OkunmamisBildirimSayisi = okunmamisSayi
        };

        ViewData["UyeAd"] = uye.AdSoyad;
        return View(model);
    }

    public IActionResult Paketler()
    {
        var uye = AktifUyeGetir();
        if (uye is null)
        {
            HataMesaji("Lutfen giris yapiniz.");
            return RedirectToAction("Index", "Login");
        }

        var paketler = _jsonDataService.Listele<UyelikPaketi>(UyelikPaketleriDosyasi)
            .Where(p => p.AktifMi)
            .ToList();

        ViewData["UyeAd"] = uye.AdSoyad;
        ViewData["AktifPaket"] = uye.UyelikPaketiAdi;
        return View(paketler);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult PaketSec(int id)
    {
        var uye = AktifUyeGetir();
        if (uye is null)
        {
            HataMesaji("Lutfen giris yapiniz.");
            return RedirectToAction("Index", "Login");
        }

        var paketler = _jsonDataService.Listele<UyelikPaketi>(UyelikPaketleriDosyasi);
        var secilenPaket = paketler.FirstOrDefault(p => p.Id == id && p.AktifMi);
        if (secilenPaket is null)
        {
            HataMesaji("Secilen paket bulunamadi veya aktif degil.");
            return RedirectToAction(nameof(Paketler));
        }

        var uyeler = _jsonDataService.Listele<Uye>(UyelerDosyasi);
        var guncellenecekUye = uyeler.FirstOrDefault(u => u.Id == uye.Id);
        if (guncellenecekUye is not null)
        {
            guncellenecekUye.UyelikPaketiAdi = secilenPaket.Ad;
            _jsonDataService.Kaydet(UyelerDosyasi, uyeler);
            BasariMesaji($"Uyelik paketiniz basariyla '{secilenPaket.Ad}' olarak guncellendi.");
        }

        return RedirectToAction(nameof(Dashboard));
    }

    public IActionResult GrupDersleri()
    {
        var uye = AktifUyeGetir();
        if (uye is null)
        {
            HataMesaji("Lutfen giris yapiniz.");
            return RedirectToAction("Index", "Login");
        }

        var grupDersleri = _jsonDataService.Listele<GrupDersi>(GrupDersleriDosyasi)
            .Where(d => d.BaslangicZamani >= DateTime.Now)
            .OrderBy(d => d.BaslangicZamani)
            .ToList();

        var rezervasyonlar = _jsonDataService.Listele<GrupDersRezervasyon>(GrupDersRezervasyonlariDosyasi)
            .Where(r => r.UyeId == uye.Id)
            .Select(r => r.GrupDersiId)
            .ToList();

        var model = new UyeGrupDersleriViewModel
        {
            TumDersler = grupDersleri,
            KayitliDersIdleri = rezervasyonlar,
            Uye = uye
        };

        ViewData["UyeAd"] = uye.AdSoyad;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DersRezervasyonYap(int dersId)
    {
        var uye = AktifUyeGetir();
        if (uye is null)
        {
            HataMesaji("Lutfen giris yapiniz.");
            return RedirectToAction("Index", "Login");
        }

        var grupDersleri = _jsonDataService.Listele<GrupDersi>(GrupDersleriDosyasi);
        var ders = grupDersleri.FirstOrDefault(d => d.Id == dersId);

        if (ders is null || ders.IptalMi)
        {
            HataMesaji("Ders bulunamadi veya iptal edilmis.");
            return RedirectToAction(nameof(GrupDersleri));
        }

        if (ders.KatilimciSayisi >= ders.Kapasite)
        {
            HataMesaji("Ders kontenjani doludur.");
            return RedirectToAction(nameof(GrupDersleri));
        }

        var rezervasyonlar = _jsonDataService.Listele<GrupDersRezervasyon>(GrupDersRezervasyonlariDosyasi);
        if (rezervasyonlar.Any(r => r.UyeId == uye.Id && r.GrupDersiId == dersId))
        {
            HataMesaji("Bu derse zaten kayitlisiniz.");
            return RedirectToAction(nameof(GrupDersleri));
        }

        // Rezervasyon ekle
        int yeniId = rezervasyonlar.Any() ? rezervasyonlar.Max(r => r.Id) + 1 : 1;
        rezervasyonlar.Add(new GrupDersRezervasyon
        {
            Id = yeniId,
            UyeId = uye.Id,
            GrupDersiId = dersId,
            RezervasyonTarihi = DateTime.Now
        });
        _jsonDataService.Kaydet(GrupDersRezervasyonlariDosyasi, rezervasyonlar);

        // Katilimci sayisini artir
        ders.KatilimciSayisi++;
        _jsonDataService.Kaydet(GrupDersleriDosyasi, grupDersleri);

        // Bildirim olustur
        var bildirimler = _jsonDataService.Listele<Bildirim>(BildirimlerDosyasi);
        int yeniBildirimId = bildirimler.Any() ? bildirimler.Max(b => b.Id) + 1 : 1;
        bildirimler.Add(new Bildirim
        {
            Id = yeniBildirimId,
            Baslik = "Grup Dersi Rezervasyonu",
            Mesaj = $"'{ders.DersAdi}' dersine rezervasyonunuz basariyla yapildi. Baslangic: {ders.BaslangicZamani:dd.MM.yyyy HH:mm}",
            GonderimTarihi = DateTime.Now,
            OkunduMu = false,
            AliciUyeId = uye.Id
        });
        _jsonDataService.Kaydet(BildirimlerDosyasi, bildirimler);

        BasariMesaji($"'{ders.DersAdi}' dersine basariyla kayit oldunuz.");
        return RedirectToAction(nameof(GrupDersleri));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DersRezervasyonIptal(int dersId)
    {
        var uye = AktifUyeGetir();
        if (uye is null)
        {
            HataMesaji("Lutfen giris yapiniz.");
            return RedirectToAction("Index", "Login");
        }

        var rezervasyonlar = _jsonDataService.Listele<GrupDersRezervasyon>(GrupDersRezervasyonlariDosyasi);
        var rezervasyon = rezervasyonlar.FirstOrDefault(r => r.UyeId == uye.Id && r.GrupDersiId == dersId);

        if (rezervasyon is null)
        {
            HataMesaji("Rezervasyon bulunamadi.");
            return RedirectToAction(nameof(GrupDersleri));
        }

        var grupDersleri = _jsonDataService.Listele<GrupDersi>(GrupDersleriDosyasi);
        var ders = grupDersleri.FirstOrDefault(d => d.Id == dersId);

        if (ders is not null)
        {
            // Kontrol: En az 2 saat kalmis olmali
            if (ders.BaslangicZamani - DateTime.Now < TimeSpan.FromHours(2))
            {
                HataMesaji("Derse 2 saatten az kaldigi icin rezervasyonunuzu iptal edemezsiniz.");
                return RedirectToAction(nameof(GrupDersleri));
            }

            ders.KatilimciSayisi = Math.Max(0, ders.KatilimciSayisi - 1);
            _jsonDataService.Kaydet(GrupDersleriDosyasi, grupDersleri);
        }

        rezervasyonlar.Remove(rezervasyon);
        _jsonDataService.Kaydet(GrupDersRezervasyonlariDosyasi, rezervasyonlar);

        // Bildirim olustur
        var bildirimler = _jsonDataService.Listele<Bildirim>(BildirimlerDosyasi);
        int yeniBildirimId = bildirimler.Any() ? bildirimler.Max(b => b.Id) + 1 : 1;
        bildirimler.Add(new Bildirim
        {
            Id = yeniBildirimId,
            Baslik = "Rezervasyon Iptali",
            Mesaj = $"'{ders?.DersAdi}' dersi rezervasyonunuz iptal edildi.",
            GonderimTarihi = DateTime.Now,
            OkunduMu = false,
            AliciUyeId = uye.Id
        });
        _jsonDataService.Kaydet(BildirimlerDosyasi, bildirimler);

        BasariMesaji("Ders rezervasyonunuz iptal edildi.");
        return RedirectToAction(nameof(GrupDersleri));
    }

    public IActionResult OzelDersTalepleri()
    {
        var uye = AktifUyeGetir();
        if (uye is null)
        {
            HataMesaji("Lutfen giris yapiniz.");
            return RedirectToAction("Index", "Login");
        }

        var egitmenler = _jsonDataService.Listele<Egitmen>(EgitmenlerDosyasi)
            .Where(e => e.AktifMi)
            .ToList();

        var talepler = _jsonDataService.Listele<OzelDersTalebi>(OzelDersTalepleriDosyasi)
            .Where(t => t.UyeAdi.Equals(uye.AdSoyad, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(t => t.TalepTarihi)
            .ToList();

        var model = new UyeOzelDersViewModel
        {
            Egitmenler = egitmenler,
            Talepler = talepler,
            Uye = uye
        };

        ViewData["UyeAd"] = uye.AdSoyad;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult OzelDersTalepEt(string egitmenAdi, string dersAdi, DateTime istenenTarih, string not)
    {
        var uye = AktifUyeGetir();
        if (uye is null)
        {
            HataMesaji("Lutfen giris yapiniz.");
            return RedirectToAction("Index", "Login");
        }

        if (string.IsNullOrWhiteSpace(egitmenAdi) || string.IsNullOrWhiteSpace(dersAdi) || istenenTarih <= DateTime.Now)
        {
            HataMesaji("Lutfen gecerli bir egitmen, ders adi ve gelecek bir tarih seciniz.");
            return RedirectToAction(nameof(OzelDersTalepleri));
        }

        if (_jsonDataService.DersZamaniCakismasiVarMi(istenenTarih, 50, egitmenAdi))
        {
            HataMesaji("Seçilen tarih ve saatte eğitmenin başka bir dersi bulunmaktadır. Lütfen farklı bir saat seçiniz.");
            return RedirectToAction(nameof(OzelDersTalepleri));
        }

        var talepler = _jsonDataService.Listele<OzelDersTalebi>(OzelDersTalepleriDosyasi);
        int yeniId = talepler.Any() ? talepler.Max(t => t.Id) + 1 : 1;

        var yeniTalep = new OzelDersTalebi
        {
            Id = yeniId,
            UyeAdi = uye.AdSoyad,
            EgitmenAdi = egitmenAdi,
            DersAdi = dersAdi,
            TalepTarihi = DateTime.Now,
            IstenenTarih = istenenTarih,
            Durum = "Bekliyor",
            Not = not ?? string.Empty
        };

        talepler.Add(yeniTalep);
        _jsonDataService.Kaydet(OzelDersTalepleriDosyasi, talepler);

        // Admin bildirimi
        var bildirimler = _jsonDataService.Listele<Bildirim>(BildirimlerDosyasi);
        int yeniBildirimId = bildirimler.Any() ? bildirimler.Max(b => b.Id) + 1 : 1;
        bildirimler.Add(new Bildirim
        {
            Id = yeniBildirimId,
            Baslik = "Yeni Ozel Ders Talebi",
            Mesaj = $"{uye.AdSoyad}, {egitmenAdi} esliginde '{dersAdi}' ozel dersi talep etti. Tarih: {istenenTarih:dd.MM.yyyy HH:mm}",
            GonderimTarihi = DateTime.Now,
            OkunduMu = false,
            AliciUyeId = null // Admin'e
        });
        _jsonDataService.Kaydet(BildirimlerDosyasi, bildirimler);

        BasariMesaji("Ozel ders talebiniz basariyla alindi. Admin onayindan sonra kesinlesecektir.");
        return RedirectToAction(nameof(OzelDersTalepleri));
    }

    public IActionResult Bildirimler()
    {
        var uye = AktifUyeGetir();
        if (uye is null)
        {
            HataMesaji("Lutfen giris yapiniz.");
            return RedirectToAction("Index", "Login");
        }

        var bildirimler = _jsonDataService.Listele<Bildirim>(BildirimlerDosyasi);
        var uyeBildirimler = bildirimler
            .Where(b => b.AliciUyeId == uye.Id)
            .OrderByDescending(b => b.GonderimTarihi)
            .ToList();

        // Okundu olarak isaretle
        bool guncellendi = false;
        foreach (var b in uyeBildirimler)
        {
            if (!b.OkunduMu)
            {
                b.OkunduMu = true;
                guncellendi = true;
            }
        }

        if (guncellendi)
        {
            _jsonDataService.Kaydet(BildirimlerDosyasi, bildirimler);
        }

        ViewData["UyeAd"] = uye.AdSoyad;
        return View(uyeBildirimler);
    }

    public IActionResult Profil()
    {
        var uye = AktifUyeGetir();
        if (uye is null)
        {
            return RedirectToAction("Index", "Login");
        }

        var model = new UyeProfilViewModel
        {
            Id = uye.Id,
            AdSoyad = uye.AdSoyad,
            Eposta = uye.Eposta,
            Telefon = uye.Telefon
        };

        ViewData["UyeAd"] = uye.AdSoyad;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Profil(UyeProfilViewModel model)
    {
        var uye = AktifUyeGetir();
        if (uye is null)
        {
            return RedirectToAction("Index", "Login");
        }

        if (!ModelState.IsValid)
        {
            ViewData["UyeAd"] = uye.AdSoyad;
            return View(model);
        }

        var uyeler = _jsonDataService.Listele<Uye>(UyelerDosyasi);
        var guncellenecek = uyeler.FirstOrDefault(u => u.Id == uye.Id);

        if (guncellenecek is null)
        {
            HataMesaji("Uye bulunamadi.");
            return RedirectToAction(nameof(Dashboard));
        }

        if (uyeler.Any(u => u.Eposta.Equals(model.Eposta, StringComparison.OrdinalIgnoreCase) && u.Id != uye.Id))
        {
            HataMesaji("Bu e-posta adresi baska bir uye tarafindan kullaniliyor.");
            ViewData["UyeAd"] = uye.AdSoyad;
            return View(model);
        }

        string eskiAd = guncellenecek.AdSoyad;
        guncellenecek.AdSoyad = model.AdSoyad;
        guncellenecek.Eposta = model.Eposta;
        guncellenecek.Telefon = model.Telefon;

        if (!string.IsNullOrWhiteSpace(model.Sifre))
        {
            guncellenecek.Sifre = model.Sifre;
        }

        _jsonDataService.Kaydet(UyelerDosyasi, uyeler);

        // Ad değiştiyse çerezi ve özel ders taleplerindeki üye adını da güncelle!
        if (!eskiAd.Equals(model.AdSoyad, StringComparison.OrdinalIgnoreCase))
        {
            var talepler = _jsonDataService.Listele<OzelDersTalebi>(OzelDersTalepleriDosyasi);
            foreach (var talep in talepler.Where(t => t.UyeAdi.Equals(eskiAd, StringComparison.OrdinalIgnoreCase)))
            {
                talep.UyeAdi = model.AdSoyad;
            }
            _jsonDataService.Kaydet(OzelDersTalepleriDosyasi, talepler);

            Response.Cookies.Append("UyeAd", model.AdSoyad, new CookieOptions { HttpOnly = true, Expires = DateTimeOffset.UtcNow.AddDays(1) });
        }

        BasariMesaji("Profil bilgileriniz basariyla guncellendi.");
        return RedirectToAction(nameof(Profil));
    }

    public IActionResult Cikis()
    {
        Response.Cookies.Delete("UyeId");
        Response.Cookies.Delete("UyeAd");
        BasariMesaji("Basariyla cikis yapildi.");
        return RedirectToAction("Index", "Login");
    }
}
