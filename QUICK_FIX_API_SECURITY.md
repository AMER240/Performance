# ? HIZLI ÇÖZÜM - API KEY GÜVENL???

## ?? **?U ANDA YAPILACAKLAR**

### **Seçenek A: Kolay Yol (Önerilir)** ?

#### **1. Yedek Al (30 saniye)**
```powershell
xcopy /E /I /H "C:\Users\amers\OneDrive\Desktop\OOP Project\Performance" "C:\Users\amers\Desktop\Performance_Backup"
```

#### **2. API Key'i Yenile (2 dakika)**
```
1. https://makersuite.google.com/app/apikey
2. Eski key'i S?L
3. Yeni key OLU?TUR
4. Kopyala
```

#### **3. GitHub Repo'yu S?f?rla (5 dakika)**
```
A) GitHub.com ? Performance ? Settings ? Delete repository
B) GitHub.com ? New Repository ? "Performance" ? Create
C) PowerShell:
   cd "C:\Users\amers\OneDrive\Desktop\OOP Project\Performance"
   Remove-Item -Recurse -Force .git
   git init
   git add .
   git commit -m "Initial commit - Secure version"
   git branch -M main
   git remote add origin https://github.com/AMER240/Performance.git
   git push -u origin main
```

#### **4. Yeni API Key'i Ekle (30 saniye)**
```json
// Performance/appsettings.json (sadece yerel)
{
  "GeminiAI": {
    "ApiKey": "YEN?_KEY_BURAYA"
  }
}
```

#### **5. Test Et**
```sh
dotnet run --project Performance
```

**Toplam Süre:** ~8 dakika  
**Sonuç:** ? Temiz GitHub geçmi?i, yeni kodlar korundu

---

### **Seçenek B: Git Geçmi?ini Koru (?leri Seviye)**

Detayl? rehber: `GITHUB_CLEANUP_GUIDE.md`

---

## ? **TAMAMLANDI**

- ? `appsettings.json` template olu?turuldu
- ? `.gitignore` zaten mevcut
- ? API key placeholder eklendi
- ? Güvenlik rehberleri haz?r

---

## ?? **REHBERLER**

| Dosya | Ne ?çin? |
|-------|----------|
| `GITHUB_CLEANUP_GUIDE.md` | Detayl? temizleme ad?mlar? |
| `API_SECURITY_SETUP.md` | Gelecekte güvenli kurulum |
| `appsettings.template.json` | GitHub'a yüklenecek template |
| `appsettings.json` | Yerel kullan?m (ignore'da) |

---

## ?? **??MD? NE YAPILACAK?**

1. **Seçenek A'y? uygula** (8 dakika)
2. **Yeni API key al**
3. **Test et**
4. **Kullan!** ??

---

**Ba?ar?lar!** ??
