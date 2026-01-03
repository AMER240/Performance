# ?? Performance Application - .exe Olu?turma Rehberi

Bu rehber, Performance projesini çal??t?r?labilir .exe dosyas?na dönü?türme yöntemlerini aç?klar.

---

## ?? Yöntem 1: PowerShell Script ile (En Kolay)

### Self-Contained (Tek .exe, .NET gerektirmez)
```powershell
.\publish.bat
# veya
.\publish.bat SelfContained
```

**Dosya Boyutu:** ~150-200 MB  
**Gereksinim:** Yok (tüm .NET runtime dahil)  
**Da??t?m:** Sadece `publish` klasörünü kopyala

---

### Framework-Dependent (.NET 8 gerektirir)
```powershell
.\publish.bat FrameworkDependent
```

**Dosya Boyutu:** ~5-10 MB  
**Gereksinim:** .NET 8 Runtime  
**Da??t?m:** Hedef bilgisayarda .NET 8 yüklü olmal?

---

### Optimized (Trimmed + ReadyToRun)
```powershell
.\publish.bat Optimized
```

**Dosya Boyutu:** ~80-120 MB  
**Gereksinim:** Yok  
**Avantaj:** Daha h?zl? aç?l??, optimize edilmi?

---

## ?? Yöntem 2: Visual Studio ile Publish

### Ad?m Ad?m:

1. **Solution Explorer**'da `Performance` (UI) projesine sa? t?kla
2. **Publish...** seçene?ini t?kla
3. **Folder** hedefini seç
4. Konum: `publish`
5. **Show all settings** t?kla
6. ?u ayarlar? yap:
   - **Configuration:** Release
   - **Target Framework:** net8.0-windows
   - **Deployment Mode:** Self-contained
   - **Target Runtime:** win-x64
   - **File publish options:**
     - ? Produce single file
     - ? Enable ReadyToRun compilation
     - ? Trim unused code (opsiyonel)
7. **Save** ve **Publish** t?kla

**Sonuç:** `publish\Performance.exe` olu?turulur

---

## ?? Yöntem 3: Manuel dotnet CLI Komutlar?

### Self-Contained (Önerilen)
```bash
dotnet publish src\Performance.UI\Performance.csproj ^
  --configuration Release ^
  --runtime win-x64 ^
  --self-contained true ^
  --output publish ^
  -p:PublishSingleFile=true ^
  -p:IncludeNativeLibrariesForSelfExtract=true ^
  -p:EnableCompressionInSingleFile=true
```

### Framework-Dependent
```bash
dotnet publish src\Performance.UI\Performance.csproj ^
  --configuration Release ^
  --runtime win-x64 ^
  --self-contained false ^
  --output publish ^
  -p:PublishSingleFile=true
```

### Optimized + Trimmed
```bash
dotnet publish src\Performance.UI\Performance.csproj ^
  --configuration Release ^
  --runtime win-x64 ^
  --self-contained true ^
  --output publish ^
  -p:PublishSingleFile=true ^
  -p:PublishReadyToRun=true ^
  -p:PublishTrimmed=true
```

---

## ?? Publish Sonras? Dosya Yap?s?

```
publish/
??? Performance.exe          ? Ana uygulama (çal??t?r?labilir)
??? appsettings.json         ? Yap?land?rma (Gemini API Key burada!)
??? Performance.pdb          ? Debug symbols (opsiyonel, silebilirsiniz)
```

---

## ?? Da??t?m ?çin Yap?lacaklar

### 1. API Key Ekle
`publish/appsettings.json` dosyas?n? düzenle:
```json
{
  "GeminiAI": {
    "ApiKey": "YOUR_GEMINI_API_KEY_HERE",
    "Model": "gemini-2.5-flash"
  }
}
```

### 2. Veritaban? Ba?lant?s?
Ayn? dosyada connection string'i düzenle:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=PerformanceDb;..."
  }
}
```

### 3. Dosyalar? Paketleme
```
Performance_v1.1.0.zip
??? Performance.exe
??? appsettings.json
??? README.txt (kullan?m talimatlar?)
```

---

## ?? Geli?mi? Yap?land?rma

### Performance.csproj Dosyas?na Ekle

Projeyi her zaman single-file olarak publish etmek için:

```xml
<PropertyGroup>
  <PublishSingleFile>true</PublishSingleFile>
  <SelfContained>true</SelfContained>
  <RuntimeIdentifier>win-x64</RuntimeIdentifier>
  <PublishReadyToRun>true</PublishReadyToRun>
  <IncludeNativeLibrariesForSelfExtract>true</IncludeNativeLibrariesForSelfExtract>
  <EnableCompressionInSingleFile>true</EnableCompressionInSingleFile>
</PropertyGroup>
```

Sonra sadece:
```bash
dotnet publish -c Release
```

---

## ?? Sorun Giderme

### Hata: "DevExpress components not found"
**Çözüm:** Self-contained mode kullan?n (tüm DLL'ler dahil olur)

### Hata: "appsettings.json not found"
**Çözüm:** `Performance.csproj` dosyas?na ekle:
```xml
<ItemGroup>
  <None Update="appsettings.json">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    <CopyToPublishDirectory>PreserveNewest</CopyToPublishDirectory>
  </None>
</ItemGroup>
```

### .exe dosyas? çok büyük
**Çözüm:** Framework-dependent mode kullan (ama hedef bilgisayarda .NET 8 gerekir)

### Uygulama aç?l?rken hata veriyor
**Çözüm:** 
1. `appsettings.json` dosyas?n?n `publish` klasöründe oldu?unu kontrol et
2. SQL Server LocalDB'nin yüklü oldu?unu kontrol et
3. Gemini API key'in geçerli oldu?unu kontrol et

---

## ?? Deployment Checklist

- [ ] Projeyi Release modunda build et
- [ ] Publish komutunu çal??t?r
- [ ] `publish` klasörünü kontrol et
- [ ] `Performance.exe` dosyas?n? test et
- [ ] `appsettings.json` içinde API key ekle
- [ ] Connection string'i düzenle
- [ ] Tüm dosyalar? .zip olarak paketle
- [ ] README.txt dosyas? ekle
- [ ] Kullan?c? için kurulum talimatlar? yaz

---

## ?? Kullan?c? ?çin README.txt ?ablonu

```
Performance - Task & Project Management System
Version 1.1.0

SISTEM GEREKS?N?MLER?:
- Windows 10/11 (64-bit)
- SQL Server LocalDB (Visual Studio ile gelir)
- ?nternet ba?lant?s? (Gemini AI için, opsiyonel)

KURULUM:
1. Tüm dosyalar? bir klasöre ç?kart?n
2. Performance.exe dosyas?n? çal??t?r?n
3. ?lk çal??mada veritaban? otomatik olu?turulur

G?R?? B?LG?LER?:
Manager:
  Kullan?c? ad?: manager
  ?ifre: manager123

Employee:
  Kullan?c? ad?: employee
  ?ifre: employee123

GEMINI AI AYARLARI (Opsiyonel):
1. appsettings.json dosyas?n? not defteri ile aç?n
2. "ApiKey" alan?na Gemini API anahtar?n?z? girin
3. Kaydedin ve uygulamay? yeniden ba?lat?n

API KEY ALMAK ?Ç?N:
https://aistudio.google.com/app/apikey

DESTEK:
GitHub: https://github.com/AMER240/Performance
```

---

## ?? H?zl? Ba?lang?ç

En basit yöntem:

```powershell
# 1. Script ile publish et
.\publish.bat

# 2. Çal??t?r
cd publish
.\Performance.exe
```

**Tüm i?lem 1-2 dakika sürer!** ??

---

## ?? Yard?m

Sorun ya?arsan:
- GitHub Issues: https://github.com/AMER240/Performance/issues
- publish.ps1 script'i detayl? hata mesajlar? verir

---

**Son Güncelleme:** 2 Ocak 2026  
**Proje:** Performance v1.1.0  
**Deployment:** Windows x64
