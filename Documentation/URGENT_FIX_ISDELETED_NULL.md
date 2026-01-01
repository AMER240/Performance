# ?? AC?L FIX - ESK? GÖREVLER GÖRÜNMÜYOR!

## ? SORUN

**Neden:** Database'e `IsDeleted` field'? eklendi ama **eski task'larda bu field NULL**!  
DbContext'teki query filter (`HasQueryFilter(t => !t.IsDeleted)`) NULL de?erleri de filtr eliyor.

## ? ÇÖZÜM 1: DATABASE'Deki NULL DE?ERLER? FALSE YAP (ÖNER?L?R)

### **SQL Script:**
```sql
-- Tüm NULL IsDeleted de?erlerini FALSE yap
UPDATE Tasks 
SET IsDeleted = 0 
WHERE IsDeleted IS NULL;

-- Ayn?s?n? DeletedAt için de yap
UPDATE Tasks 
SET DeletedAt = NULL 
WHERE DeletedAt IS NOT NULL AND IsDeleted = 0;
```

### **PowerShell'den Çal??t?r:**
```powershell
# SQL Server LocalDB'ye ba?lan ve çal??t?r
sqlcmd -S localhost -d PerformanceDb -E -C -Q "UPDATE Tasks SET IsDeleted = 0 WHERE IsDeleted IS NULL"
```

---

## ? ÇÖZÜM 2: QUERY FILTER'I DÜZELT (GEÇ?C?)

DbContext'teki query filter'i NULL-safe yap.

### **PerformanceDbContext.cs de?i?ikli?i:**
```csharp
// ? ESK? (NULL de?erleri filtreliyor):
entity.HasQueryFilter(t => !t.IsDeleted);

// ? YEN? (NULL de?erleri de dahil et):
entity.HasQueryFilter(t => t.IsDeleted == false || t.IsDeleted == null);
```

**Ama bu geçici bir çözüm!** Database'i düzeltmek daha iyi.

---

## ? ÇÖZÜM 3: M?GRATION ?LE DÜZ ELT (EN PROFESYONEL)

Yeni migration olu?tur ve eski task'lar?n `IsDeleted` de?erini FALSE yap.

### **Migration olu?tur:**
```bash
dotnet ef migrations add FixIsDeletedNullValues --project Performance
```

### **Migration içeri?i:**
```csharp
public partial class FixIsDeletedNullValues : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("UPDATE Tasks SET IsDeleted = 0 WHERE IsDeleted IS NULL");
        migrationBuilder.Sql("UPDATE Tasks SET DeletedAt = NULL WHERE DeletedAt IS NOT NULL AND IsDeleted = 0");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // No need to revert
    }
}
```

### **Apply migration:**
```bash
dotnet ef database update --project Performance
```

---

## ?? HIZLI FIX (??MD? YAP!)

### **1. SQL ile Düzelt (EN HIZLI):**
```powershell
# PowerShell'de çal??t?r:
sqlcmd -S localhost -d PerformanceDb -E -C -Q "UPDATE Tasks SET IsDeleted = 0 WHERE IsDeleted IS NULL"
```

### **2. Uygulamay? yeniden ba?lat:**
```bash
dotnet run --project Performance
```

### **3. Test et:**
- Proje aç ? Task listesi ? ? Eski görevler görünecek!

---

## ?? SORUN 2: ADD TASK YAVA?

### **Sebep:**
`Add_Click()` içinde `await ApplyFilters()` ça?r?l?yor. Bu async method ama UI thread bekliyor.

### **Çözüm:**
`ApplyFilters()` yerine `RefreshList()` kullan (daha basit ve h?zl?).

### **TaskListForm.cs de?i?ikli?i:**
```csharp
// ? ESK? (Yava?):
private async Task Add_Click()
{
    // ...
    if (form.ShowDialog(this) == DialogResult.OK)
    {
        await ApplyFilters(); // ? Filter'lar gereksiz yere tekrar uygulan?yor
    }
}

// ? YEN? (H?zl?):
private async Task Add_Click()
{
    // ...
    if (form.ShowDialog(this) == DialogResult.OK)
    {
        await RefreshList(); // ? Direkt liste yenileniyor
    }
}
```

**YA DA:**

Hiçbir ?ey yapma! Zaten `RefreshList()` ça?r?l?yor gibi görünüyor. Ama `includeRelations: true` yava?latabilir.

---

## ?? ÖNCEL?K

### **Acil (?imdi):**
1. ?? **SQL Script çal??t?r** (IsDeleted = 0)
2. ? **Uygulamay? yeniden ba?lat**
3. ? **Test et - Eski görevler görünecek**

### **Sonra (?yile?tirme):**
4. Migration olu?tur (FixIsDeletedNullValues)
5. Apply migration
6. Add Task h?z?n? test et

---

## ?? HEMEN YAP!

```powershell
# 1. SQL çal??t?r
sqlcmd -S localhost -d PerformanceDb -E -C -Q "UPDATE Tasks SET IsDeleted = 0 WHERE IsDeleted IS NULL"

# 2. Uygulamay? çal??t?r
dotnet run --project Performance

# 3. Test et
# - Login (manager/manager123)
# - Proje aç
# - Task listesini aç
# - ? Eski görevler görünüyor mu?
```

---

**NOT:** Bu sorun sadece **migration'dan önce olu?turulan task'larda** var. Yeni task'larda sorun olmayacak çünkü `IsDeleted = false` default de?eri var.
