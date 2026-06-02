using System.Text.Json;

namespace FitReserve.Services;

public class JsonDataService
{
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

    private string DosyaYoluOlustur(string dosyaAdi)
    {
        var guvenliDosyaAdi = Path.GetFileName(dosyaAdi);
        return Path.Combine(_dataKlasoru, guvenliDosyaAdi);
    }
}
