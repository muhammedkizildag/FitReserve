using FitReserve.Helpers;
using FitReserve.Models;
using FitReserve.Services;
using FitReserve.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace FitReserve.Controllers;

public class LoginController : BaseController
{
    private const string UyelerDosyasi = "uyeler.json";
    private const string EgitmenlerDosyasi = "egitmenler.json";
    private const string UyelikPaketleriDosyasi = "uyelikPaketleri.json";
    private const string EgitmenKayitTalepleriDosyasi = "egitmenKayitTalepleri.json";
    private const string BildirimlerDosyasi = "bildirimler.json";
    private const string AdminDosyasi = "admin.json";

    private readonly JsonDataService _jsonDataService;

    public LoginController(JsonDataService jsonDataService)
    {
        _jsonDataService = jsonDataService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View(LoginSayfasiHazirla());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult GirisYap(LoginSayfasiViewModel model)
    {
        ModelState.Clear();
        TryValidateModel(model.Giris, nameof(model.Giris));

        if (!ModelState.IsValid)
        {
            return View("Index", LoginSayfasiHazirla(model));
        }

        if (model.Giris.Rol == Roller.Admin)
        {
            var adminVarMi = _jsonDataService.Listele<AdminKullanici>(AdminDosyasi)
                .Any(admin => admin.AktifMi && admin.Eposta == model.Giris.Eposta && admin.Sifre == model.Giris.Sifre);

            if (!adminVarMi)
            {
                HataMesaji("Admin bilgileri hatali.");
                return RedirectToAction(nameof(Index));
            }

            Response.Cookies.Append("AdminEposta", model.Giris.Eposta, new CookieOptions { HttpOnly = true, Expires = DateTimeOffset.UtcNow.AddDays(1) });

            BasariMesaji("Admin girişi başarılı.");
            return RedirectToAction("Dashboard", "Admin");
        }

        if (model.Giris.Rol == Roller.Uye)
        {
            var uye = _jsonDataService.Listele<Uye>(UyelerDosyasi)
                .FirstOrDefault(u => u.AktifMi && u.Eposta == model.Giris.Eposta && u.Sifre == model.Giris.Sifre);

            if (uye is null)
            {
                HataMesaji("Uye bilgileri hatali.");
                return RedirectToAction(nameof(Index));
            }

            Response.Cookies.Append("UyeId", uye.Id.ToString(), new CookieOptions { HttpOnly = true, Expires = DateTimeOffset.UtcNow.AddDays(1) });
            Response.Cookies.Append("UyeAd", uye.AdSoyad, new CookieOptions { HttpOnly = true, Expires = DateTimeOffset.UtcNow.AddDays(1) });

            BasariMesaji("Uye girisi basarili.");
            return RedirectToAction("Dashboard", "Uye");
        }

        if (model.Giris.Rol == Roller.Egitmen)
        {
            var egitmen = _jsonDataService.Listele<Egitmen>(EgitmenlerDosyasi)
                .FirstOrDefault(e => e.AktifMi && e.Eposta == model.Giris.Eposta && e.Sifre == model.Giris.Sifre);

            if (egitmen is null)
            {
                HataMesaji("Egitmen bilgileri hatali.");
                return RedirectToAction(nameof(Index));
            }

            Response.Cookies.Append("EgitmenId", egitmen.Id.ToString(), new CookieOptions { HttpOnly = true, Expires = DateTimeOffset.UtcNow.AddDays(1) });
            Response.Cookies.Append("EgitmenAd", egitmen.AdSoyad, new CookieOptions { HttpOnly = true, Expires = DateTimeOffset.UtcNow.AddDays(1) });

            BasariMesaji("Egitmen girisi basarili.");
            return RedirectToAction("Dashboard", "Egitmen");
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UyeKayitOl(LoginSayfasiViewModel model)
    {
        ModelState.Clear();
        TryValidateModel(model.UyeKayit, nameof(model.UyeKayit));

        if (!ModelState.IsValid)
        {
            return View("Index", LoginSayfasiHazirla(model));
        }

        var uyeler = _jsonDataService.Listele<Uye>(UyelerDosyasi);
        if (uyeler.Any(uye => uye.Eposta == model.UyeKayit.Eposta))
        {
            HataMesaji("Bu eposta ile kayitli bir uye zaten var.");
            return RedirectToAction(nameof(Index));
        }

        uyeler.Add(new Uye
        {
            Id = YeniIdOlustur(uyeler.Select(uye => uye.Id)),
            AdSoyad = model.UyeKayit.AdSoyad,
            Eposta = model.UyeKayit.Eposta,
            Sifre = model.UyeKayit.Sifre,
            Telefon = model.UyeKayit.Telefon,
            UyelikPaketiAdi = model.UyeKayit.UyelikPaketiAdi,
            KayitTarihi = DateTime.Now,
            AktifMi = true
        });

        _jsonDataService.Kaydet(UyelerDosyasi, uyeler);
        BasariMesaji("Uye kaydi olusturuldu.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult EgitmenKayitOl(LoginSayfasiViewModel model)
    {
        ModelState.Clear();
        TryValidateModel(model.EgitmenKayit, nameof(model.EgitmenKayit));

        if (!ModelState.IsValid)
        {
            return View("Index", LoginSayfasiHazirla(model));
        }

        var egitmenler = _jsonDataService.Listele<Egitmen>(EgitmenlerDosyasi);
        var talepler = _jsonDataService.Listele<EgitmenKayitTalebi>(EgitmenKayitTalepleriDosyasi);

        if (egitmenler.Any(egitmen => egitmen.Eposta == model.EgitmenKayit.Eposta) ||
            talepler.Any(talep => talep.Eposta == model.EgitmenKayit.Eposta && talep.Durum == "Bekliyor"))
        {
            HataMesaji("Bu eposta ile aktif veya bekleyen bir egitmen kaydi var.");
            return RedirectToAction(nameof(Index));
        }

        talepler.Add(new EgitmenKayitTalebi
        {
            Id = YeniIdOlustur(talepler.Select(talep => talep.Id)),
            AdSoyad = model.EgitmenKayit.AdSoyad,
            UzmanlikAlani = model.EgitmenKayit.UzmanlikAlani,
            Eposta = model.EgitmenKayit.Eposta,
            Sifre = model.EgitmenKayit.Sifre,
            Telefon = model.EgitmenKayit.Telefon,
            TalepTarihi = DateTime.Now,
            Durum = "Bekliyor"
        });

        var bildirimler = _jsonDataService.Listele<Bildirim>(BildirimlerDosyasi);
        bildirimler.Add(new Bildirim
        {
            Id = YeniIdOlustur(bildirimler.Select(bildirim => bildirim.Id)),
            Baslik = "Yeni egitmen kayit talebi",
            Mesaj = $"{model.EgitmenKayit.AdSoyad} egitmen olarak kayit olmak istiyor.",
            GonderimTarihi = DateTime.Now,
            OkunduMu = false
        });

        _jsonDataService.Kaydet(EgitmenKayitTalepleriDosyasi, talepler);
        _jsonDataService.Kaydet(BildirimlerDosyasi, bildirimler);
        BasariMesaji("Egitmen kayit talebiniz admin onayina gonderildi.");
        return RedirectToAction(nameof(Index));
    }

    private LoginSayfasiViewModel LoginSayfasiHazirla(LoginSayfasiViewModel? model = null)
    {
        model ??= new LoginSayfasiViewModel();
        model.UyelikPaketleri = _jsonDataService.Listele<UyelikPaketi>(UyelikPaketleriDosyasi)
            .Where(paket => paket.AktifMi)
            .ToList();

        return model;
    }

    private static int YeniIdOlustur(IEnumerable<int> idler)
    {
        return idler.Any() ? idler.Max() + 1 : 1;
    }
}
