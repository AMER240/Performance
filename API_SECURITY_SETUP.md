# ?? API KEY GÜVENL?K KURULUMU

## ?? ÖNEML? UYARI

**appsettings.json dosyas? asla GitHub'a yüklenmemelidir!**

---

## ?? **KURULUM ADIMLARI**

### **1. Template'den Kopyala**

```sh
# Windows (PowerShell)
Copy-Item Performance/appsettings.template.json Performance/appsettings.json

# Linux/Mac
cp Performance/appsettings.template.json Performance/appsettings.json
```

### **2. API Key Ekle**

`Performance/appsettings.json` dosyas?n? aç ve API key'ini ekle:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=PerformanceDb;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  },
  "GeminiAI": {
    "ApiKey": "BURAYA_GERÇEk_API_KEY_G?R",
    "Model": "gemini-2.5-flash"
  }
}
```

### **3. API Key Nas?l Al?n?r?**

1. https://makersuite.google.com/app/apikey adresine git
2. Google hesab?nla giri? yap
3. "Create API Key" t?kla
4. Olu?turulan key'i kopyala
5. `appsettings.json`'a yap??t?r

---

## ??? **GÜVENL?K**

### **.gitignore Kontrol**

`.gitignore` dosyas? zaten `appsettings.json`'u ignore ediyor:

```gitignore
Performance/appsettings.json
```

### **Do?rulama**

API key'in Git'e eklenmedi?ini kontrol et:

```sh
git status
```

Ç?kt?da `Performance/appsettings.json` **GÖRÜNMEMEL?**!

---

## ?? **ÇALI?TIRMA**

```sh
# Program? çal??t?r
dotnet run --project Performance

# Login
Username: manager
Password: manager123
```

---

## ?? **DOSYA YAPISI**

```
Performance/
??? appsettings.template.json  ? (GitHub'da)
??? appsettings.json           ? (Sadece yerel, .gitignore'da)
```

---

## ?? **YEN? CLONE SONRASI**

Ba?ka bir bilgisayarda çal???rken:

```sh
# 1. Repository clone
git clone https://github.com/AMER240/Performance.git
cd Performance

# 2. Template'den kopyala
Copy-Item Performance/appsettings.template.json Performance/appsettings.json

# 3. API key ekle
# appsettings.json'u düzenle

# 4. Çal??t?r
dotnet run --project Performance
```

---

## ?? **ASLA YAPMAYIN!**

```sh
# ? YANLI? - appsettings.json'u commit etmeyin!
git add Performance/appsettings.json
git commit -m "Add API key"  # TEHLIKE!

# ? DO?RU - Template'i kullan?n
git add Performance/appsettings.template.json
git commit -m "Add settings template"
```

---

## ?? **HATA: API Key Yüklendiyse?**

E?er yanl??l?kla API key'i GitHub'a yüklediyseniz:

1. **Hemen API key'i yenileyin!**
   - https://makersuite.google.com/app/apikey
   - Eski key'i sil
   - Yeni key olu?tur

2. **GitHub geçmi?ini temizleyin**
   - `GITHUB_CLEANUP_GUIDE.md` dosyas?n? okuyun
   - Seçenek 4: Repository'yi yeniden olu?tur (önerilir)

---

## ?? **DESTEK**

Sorular için:
- GitHub Issues: https://github.com/AMER240/Performance/issues
- `GITHUB_CLEANUP_GUIDE.md` - API key temizleme rehberi

---

*Güvenlik Seviyesi: ?? Yüksek*  
*Son Güncelleme: 30 Aral?k 2024*
