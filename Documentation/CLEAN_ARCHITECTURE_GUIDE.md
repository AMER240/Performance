# ??? MODERN CLEAN ARCHITECTURE - TAM REHBER?

## ?? MEVCUT DURUM ANAL?Z?

### **?u Anki Yap? (Monolithic):**
```
Performance/
??? Performance.csproj (UI + Business + Data - HEPS? TEK PROJEDE)
??? Application/Services/              ? Business Logic
??? Infrastructure/                     ? Data Access
?   ??? Application/PerformanceDbContext.cs
?   ??? Entities/
?   ??? Migrations/
??? Forms/                              ? UI
```

**Sorunlar:**
- ? Tüm katmanlar tek projede (Tight coupling)
- ? UI katman? EF Core'a direkt ba??ml?
- ? Test edilmesi zor
- ? Ba??ms?z deploy edilemiyor
- ? Domain logic ile infrastructure kar???k

---

## ?? HEDEF M?MAR? (Modern Clean Architecture)

```
Performance.sln
?
??? src/
?   ??? 1. Core Layer (?çteki Halka - Hiçbir d?? ba??ml?l?k yok)
?   ?   ??? Performance.Domain/              ? Entities + Value Objects + Enums
?   ?   ?   ??? Entities/
?   ?   ?   ?   ??? UserEntity.cs
?   ?   ?   ?   ??? ProjectEntity.cs
?   ?   ?   ?   ??? TaskEntity.cs
?   ?   ?   ??? Enums/
?   ?   ?   ?   ??? UserRole.cs
?   ?   ?   ?   ??? TaskStatus.cs
?   ?   ?   ?   ??? TaskPriority.cs
?   ?   ?   ?   ??? ProjectStatus.cs
?   ?   ?   ??? Common/
?   ?   ?       ??? BaseEntity.cs (opsiyonel)
?   ?   ?
?   ?   ??? Performance.Application/         ? Business Logic + Interfaces
?   ?       ??? Interfaces/
?   ?       ?   ??? IProjectService.cs
?   ?       ?   ??? ITaskService.cs
?   ?       ?   ??? IUserService.cs
?   ?       ?   ??? ITaskSuggestionService.cs
?   ?       ?   ??? IProjectSuggestionService.cs
?   ?       ??? Services/
?   ?       ?   ??? ProjectService.cs
?   ?       ?   ??? TaskService.cs
?   ?       ?   ??? UserService.cs
?   ?       ?   ??? EnhancedTaskSuggestionService.cs
?   ?       ?   ??? GeminiTaskSuggestionService.cs
?   ?       ?   ??? GeminiProjectSuggestionService.cs
?   ?       ??? DTOs/ (opsiyonel)
?   ?       ??? Validators/ (opsiyonel)
?   ?
?   ??? 2. Infrastructure Layer (D??taki Halka - Core'a ba??ml?)
?   ?   ??? Performance.Infrastructure/
?   ?       ??? Data/
?   ?       ?   ??? PerformanceDbContext.cs
?   ?       ?   ??? Configurations/          ? Fluent API
?   ?       ?       ??? UserConfiguration.cs
?   ?       ?       ??? ProjectConfiguration.cs
?   ?       ?       ??? TaskConfiguration.cs
?   ?       ??? Migrations/
?   ?       ??? Repositories/ (opsiyonel)
?   ?       ?   ??? IRepository.cs
?   ?       ?   ??? Repository.cs
?   ?       ??? ExternalServices/
?   ?           ??? GeminiApiClient.cs
?   ?
?   ??? 3. Presentation Layer (En d??taki halka - Herkese ba??ml?)
?       ??? Performance.UI/
?           ??? Forms/
?           ?   ??? LoginForm.cs
?           ?   ??? MainForm.cs
?           ?   ??? ProjectEditForm.cs
?           ?   ??? ...
?           ??? UI/
?           ?   ??? UiColors.cs
?           ?   ??? UiHelpers.cs
?           ?   ??? BaseForm.cs
?           ??? Program.cs
?           ??? appsettings.json
?
??? tests/ (opsiyonel)
    ??? Performance.Domain.Tests/
    ??? Performance.Application.Tests/
    ??? Performance.Infrastructure.Tests/
```

---

## ?? BA?IMLILIK KURALLARI (Dependency Rule)

```
???????????????????????????????????????????????????????????????
?                                                             ?
?   Performance.UI (Presentation)                            ?
?   ??? References: Application, Infrastructure, Domain      ?
?   ??? Dependency: ???????????????????????????????????????  ?
?                                                          ?  ?
?   ????????????????????????????????????????????????????  ?  ?
?   ?                                                  ?  ?  ?
?   ?   Performance.Infrastructure                    ?  ?  ?
?   ?   ??? References: Application, Domain           ?  ?  ?
?   ?   ??? Dependency: ?????????????????????????     ?  ?  ?
?   ?                                           ?     ?  ?  ?
?   ?   ?????????????????????????????????????  ?     ?  ?  ?
?   ?   ?                                   ?  ?     ?  ?  ?
?   ?   ?   Performance.Application         ?  ?     ?  ?  ?
?   ?   ?   ??? References: Domain ONLY     ?  ?     ?  ?  ?
?   ?   ?   ??? Dependency: ??????????      ?  ?     ?  ?  ?
?   ?   ?                            ?      ?  ?     ?  ?  ?
?   ?   ?   ??????????????????????   ?      ?  ?     ?  ?  ?
?   ?   ?   ?                    ?   ?      ?  ?     ?  ?  ?
?   ?   ?   ? Performance.Domain ?????      ?  ?     ?  ?  ?
?   ?   ?   ? (NO DEPENDENCIES!) ????????????  ?     ?  ?  ?
?   ?   ?   ?                    ???????????????     ?  ?  ?
?   ?   ?   ??????????????????????????????????????????  ?  ?
?   ?   ?                                               ?  ?
?   ?   ?????????????????????????????????????????????????  ?
?   ?                                                      ?
?   ????????????????????????????????????????????????????????
?                                                             ?
???????????????????????????????????????????????????????????????

? ?çten d??a ba??ml?l?k YASAK!
? D??tan içe ba??ml?l?k TAMAM!
? Domain hiçbir ?eye ba??ml? de?il!
```

---

## ?? ADIM ADIM OLU?TURMA

### **Ad?m 1: Yeni Proje Yap?s? Olu?tur**

```powershell
# Workspace root'a git
cd C:\Users\amers\OneDrive\Desktop\Performance

# src klasörü olu?tur
New-Item -Path "src" -ItemType Directory -Force

# Domain projesi olu?tur
dotnet new classlib -n Performance.Domain -o src/Performance.Domain -f net8.0
Remove-Item "src/Performance.Domain/Class1.cs" -Force

# Application projesi olu?tur
dotnet new classlib -n Performance.Application -o src/Performance.Application -f net8.0
Remove-Item "src/Performance.Application/Class1.cs" -Force

# Infrastructure projesi olu?tur
dotnet new classlib -n Performance.Infrastructure -o src/Performance.Infrastructure -f net8.0
Remove-Item "src/Performance.Infrastructure/Class1.cs" -Force

# UI projesi (mevcut Performance projesini ta??)
Move-Item "Performance" "src/Performance.UI"

# Solution olu?tur
dotnet new sln -n Performance
dotnet sln add src/Performance.Domain/Performance.Domain.csproj
dotnet sln add src/Performance.Application/Performance.Application.csproj
dotnet sln add src/Performance.Infrastructure/Performance.Infrastructure.csproj
dotnet sln add src/Performance.UI/Performance.csproj
```

---

### **Ad?m 2: Proje Ba??ml?l?klar? Ekle**

```powershell
# Application ? Domain
dotnet add src/Performance.Application reference src/Performance.Domain

# Infrastructure ? Application + Domain
dotnet add src/Performance.Infrastructure reference src/Performance.Application
dotnet add src/Performance.Infrastructure reference src/Performance.Domain

# UI ? Application + Infrastructure + Domain
dotnet add src/Performance.UI reference src/Performance.Application
dotnet add src/Performance.UI reference src/Performance.Infrastructure
dotnet add src/Performance.UI reference src/Performance.Domain
```

---

### **Ad?m 3: NuGet Paketlerini Ekle**

```powershell
# Infrastructure (EF Core)
dotnet add src/Performance.Infrastructure package Microsoft.EntityFrameworkCore
dotnet add src/Performance.Infrastructure package Microsoft.EntityFrameworkCore.SqlServer
dotnet add src/Performance.Infrastructure package Microsoft.EntityFrameworkCore.Design

# UI (DI + Configuration)
dotnet add src/Performance.UI package Microsoft.Extensions.Hosting
dotnet add src/Performance.UI package Microsoft.Extensions.Configuration.Json
dotnet add src/Performance.UI package Microsoft.Extensions.DependencyInjection

# UI (DevExpress - zaten var)
# dotnet add src/Performance.UI package DevExpress.Win.Design
```

---

### **Ad?m 4: Domain Layer - Entities Ta??**

**Performance.Domain klasör yap?s?:**
```
Performance.Domain/
??? Entities/
?   ??? UserEntity.cs
?   ??? ProjectEntity.cs
?   ??? TaskEntity.cs
??? Enums/
?   ??? UserRole.cs
?   ??? TaskStatus.cs
?   ??? TaskPriority.cs
?   ??? ProjectStatus.cs
??? Performance.Domain.csproj
```

**UserEntity.cs içinde namespace de?i?ikli?i:**
```csharp
// ? ESK?:
namespace Performance.Infrastructure.Entities

// ? YEN?:
namespace Performance.Domain.Entities
```

---

### **Ad?m 5: Application Layer - Services Ta??**

**Performance.Application klasör yap?s?:**
```
Performance.Application/
??? Interfaces/
?   ??? IProjectService.cs
?   ??? ITaskService.cs
?   ??? IUserService.cs
?   ??? ITaskSuggestionService.cs
?   ??? IProjectSuggestionService.cs
??? Services/
?   ??? ProjectService.cs
?   ??? TaskService.cs
?   ??? UserService.cs
?   ??? EnhancedTaskSuggestionService.cs
?   ??? GeminiTaskSuggestionService.cs
?   ??? GeminiProjectSuggestionService.cs
??? Performance.Application.csproj
```

**Namespace de?i?ikli?i:**
```csharp
// ? ESK?:
namespace Performance.Application.Services

// ? YEN?:
namespace Performance.Application.Interfaces  // Interfaces için
namespace Performance.Application.Services     // Services için
```

**Using de?i?ikli?i:**
```csharp
// ? ESK?:
using Performance.Infrastructure.Entities;
using Performance.Infrastructure.Application;

// ? YEN?:
using Performance.Domain.Entities;
using Performance.Infrastructure.Data;
```

---

### **Ad?m 6: Infrastructure Layer - DbContext Ta??**

**Performance.Infrastructure klasör yap?s?:**
```
Performance.Infrastructure/
??? Data/
?   ??? PerformanceDbContext.cs
?   ??? Configurations/
?       ??? UserConfiguration.cs
?       ??? ProjectConfiguration.cs
?       ??? TaskConfiguration.cs
??? Migrations/
?   ??? (tüm migration dosyalar?)
??? Performance.Infrastructure.csproj
```

**PerformanceDbContext.cs namespace de?i?ikli?i:**
```csharp
// ? ESK?:
namespace Performance.Infrastructure.Application

// ? YEN?:
namespace Performance.Infrastructure.Data

// Using de?i?iklikleri:
using Performance.Domain.Entities;  // ? YEN?
```

---

### **Ad?m 7: UI Layer - Temizlik**

**Performance.UI içinden S?L:**
```powershell
# Eski klasörleri sil
Remove-Item "src/Performance.UI/Application" -Recurse -Force
Remove-Item "src/Performance.UI/Infrastructure" -Recurse -Force
```

**Program.cs güncelle:**
```csharp
// ? ESK?:
using Performance.Application.Services;
using Performance.Infrastructure.Application;

// ? YEN?:
using Performance.Application.Interfaces;
using Performance.Application.Services;
using Performance.Infrastructure.Data;
using Performance.Domain.Entities;
```

---

## ?? ÖRNEK DOSYA ?ÇER?KLER?

### **Performance.Domain.csproj:**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
</Project>
```

### **Performance.Application.csproj:**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  
  <ItemGroup>
    <ProjectReference Include="..\Performance.Domain\Performance.Domain.csproj" />
  </ItemGroup>
  
  <ItemGroup>
    <PackageReference Include="System.Net.Http.Json" Version="8.0.0" />
  </ItemGroup>
</Project>
```

### **Performance.Infrastructure.csproj:**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  
  <ItemGroup>
    <ProjectReference Include="..\Performance.Domain\Performance.Domain.csproj" />
    <ProjectReference Include="..\Performance.Application\Performance.Application.csproj" />
  </ItemGroup>
  
  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.0" />
  </ItemGroup>
</Project>
```

### **Performance.UI (Performance.csproj):**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0-windows</TargetFramework>
    <UseWindowsForms>true</UseWindowsForms>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  
  <ItemGroup>
    <ProjectReference Include="..\Performance.Domain\Performance.Domain.csproj" />
    <ProjectReference Include="..\Performance.Application\Performance.Application.csproj" />
    <ProjectReference Include="..\Performance.Infrastructure\Performance.Infrastructure.csproj" />
  </ItemGroup>
  
  <ItemGroup>
    <PackageReference Include="DevExpress.Win.Design" Version="25.1.5" />
    <PackageReference Include="Microsoft.Extensions.Hosting" Version="8.0.1" />
    <PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="8.0.1" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.1" />
  </ItemGroup>
  
  <ItemGroup>
    <None Update="appsettings.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
  </ItemGroup>
</Project>
```

---

## ?? MIGRATION KOMUTLARI

```powershell
# Migration ekle (Infrastructure'dan çal??t?r)
dotnet ef migrations add MigrationName `
  --project src/Performance.Infrastructure `
  --startup-project src/Performance.UI

# Database güncelle
dotnet ef database update `
  --project src/Performance.Infrastructure `
  --startup-project src/Performance.UI

# Migration sil
dotnet ef migrations remove `
  --project src/Performance.Infrastructure `
  --startup-project src/Performance.UI
```

---

## ? AVANTAJLAR

### **Kod Kalitesi:**
```
? Separation of Concerns (Her katman kendi i?ini yapar)
? Single Responsibility Principle
? Dependency Inversion Principle
? Testable (Her katman ba??ms?z test edilebilir)
? Maintainable (De?i?iklikler izole)
```

### **Mimari Avantajlar:**
```
? Domain hiçbir ?eye ba??ml? de?il (Pure POCO)
? Business logic UI'dan ba??ms?z
? Infrastructure de?i?tirilebilir (SQL ? NoSQL)
? Multiple UI desteklenebilir (WinForms, Web, Mobile)
? Microservice'e geçi? kolay
```

### **Geli?tirme Avantajlar?:**
```
? Paralel çal??ma (Her katmanda farkl? developer)
? Hata izolasyonu (Sorun hangi katmanda?)
? Versiyon kontrolü (Git'te daha temiz)
? CI/CD kolayl???
? Code review daha kolay
```

---

## ?? SON YAPIM

```
Performance.sln                                  ? Solution File
?
??? src/
?   ??? Performance.Domain/                     ? Core (?çteki Halka)
?   ?   ??? Entities/
?   ?   ??? Enums/
?   ?   ??? Performance.Domain.csproj
?   ?
?   ??? Performance.Application/                ? Core (?ç Halka)
?   ?   ??? Interfaces/
?   ?   ??? Services/
?   ?   ??? Performance.Application.csproj
?   ?
?   ??? Performance.Infrastructure/             ? D?? Halka
?   ?   ??? Data/
?   ?   ??? Migrations/
?   ?   ??? Performance.Infrastructure.csproj
?   ?
?   ??? Performance.UI/                          ? En D?? Halka
?       ??? Forms/
?       ??? UI/
?       ??? Program.cs
?       ??? appsettings.json
?       ??? Performance.csproj
?
??? tests/ (opsiyonel)
    ??? Performance.Domain.Tests/
    ??? Performance.Application.Tests/
    ??? Performance.Infrastructure.Tests/
```

---

## ?? NEXT STEPS

1. ? Bu rehberi oku
2. ? PowerShell komutlar?n? çal??t?r (yukar?daki Ad?m 1-7)
3. ? Dosyalar? ta?? ve namespace'leri düzelt
4. ? Build testi yap
5. ? Migration'lar? yeniden olu?tur
6. ? Uygulamay? çal??t?r ve test et

**Haz?r m?s?n? Ba?layal?m!** ??
