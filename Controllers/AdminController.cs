using FitReserve.Models;
using FitReserve.Services;
using FitReserve.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FitReserve.Controllers;

public class AdminController : BaseController
{
    private const string UyelerDosyasi = "uyeler.json";
    private const string EgitmenlerDosyasi = "egitmenler.json";
    private const string UyelikPaketleriDosyasi = "uyelikPaketleri.json";
    private const string DerslerDosyasi = "dersler.json";
    private const string GrupDersleriDosyasi = "grupDersleri.json";
    private const string OzelDersTalepleriDosyasi = "ozelDersTalepleri.json";
    private const string EgitmenKayitTalepleriDosyasi = "egitmenKayitTalepleri.json";
    private const string BildirimlerDosyasi = "bildirimler.json";

    private readonly JsonDataService _jsonDataService;

    public AdminController(JsonDataService jsonDataService)
    {
        _jsonDataService = jsonDataService;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        // Login action'ı hariç tutulmalıdır ama AdminController'da login action'ı yok, hepsi yetkili olmalı!
        if (!context.HttpContext.Request.Cookies.ContainsKey("AdminEposta"))
        {
            TempData["HataMesaji"] = "Lütfen önce giriş yapınız.";
            context.Result = RedirectToAction("Index", "Login");
        }
        base.OnActionExecuting(context);
    }

    public IActionResult Cikis()
    {
        Response.Cookies.Delete("AdminEposta");
        BasariMesaji("Başarıyla çıkış yapıldı.");
        return RedirectToAction("Index", "Login");
    }

    public IActionResult Dashboard()
    {
        var uyeler = _jsonDataService.Listele<Uye>(UyelerDosyasi);
        var egitmenler = _jsonDataService.Listele<Egitmen>(EgitmenlerDosyasi);
        var uyelikPaketleri = _jsonDataService.Listele<UyelikPaketi>(UyelikPaketleriDosyasi);
        var grupDersleri = _jsonDataService.Listele<GrupDersi>(GrupDersleriDosyasi);
        var ozelDersTalepleri = _jsonDataService.Listele<OzelDersTalebi>(OzelDersTalepleriDosyasi);
        var bildirimler = _jsonDataService.Listele<Bildirim>(BildirimlerDosyasi);

        var model = new AdminDashboardViewModel
        {
            ToplamUyeSayisi = uyeler.Count,
            AktifUyeSayisi = uyeler.Count(uye => uye.AktifMi),
            AktifEgitmenSayisi = egitmenler.Count(egitmen => egitmen.AktifMi),
            AktifUyelikPaketiSayisi = uyelikPaketleri.Count(paket => paket.AktifMi),
            BugunkuGrupDersiSayisi = grupDersleri.Count(ders => !ders.IptalMi && ders.BaslangicZamani.Date == DateTime.Today),
            BekleyenOzelDersTalebiSayisi = ozelDersTalepleri.Count(talep => talep.Durum == "Bekliyor"),
            OkunmamisBildirimSayisi = bildirimler.Count(bildirim => !bildirim.OkunduMu)
        };

        return View(model);
    }

    public IActionResult Uyeler()
    {
        return View(_jsonDataService.Listele<Uye>(UyelerDosyasi));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UyeligiBitir(int id)
    {
        var uyeler = _jsonDataService.Listele<Uye>(UyelerDosyasi);
        var uye = uyeler.FirstOrDefault(kayit => kayit.Id == id);

        if (uye is null)
        {
            return NotFound();
        }

        uyeler.Remove(uye);
        _jsonDataService.Kaydet(UyelerDosyasi, uyeler);
        BasariMesaji("Uyelik sonlandirildi.");
        return RedirectToAction(nameof(Uyeler));
    }

    public IActionResult Egitmenler()
    {
        var model = new AdminEgitmenlerViewModel
        {
            BekleyenKayitTalepleri = _jsonDataService.Listele<EgitmenKayitTalebi>(EgitmenKayitTalepleriDosyasi)
                .Where(talep => talep.Durum == "Bekliyor")
                .OrderBy(talep => talep.TalepTarihi)
                .ToList(),
            Egitmenler = _jsonDataService.Listele<Egitmen>(EgitmenlerDosyasi)
                .OrderBy(egitmen => egitmen.AdSoyad)
                .ToList()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult EgitmenligiBitir(int id)
    {
        var egitmenler = _jsonDataService.Listele<Egitmen>(EgitmenlerDosyasi);
        var egitmen = egitmenler.FirstOrDefault(kayit => kayit.Id == id);

        if (egitmen is null)
        {
            return NotFound();
        }

        egitmenler.Remove(egitmen);
        _jsonDataService.Kaydet(EgitmenlerDosyasi, egitmenler);
        BasariMesaji("Egitmenlik sonlandirildi.");
        return RedirectToAction(nameof(Egitmenler));
    }

    [HttpGet]
    public IActionResult EgitmenGuncelle(int id)
    {
        var egitmen = _jsonDataService.Listele<Egitmen>(EgitmenlerDosyasi).FirstOrDefault(kayit => kayit.Id == id);
        if (egitmen is null)
        {
            return NotFound();
        }

        return View(new EgitmenFormViewModel
        {
            Id = egitmen.Id,
            AdSoyad = egitmen.AdSoyad,
            UzmanlikAlani = egitmen.UzmanlikAlani,
            Eposta = egitmen.Eposta,
            Telefon = egitmen.Telefon
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult EgitmenGuncelle(EgitmenFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var egitmenler = _jsonDataService.Listele<Egitmen>(EgitmenlerDosyasi);
        var egitmen = egitmenler.FirstOrDefault(kayit => kayit.Id == model.Id);
        if (egitmen is null)
        {
            return NotFound();
        }

        egitmen.AdSoyad = model.AdSoyad;
        egitmen.UzmanlikAlani = model.UzmanlikAlani;
        egitmen.Eposta = model.Eposta;
        egitmen.Telefon = model.Telefon;

        _jsonDataService.Kaydet(EgitmenlerDosyasi, egitmenler);
        BasariMesaji("Egitmen guncellendi.");
        return RedirectToAction(nameof(Egitmenler));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult EgitmenKayitTalebiOnayla(int id)
    {
        var talepler = _jsonDataService.Listele<EgitmenKayitTalebi>(EgitmenKayitTalepleriDosyasi);
        var talep = talepler.FirstOrDefault(kayit => kayit.Id == id);

        if (talep is null)
        {
            return NotFound();
        }

        var egitmenler = _jsonDataService.Listele<Egitmen>(EgitmenlerDosyasi);
        if (!egitmenler.Any(egitmen => egitmen.Eposta == talep.Eposta))
        {
            egitmenler.Add(new Egitmen
            {
                Id = YeniIdOlustur(egitmenler.Select(egitmen => egitmen.Id)),
                AdSoyad = talep.AdSoyad,
                UzmanlikAlani = talep.UzmanlikAlani,
                Eposta = talep.Eposta,
                Sifre = talep.Sifre,
                Telefon = talep.Telefon,
                AktifMi = true
            });
        }

        talepler.Remove(talep);
        _jsonDataService.Kaydet(EgitmenlerDosyasi, egitmenler);
        _jsonDataService.Kaydet(EgitmenKayitTalepleriDosyasi, talepler);
        BasariMesaji("Egitmen kayit talebi onaylandi.");
        return RedirectToAction(nameof(Egitmenler));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult EgitmenKayitTalebiReddet(int id)
    {
        var talepler = _jsonDataService.Listele<EgitmenKayitTalebi>(EgitmenKayitTalepleriDosyasi);
        var talep = talepler.FirstOrDefault(kayit => kayit.Id == id);

        if (talep is null)
        {
            return NotFound();
        }

        talepler.Remove(talep);
        _jsonDataService.Kaydet(EgitmenKayitTalepleriDosyasi, talepler);
        BasariMesaji("Egitmen kayit talebi reddedildi.");
        return RedirectToAction(nameof(Egitmenler));
    }

    public IActionResult UyelikPaketleri()
    {
        return View(_jsonDataService.Listele<UyelikPaketi>(UyelikPaketleriDosyasi));
    }

    [HttpGet]
    public IActionResult UyelikPaketiEkle()
    {
        return View(new UyelikPaketiFormViewModel { SureGun = 30 });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UyelikPaketiEkle(UyelikPaketiFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var paketler = _jsonDataService.Listele<UyelikPaketi>(UyelikPaketleriDosyasi);
        paketler.Add(new UyelikPaketi
        {
            Id = YeniIdOlustur(paketler.Select(paket => paket.Id)),
            Ad = model.Ad,
            Aciklama = model.Aciklama,
            SureGun = model.SureGun,
            Fiyat = model.Fiyat,
            AktifMi = true
        });

        _jsonDataService.Kaydet(UyelikPaketleriDosyasi, paketler);
        BasariMesaji("Uyelik paketi eklendi.");
        return RedirectToAction(nameof(UyelikPaketleri));
    }

    [HttpGet]
    public IActionResult UyelikPaketiGuncelle(int id)
    {
        var paket = _jsonDataService.Listele<UyelikPaketi>(UyelikPaketleriDosyasi).FirstOrDefault(kayit => kayit.Id == id);
        if (paket is null)
        {
            return NotFound();
        }

        return View(new UyelikPaketiFormViewModel
        {
            Id = paket.Id,
            Ad = paket.Ad,
            Aciklama = paket.Aciklama,
            SureGun = paket.SureGun,
            Fiyat = paket.Fiyat
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UyelikPaketiGuncelle(UyelikPaketiFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var paketler = _jsonDataService.Listele<UyelikPaketi>(UyelikPaketleriDosyasi);
        var paket = paketler.FirstOrDefault(kayit => kayit.Id == model.Id);
        if (paket is null)
        {
            return NotFound();
        }

        paket.Ad = model.Ad;
        paket.Aciklama = model.Aciklama;
        paket.SureGun = model.SureGun;
        paket.Fiyat = model.Fiyat;

        _jsonDataService.Kaydet(UyelikPaketleriDosyasi, paketler);
        BasariMesaji("Uyelik paketi guncellendi.");
        return RedirectToAction(nameof(UyelikPaketleri));
    }

    public IActionResult Dersler()
    {
        var bugun = DateTime.Today;
        var haftaBaslangici = bugun.AddDays(-(int)(bugun.DayOfWeek == DayOfWeek.Sunday ? 6 : bugun.DayOfWeek - DayOfWeek.Monday));
        var haftaBitisi = haftaBaslangici.AddDays(7);

        var dersler = _jsonDataService.Listele<Ders>(DerslerDosyasi);
        var grupDersleri = _jsonDataService.Listele<GrupDersi>(GrupDersleriDosyasi);
        var ozelDersTalepleri = _jsonDataService.Listele<OzelDersTalebi>(OzelDersTalepleriDosyasi);

        var model = new AdminDerslerViewModel
        {
            GrupDersleri = dersler
                .Where(ders => ders.AktifMi && ders.Kategori == "Grup Dersi")
                .OrderBy(ders => ders.Ad)
                .ToList(),
            HaftalikGrupDersleri = grupDersleri
                .Where(ders => !ders.IptalMi && ders.BaslangicZamani >= haftaBaslangici && ders.BaslangicZamani < haftaBitisi)
                .OrderBy(ders => ders.BaslangicZamani)
                .ToList(),
            HaftalikOzelDersler = ozelDersTalepleri
                .Where(talep => talep.Durum == "Onaylandi" && talep.IstenenTarih >= haftaBaslangici && talep.IstenenTarih < haftaBitisi)
                .OrderBy(talep => talep.IstenenTarih)
                .ToList(),
            HaftaBaslangici = haftaBaslangici,
            HaftaBitisi = haftaBitisi.AddDays(-1)
        };

        return View(model);
    }

    [HttpGet]
    public IActionResult DersAc(int id)
    {
        var ders = _jsonDataService.Listele<Ders>(DerslerDosyasi)
            .FirstOrDefault(kayit => kayit.Id == id && kayit.AktifMi && kayit.Kategori == "Grup Dersi");

        if (ders is null)
        {
            return NotFound();
        }

        return View(new DersAcFormViewModel
        {
            DersId = ders.Id,
            DersAdi = ders.Ad,
            EgitmenAdi = ders.EgitmenAdi,
            BaslangicZamani = DateTime.Today.AddDays(1).AddHours(18),
            Kapasite = 12
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DersAc(DersAcFormViewModel model)
    {
        var ders = _jsonDataService.Listele<Ders>(DerslerDosyasi)
            .FirstOrDefault(kayit => kayit.Id == model.DersId && kayit.AktifMi && kayit.Kategori == "Grup Dersi");

        if (ders is null)
        {
            return NotFound();
        }

        model.DersAdi = ders.Ad;
        model.EgitmenAdi = ders.EgitmenAdi;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var grupDersleri = _jsonDataService.Listele<GrupDersi>(GrupDersleriDosyasi);
        grupDersleri.Add(new GrupDersi
        {
            Id = YeniIdOlustur(grupDersleri.Select(grupDersi => grupDersi.Id)),
            DersAdi = ders.Ad,
            EgitmenAdi = ders.EgitmenAdi,
            BaslangicZamani = model.BaslangicZamani,
            SureDakika = ders.SureDakika,
            Kapasite = model.Kapasite,
            KatilimciSayisi = 0,
            IptalMi = false
        });

        _jsonDataService.Kaydet(GrupDersleriDosyasi, grupDersleri);
        BasariMesaji("Ders aktif olarak acildi.");
        return RedirectToAction(nameof(Dersler));
    }

    [HttpGet]
    public IActionResult GrupDersiGuncelle(int id)
    {
        var grupDersi = _jsonDataService.Listele<GrupDersi>(GrupDersleriDosyasi)
            .FirstOrDefault(kayit => kayit.Id == id);

        if (grupDersi is null)
        {
            return NotFound();
        }

        return View(new GrupDersiGuncelleFormViewModel
        {
            Id = grupDersi.Id,
            DersAdi = grupDersi.DersAdi,
            EgitmenAdi = grupDersi.EgitmenAdi,
            BaslangicZamani = grupDersi.BaslangicZamani,
            Kapasite = grupDersi.Kapasite
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult GrupDersiGuncelle(GrupDersiGuncelleFormViewModel model)
    {
        var grupDersleri = _jsonDataService.Listele<GrupDersi>(GrupDersleriDosyasi);
        var grupDersi = grupDersleri.FirstOrDefault(kayit => kayit.Id == model.Id);

        if (grupDersi is null)
        {
            return NotFound();
        }

        model.DersAdi = grupDersi.DersAdi;
        model.EgitmenAdi = grupDersi.EgitmenAdi;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        grupDersi.BaslangicZamani = model.BaslangicZamani;
        grupDersi.Kapasite = model.Kapasite;

        _jsonDataService.Kaydet(GrupDersleriDosyasi, grupDersleri);
        BasariMesaji("Grup dersi guncellendi.");
        return RedirectToAction(nameof(Dersler));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult GrupDersiIptalEt(int id)
    {
        var grupDersleri = _jsonDataService.Listele<GrupDersi>(GrupDersleriDosyasi);
        var grupDersi = grupDersleri.FirstOrDefault(kayit => kayit.Id == id);

        if (grupDersi is null)
        {
            return NotFound();
        }

        grupDersi.IptalMi = true;
        _jsonDataService.Kaydet(GrupDersleriDosyasi, grupDersleri);
        BasariMesaji("Grup dersi iptal edildi.");
        return RedirectToAction(nameof(Dersler));
    }

    [HttpGet]
    public IActionResult OzelDersGuncelle(int id)
    {
        var talep = _jsonDataService.Listele<OzelDersTalebi>(OzelDersTalepleriDosyasi)
            .FirstOrDefault(kayit => kayit.Id == id);

        if (talep is null)
        {
            return NotFound();
        }

        return View(new OzelDersGuncelleFormViewModel
        {
            Id = talep.Id,
            DersAdi = talep.DersAdi,
            UyeAdi = talep.UyeAdi,
            EgitmenAdi = talep.EgitmenAdi,
            IstenenTarih = talep.IstenenTarih
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult OzelDersGuncelle(OzelDersGuncelleFormViewModel model)
    {
        var talepler = _jsonDataService.Listele<OzelDersTalebi>(OzelDersTalepleriDosyasi);
        var talep = talepler.FirstOrDefault(kayit => kayit.Id == model.Id);

        if (talep is null)
        {
            return NotFound();
        }

        model.DersAdi = talep.DersAdi;
        model.UyeAdi = talep.UyeAdi;
        model.EgitmenAdi = talep.EgitmenAdi;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        talep.IstenenTarih = model.IstenenTarih;

        _jsonDataService.Kaydet(OzelDersTalepleriDosyasi, talepler);
        BasariMesaji("Ozel ders guncellendi.");
        return RedirectToAction(nameof(Dersler));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult OzelDersIptalEt(int id)
    {
        var talepler = _jsonDataService.Listele<OzelDersTalebi>(OzelDersTalepleriDosyasi);
        var talep = talepler.FirstOrDefault(kayit => kayit.Id == id);

        if (talep is null)
        {
            return NotFound();
        }

        talep.Durum = "Iptal Edildi";
        _jsonDataService.Kaydet(OzelDersTalepleriDosyasi, talepler);
        BasariMesaji("Ozel ders iptal edildi.");
        return RedirectToAction(nameof(Dersler));
    }

    public IActionResult OzelDersTalepleri()
    {
        return View(_jsonDataService.Listele<OzelDersTalebi>(OzelDersTalepleriDosyasi));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult OzelDersTalebiOnayla(int id)
    {
        return OzelDersTalebiDurumDegistir(id, "Onaylandi", "Ozel ders talebi onaylandi.");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult OzelDersTalebiReddet(int id)
    {
        return OzelDersTalebiDurumDegistir(id, "Reddedildi", "Ozel ders talebi reddedildi.");
    }

    public IActionResult Bildirimler()
    {
        var bildirimler = _jsonDataService.Listele<Bildirim>(BildirimlerDosyasi);

        if (bildirimler.Any(bildirim => !bildirim.OkunduMu))
        {
            foreach (var bildirim in bildirimler.Where(bildirim => !bildirim.OkunduMu))
            {
                bildirim.OkunduMu = true;
            }

            _jsonDataService.Kaydet(BildirimlerDosyasi, bildirimler);
        }

        return View(bildirimler);
    }

    private IActionResult OzelDersTalebiDurumDegistir(int id, string yeniDurum, string mesaj)
    {
        var talepler = _jsonDataService.Listele<OzelDersTalebi>(OzelDersTalepleriDosyasi);
        var talep = talepler.FirstOrDefault(kayit => kayit.Id == id);
        if (talep is null)
        {
            return NotFound();
        }

        if (yeniDurum == "Onaylandi")
        {
            if (_jsonDataService.DersZamaniCakismasiVarMi(talep.IstenenTarih, 50, talep.EgitmenAdi, talep.Id))
            {
                HataMesaji("Çakışma Hatası: Belirtilen tarihte eğitmenin başka bir dersi bulunmaktadır!");
                return RedirectToAction(nameof(OzelDersTalepleri));
            }
        }

        talep.Durum = yeniDurum;
        _jsonDataService.Kaydet(OzelDersTalepleriDosyasi, talepler);
        BasariMesaji(mesaj);
        return RedirectToAction(nameof(OzelDersTalepleri));
    }

    private static int YeniIdOlustur(IEnumerable<int> idler)
    {
        return idler.Any() ? idler.Max() + 1 : 1;
    }
}
