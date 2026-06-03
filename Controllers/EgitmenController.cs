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

public class EgitmenController : BaseController
{
    private const string UyelerDosyasi = "uyeler.json";
    private const string EgitmenlerDosyasi = "egitmenler.json";
    private const string DerslerDosyasi = "dersler.json";
    private const string GrupDersleriDosyasi = "grupDersleri.json";
    private const string OzelDersTalepleriDosyasi = "ozelDersTalepleri.json";
    private const string BildirimlerDosyasi = "bildirimler.json";
    private const string GrupDersRezervasyonlariDosyasi = "grupDersRezervasyonlari.json";

    private readonly JsonDataService _jsonDataService;

    public EgitmenController(JsonDataService jsonDataService)
    {
        _jsonDataService = jsonDataService;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.HttpContext.Request.Cookies.ContainsKey("EgitmenId"))
        {
            TempData["HataMesaji"] = "Lütfen önce giriş yapınız.";
            context.Result = RedirectToAction("Index", "Login");
        }
        base.OnActionExecuting(context);
    }

    private Egitmen? AktifEgitmenGetir()
    {
        if (Request.Cookies.TryGetValue("EgitmenId", out var idStr) && int.TryParse(idStr, out var id))
        {
            return _jsonDataService.Listele<Egitmen>(EgitmenlerDosyasi).FirstOrDefault(e => e.Id == id && e.AktifMi);
        }
        return null;
    }

    public IActionResult Dashboard()
    {
        var egitmen = AktifEgitmenGetir();
        if (egitmen is null)
        {
            HataMesaji("Lütfen giriş yapınız.");
            return RedirectToAction("Index", "Login");
        }

        var grupDersleri = _jsonDataService.Listele<GrupDersi>(GrupDersleriDosyasi)
            .Where(d => d.EgitmenAdi.Equals(egitmen.AdSoyad, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var ozelDersler = _jsonDataService.Listele<OzelDersTalebi>(OzelDersTalepleriDosyasi)
            .Where(t => t.EgitmenAdi.Equals(egitmen.AdSoyad, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var bildirimler = _jsonDataService.Listele<Bildirim>(BildirimlerDosyasi)
            .Where(b => b.AliciEgitmenId == egitmen.Id)
            .OrderByDescending(b => b.GonderimTarihi)
            .Take(5)
            .ToList();

        var bugun = DateTime.Today;
        var bugunkuDersSayisi = grupDersleri.Count(d => d.BaslangicZamani.Date == bugun && !d.IptalMi) +
                               ozelDersler.Count(t => t.IstenenTarih.Date == bugun && t.Durum == "Onaylandi");

        var bekleyenOzelDersSayisi = ozelDersler.Count(t => t.Durum == "Bekliyor");

        var toplamKatilimciSayisi = grupDersleri.Where(d => d.BaslangicZamani >= DateTime.Now && !d.IptalMi).Sum(d => d.KatilimciSayisi);

        var okunmamisSayi = _jsonDataService.Listele<Bildirim>(BildirimlerDosyasi)
            .Count(b => b.AliciEgitmenId == egitmen.Id && !b.OkunduMu);

        var model = new EgitmenDashboardViewModel
        {
            Egitmen = egitmen,
            GelecekDersler = grupDersleri.Where(d => d.BaslangicZamani >= DateTime.Now && !d.IptalMi).OrderBy(d => d.BaslangicZamani).Take(5).ToList(),
            GelecekOzelDersler = ozelDersler.Where(t => t.IstenenTarih >= DateTime.Now && t.Durum == "Onaylandi").OrderBy(t => t.IstenenTarih).Take(5).ToList(),
            Bildirimler = bildirimler,
            BugunkuDersSayisi = bugunkuDersSayisi,
            BekleyenOzelDersSayisi = bekleyenOzelDersSayisi,
            ToplamKatilimciSayisi = toplamKatilimciSayisi,
            OkunmamisBildirimSayisi = okunmamisSayi
        };

        ViewData["EgitmenAd"] = egitmen.AdSoyad;
        return View(model);
    }

    public IActionResult GrupDersleri()
    {
        var egitmen = AktifEgitmenGetir();
        if (egitmen is null)
        {
            HataMesaji("Lütfen giriş yapınız.");
            return RedirectToAction("Index", "Login");
        }

        var dersler = _jsonDataService.Listele<GrupDersi>(GrupDersleriDosyasi)
            .Where(d => d.EgitmenAdi.Equals(egitmen.AdSoyad, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(d => d.BaslangicZamani)
            .ToList();

        var rezervasyonlar = _jsonDataService.Listele<GrupDersRezervasyon>(GrupDersRezervasyonlariDosyasi);
        var uyeler = _jsonDataService.Listele<Uye>(UyelerDosyasi);

        var model = new EgitmenGrupDersleriViewModel
        {
            Dersler = dersler,
            Rezervasyonlar = rezervasyonlar,
            Uyeler = uyeler
        };

        ViewData["EgitmenAd"] = egitmen.AdSoyad;
        return View(model);
    }

    public IActionResult OzelDersleri()
    {
        var egitmen = AktifEgitmenGetir();
        if (egitmen is null)
        {
            HataMesaji("Lütfen giriş yapınız.");
            return RedirectToAction("Index", "Login");
        }

        var talepler = _jsonDataService.Listele<OzelDersTalebi>(OzelDersTalepleriDosyasi)
            .Where(t => t.EgitmenAdi.Equals(egitmen.AdSoyad, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(t => t.TalepTarihi)
            .ToList();

        ViewData["EgitmenAd"] = egitmen.AdSoyad;
        return View(talepler);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult OzelDersTalepOnayla(int id)
    {
        var egitmen = AktifEgitmenGetir();
        if (egitmen is null)
        {
            HataMesaji("Lütfen giriş yapınız.");
            return RedirectToAction("Index", "Login");
        }

        var talepler = _jsonDataService.Listele<OzelDersTalebi>(OzelDersTalepleriDosyasi);
        var talep = talepler.FirstOrDefault(t => t.Id == id && t.EgitmenAdi.Equals(egitmen.AdSoyad, StringComparison.OrdinalIgnoreCase));

        if (talep is null)
        {
            HataMesaji("Talep bulunamadı.");
            return RedirectToAction(nameof(OzelDersleri));
        }

        if (_jsonDataService.DersZamaniCakismasiVarMi(talep.IstenenTarih, 50, egitmen.AdSoyad, talep.Id))
        {
            HataMesaji("Çakışma Hatası: Belirtilen tarihte eğitmenin başka bir dersi bulunmaktadır!");
            return RedirectToAction(nameof(OzelDersleri));
        }

        talep.Durum = "Onaylandi";
        _jsonDataService.Kaydet(OzelDersTalepleriDosyasi, talepler);

        // Üyeye bildirim gönder
        var uyeler = _jsonDataService.Listele<Uye>(UyelerDosyasi);
        var uye = uyeler.FirstOrDefault(u => u.AdSoyad.Equals(talep.UyeAdi, StringComparison.OrdinalIgnoreCase));
        
        var bildirimler = _jsonDataService.Listele<Bildirim>(BildirimlerDosyasi);
        int yeniBildirimId = bildirimler.Any() ? bildirimler.Max(b => b.Id) + 1 : 1;
        bildirimler.Add(new Bildirim
        {
            Id = yeniBildirimId,
            Baslik = "Özel Ders Talebi Onaylandı",
            Mesaj = $"Eğitmeniniz {egitmen.AdSoyad}, '{talep.DersAdi}' özel ders talebinizi onayladı. Tarih: {talep.IstenenTarih:dd.MM.yyyy HH:mm}",
            GonderimTarihi = DateTime.Now,
            OkunduMu = false,
            AliciUyeId = uye?.Id
        });
        _jsonDataService.Kaydet(BildirimlerDosyasi, bildirimler);

        BasariMesaji("Özel ders talebi onaylandı.");
        return RedirectToAction(nameof(OzelDersleri));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult OzelDersTalepReddet(int id)
    {
        var egitmen = AktifEgitmenGetir();
        if (egitmen is null)
        {
            HataMesaji("Lütfen giriş yapınız.");
            return RedirectToAction("Index", "Login");
        }

        var talepler = _jsonDataService.Listele<OzelDersTalebi>(OzelDersTalepleriDosyasi);
        var talep = talepler.FirstOrDefault(t => t.Id == id && t.EgitmenAdi.Equals(egitmen.AdSoyad, StringComparison.OrdinalIgnoreCase));

        if (talep is null)
        {
            HataMesaji("Talep bulunamadı.");
            return RedirectToAction(nameof(OzelDersleri));
        }

        talep.Durum = "Reddedildi";
        _jsonDataService.Kaydet(OzelDersTalepleriDosyasi, talepler);

        // Üyeye bildirim gönder
        var uyeler = _jsonDataService.Listele<Uye>(UyelerDosyasi);
        var uye = uyeler.FirstOrDefault(u => u.AdSoyad.Equals(talep.UyeAdi, StringComparison.OrdinalIgnoreCase));

        var bildirimler = _jsonDataService.Listele<Bildirim>(BildirimlerDosyasi);
        int yeniBildirimId = bildirimler.Any() ? bildirimler.Max(b => b.Id) + 1 : 1;
        bildirimler.Add(new Bildirim
        {
            Id = yeniBildirimId,
            Baslik = "Özel Ders Talebi Reddedildi",
            Mesaj = $"Eğitmeniniz {egitmen.AdSoyad}, '{talep.DersAdi}' özel ders talebinizi reddetti.",
            GonderimTarihi = DateTime.Now,
            OkunduMu = false,
            AliciUyeId = uye?.Id
        });
        _jsonDataService.Kaydet(BildirimlerDosyasi, bildirimler);

        BasariMesaji("Özel ders talebi reddedildi.");
        return RedirectToAction(nameof(OzelDersleri));
    }

    public IActionResult Profil()
    {
        var egitmen = AktifEgitmenGetir();
        if (egitmen is null)
        {
            HataMesaji("Lütfen giriş yapınız.");
            return RedirectToAction("Index", "Login");
        }

        var model = new EgitmenProfilViewModel
        {
            Id = egitmen.Id,
            AdSoyad = egitmen.AdSoyad,
            UzmanlikAlani = egitmen.UzmanlikAlani,
            Eposta = egitmen.Eposta,
            Telefon = egitmen.Telefon
        };

        ViewData["EgitmenAd"] = egitmen.AdSoyad;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Profil(EgitmenProfilViewModel model)
    {
        var egitmen = AktifEgitmenGetir();
        if (egitmen is null)
        {
            HataMesaji("Lütfen giriş yapınız.");
            return RedirectToAction("Index", "Login");
        }

        if (!ModelState.IsValid)
        {
            ViewData["EgitmenAd"] = egitmen.AdSoyad;
            return View(model);
        }

        var egitmenler = _jsonDataService.Listele<Egitmen>(EgitmenlerDosyasi);
        var guncellenecek = egitmenler.FirstOrDefault(e => e.Id == egitmen.Id);

        if (guncellenecek is null)
        {
            HataMesaji("Eğitmen bulunamadı.");
            return RedirectToAction(nameof(Dashboard));
        }

        // Eposta benzersizlik kontrolü
        if (egitmenler.Any(e => e.Eposta.Equals(model.Eposta, StringComparison.OrdinalIgnoreCase) && e.Id != egitmen.Id))
        {
            HataMesaji("Bu e-posta adresi başka bir eğitmen tarafından kullanılıyor.");
            ViewData["EgitmenAd"] = egitmen.AdSoyad;
            return View(model);
        }

        // Güncellemeler (Eğitmen adı değişirse, grup derslerindeki EgitmenAdi'ları da güncellemeliyiz!)
        string eskiAd = guncellenecek.AdSoyad;
        guncellenecek.AdSoyad = model.AdSoyad;
        guncellenecek.UzmanlikAlani = model.UzmanlikAlani;
        guncellenecek.Eposta = model.Eposta;
        guncellenecek.Telefon = model.Telefon;

        if (!string.IsNullOrWhiteSpace(model.Sifre))
        {
            guncellenecek.Sifre = model.Sifre;
        }

        _jsonDataService.Kaydet(EgitmenlerDosyasi, egitmenler);

        // Ad değiştiyse atanan grup derslerini ve özel dersleri de güncelle
        if (!eskiAd.Equals(model.AdSoyad, StringComparison.OrdinalIgnoreCase))
        {
            var grupDersleri = _jsonDataService.Listele<GrupDersi>(GrupDersleriDosyasi);
            foreach (var gd in grupDersleri.Where(d => d.EgitmenAdi.Equals(eskiAd, StringComparison.OrdinalIgnoreCase)))
            {
                gd.EgitmenAdi = model.AdSoyad;
            }
            _jsonDataService.Kaydet(GrupDersleriDosyasi, grupDersleri);

            var ozelDersler = _jsonDataService.Listele<OzelDersTalebi>(OzelDersTalepleriDosyasi);
            foreach (var od in ozelDersler.Where(t => t.EgitmenAdi.Equals(eskiAd, StringComparison.OrdinalIgnoreCase)))
            {
                od.EgitmenAdi = model.AdSoyad;
            }
            _jsonDataService.Kaydet(OzelDersTalepleriDosyasi, ozelDersler);

            var dersler = _jsonDataService.Listele<Ders>(DerslerDosyasi);
            foreach (var d in dersler.Where(x => x.EgitmenAdi.Equals(eskiAd, StringComparison.OrdinalIgnoreCase)))
            {
                d.EgitmenAdi = model.AdSoyad;
            }
            _jsonDataService.Kaydet(DerslerDosyasi, dersler);

            // Çerezi de güncelle
            Response.Cookies.Append("EgitmenAd", model.AdSoyad, new CookieOptions { HttpOnly = true, Expires = DateTimeOffset.UtcNow.AddDays(1) });
        }

        BasariMesaji("Profil bilgileriniz başarıyla güncellendi.");
        return RedirectToAction(nameof(Profil));
    }

    public IActionResult Bildirimler()
    {
        var egitmen = AktifEgitmenGetir();
        if (egitmen is null)
        {
            HataMesaji("Lütfen giriş yapınız.");
            return RedirectToAction("Index", "Login");
        }

        var bildirimler = _jsonDataService.Listele<Bildirim>(BildirimlerDosyasi);
        var egitmenBildirimler = bildirimler
            .Where(b => b.AliciEgitmenId == egitmen.Id)
            .OrderByDescending(b => b.GonderimTarihi)
            .ToList();

        // Okundu olarak işaretle
        bool guncellendi = false;
        foreach (var b in egitmenBildirimler)
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

        ViewData["EgitmenAd"] = egitmen.AdSoyad;
        return View(egitmenBildirimler);
    }

    public IActionResult Cikis()
    {
        Response.Cookies.Delete("EgitmenId");
        Response.Cookies.Delete("EgitmenAd");
        BasariMesaji("Başarıyla çıkış yapıldı.");
        return RedirectToAction("Index", "Login");
    }
}
