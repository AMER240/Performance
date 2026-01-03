# ?? Security & Configuration Guide

## ?? IMPORTANT: API Key Security

**NEVER commit `appsettings.json` to GitHub!** This file contains sensitive information like:
- Gemini AI API Key
- Database connection strings
- Other secrets

---

## ??? Güvenlik Önlemleri

### 1. appsettings.json Dosyas?

? **YAP:**
- `appsettings.example.json` dosyas?n? GitHub'a commit et (?ablon olarak)
- `appsettings.json` dosyas?n? `.gitignore` içinde tut
- API key'leri environment variables ile kullan

? **YAPMA:**
- `appsettings.json` dosyas?n? GitHub'a push etme
- API key'leri kod içine hard-code etme
- ?ifreleri plain text olarak sakla

---

## ?? ?lk Kurulum (Yeni Kullan?c?lar ?çin)

### Ad?m 1: appsettings.json Olu?tur

```bash
# Example dosyas?n? kopyala
Copy-Item src\Performance.UI\appsettings.example.json src\Performance.UI\appsettings.json
```

### Ad?m 2: API Key Ekle

`src\Performance.UI\appsettings.json` dosyas?n? düzenle:

```json
{
  "GeminiAI": {
    "ApiKey": "YOUR_ACTUAL_API_KEY_HERE",
    "Model": "gemini-2.5-flash"
  }
}
```

### Ad?m 3: Git Kontrolü

```bash
# appsettings.json'?n ignore edildi?ini kontrol et
git status

# Ç?kt?da "appsettings.json" OLMAMALI
```

---

## ?? E?er Yanl??l?kla API Key Commit Ettiyseniz

### Acil Durum Ad?mlar?:

1. **API Key'i Hemen ?ptal Et**
   - [Google AI Studio](https://aistudio.google.com/app/apikey) ? Delete Key

2. **Yeni API Key Olu?tur**

3. **Git History'den Kald?r**
   ```bash
   # Son commit'i geri al
   git reset --soft HEAD~1
   
   # appsettings.json'? git'ten kald?r
   git rm --cached src/Performance.UI/appsettings.json
   
   # Yeniden commit et
   git add .
   git commit -m "Remove appsettings.json from git tracking"
   git push --force
   ```

4. **GitHub'da Dosyay? Kontrol Et**
   - Repository'de `appsettings.json` dosyas? OLMAMALI

---

## ?? .gitignore Ayarlar?

`.gitignore` dosyas? ?u kurallar? içermeli:

```gitignore
# Configuration files with API keys
**/appsettings.json
!**/appsettings.example.json

# Backup files
*.Backup.tmp
*.Backup*.tmp
*.bak

# Publish directory
publish/

# Database files
*.mdf
*.ldf
```

---

## ?? appsettings.example.json ?ablonu

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=PerformanceDb;Trusted_Connection=True;..."
  },
  "GeminiAI": {
    "ApiKey": "YOUR_GEMINI_API_KEY_HERE",
    "Model": "gemini-2.5-flash"
  }
}
```

Bu dosya GitHub'a commit edilebilir çünkü gerçek API key içermiyor.

---

## ?? *.Backup.tmp Dosyalar? Nedir?

**Neden olu?ur:**
- Visual Studio veya Rider, `.csproj` dosyalar?n? düzenlerken yedek olu?turur
- Genellikle NuGet package install/update s?ras?nda
- Otomatik yedekleme mekanizmas?

**Ne yapmal?:**
- ? `.gitignore`'a ekle ? `*.Backup*.tmp`
- ? Silebilirsin (otomatik olu?turulur)
- ? GitHub'a commit etme

**Silmek için:**
```powershell
# Tüm backup dosyalar?n? sil
Get-ChildItem -Recurse -Filter "*.Backup*.tmp" | Remove-Item -Force
```

---

## ?? Git Status Kontrolü

### Do?ru Ç?kt?:
```bash
$ git status

On branch main
Untracked files:
  appsettings.example.json   ? ? Bu TAMAM
  
# appsettings.json BURADA OLMAMALI ?
```

### Yanl?? Ç?kt?:
```bash
$ git status

Changes to be committed:
  appsettings.json           ? ? TEHL?KE!
```

E?er `appsettings.json` görüyorsan:
```bash
git reset HEAD appsettings.json
git rm --cached appsettings.json
```

---

## ?? Environment Variables (Alternatif Yöntem)

### Sistem Ortam De?i?kenleri ile API Key Kullan?m?

**Windows:**
```powershell
# PowerShell (Admin)
[System.Environment]::SetEnvironmentVariable('GEMINI_API_KEY', 'your-api-key', 'User')
```

**Kod De?i?ikli?i:**
```csharp
// Program.cs veya Startup.cs
var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY") 
             ?? configuration["GeminiAI:ApiKey"];
```

Bu yöntemle:
- ? API key kodda/config'de olmaz
- ? Sunucu ortam?nda güvenli
- ? GitHub'a hiç yüklenmez

---

## ?? Güvenlik Checklist

Deployment öncesi kontrol:

- [ ] `appsettings.json` `.gitignore`'da m??
- [ ] `appsettings.example.json` GitHub'da m??
- [ ] `git status` ç?kt?s?nda `appsettings.json` YOK mu?
- [ ] API key environment variable olarak m? kullan?l?yor? (opsiyonel)
- [ ] Backup dosyalar? (*.tmp) `.gitignore`'da m??
- [ ] `publish/` klasörü `.gitignore`'da m??

---

## ?? Yard?m

**API Key s?zd?r?ld? m??**
1. Hemen API key'i iptal et
2. Git history'den dosyay? kald?r
3. Yeni key olu?tur
4. `.gitignore` kurallar?n? kontrol et

**GitHub'da hala görünüyor mu?**
```bash
git filter-branch --force --index-filter \
  "git rm --cached --ignore-unmatch src/Performance.UI/appsettings.json" \
  --prune-empty --tag-name-filter cat -- --all

git push --force --all
```

?? **D?KKAT:** Bu i?lem git history'yi de?i?tirir!

---

## ?? ?leti?im

Güvenlik sorunlar? için:
- GitHub Security Advisory olu?tur
- Private issue aç

---

**Son Güncelleme:** 2 Ocak 2026  
**Güvenlik Seviyesi:** ?? High
