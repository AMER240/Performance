# ?? DEBUG - ESK? GÖREVLER NEDEN GÖRÜNMÜYOR?

## TEST ADIMLARI

### 1. Database'de Task Var m? Kontrol Et
```sql
sqlcmd -S localhost -d PerformanceDb -E -C -Q "SELECT Id, Title, ProjectId, Status, IsDeleted, CreatedAt FROM Tasks"
```

**Sonuç:** ? 2 task var, IsDeleted = 0

---

### 2. EF Core Query Filter Kontrolü

**DbContext'teki filter:**
```csharp
entity.HasQueryFilter(t => !t.IsDeleted);
```

**Problem olabilir mi?**
- ? `IsDeleted` field'? boolean NOT NULL ama entity'de `bool` (non-nullable)
- ? Database'de zaten `IsDeleted = 0` (false)

**Çözüm:** IgnoreQueryFilters ile test et

---

### 3. Manuel Test (Code ile)

TaskService.cs'ye geçici kod ekle:

```csharp
public async Task<List<TaskEntity>> ListByProjectAsync(int projectId, bool includeRelations = false)
{
    // ? GEÇICI: Query filter'? devre d??? b?rak
    var query = _db.Tasks
        .IgnoreQueryFilters() // ? BU SATIRI EKLE
        .AsNoTracking()
        .Where(t => t.ProjectId == projectId);
    
    if (includeRelations)
    {
        query = query
            .Include(t => t.Project)
            .Include(t => t.AssignedToUser);
    }
    
    return await query.OrderBy(t => t.Status).ThenByDescending(t => t.Priority).ToListAsync();
}
```

### 4. Build ve Test

```bash
dotnet build
dotnet run --project Performance
```

**Beklenen Sonuç:**
- ? Task'lar görünecek (IgnoreQueryFilters sayesinde)

---

### 5. E?er Görünürse ? Query Filter Problemi

**Çözüm 1: Query Filter'? Düzelt**

DbContext.cs'de:
```csharp
// ? ESK?:
entity.HasQueryFilter(t => !t.IsDeleted);

// ? YEN? (NULL-safe):
entity.HasQueryFilter(t => t.IsDeleted == false);
```

**Çözüm 2: IsDeleted Default De?er Ekle**

Migration'da:
```csharp
entity.Property(t => t.IsDeleted)
    .HasDefaultValue(false)
    .IsRequired(); // ? NULL olmas?n
```

---

### 6. E?er Hala Görünmezse ? Ba?ka Problem

**Olas?l?klar:**
1. `_projectId` yanl?? de?er
2. `ListByProjectAsync` bo? döndürüyor
3. Grid binding sorunu

**Debug Kodu:**
```csharp
private async Task RefreshList()
{
    if (_projectId == 0) return;

    var tasks = await _taskService.ListByProjectAsync(_projectId, includeRelations: true);
    
    // ? DEBUG: Task say?s?n? kontrol et
    MessageBox.Show($"ProjectId: {_projectId}\nTask Count: {tasks.Count}", "Debug");
    
    _grid.DataSource = tasks;
    UpdateTaskCount(tasks.Count);
}
```

---

## ?? HIZLI FIX

### **Ad?m 1: Query Filter'? Geçici Devre D??? B?rak**

TaskService.cs ? `ListByProjectAsync` methoduna `.IgnoreQueryFilters()` ekle.

### **Ad?m 2: Build ve Test**

```bash
dotnet build
dotnet run --project Performance
```

### **Ad?m 3: Sonuca Göre Aksiyon Al**

**E?er task'lar görünürse:**
? Problem query filter'da
? DbContext'i düzelt (`t.IsDeleted == false`)

**E?er hala görünmezse:**
? Debug kodu ekle
? `_projectId` ve `tasks.Count` kontrol et

---

## ?? ??MD? YAPILACAK

```csharp
// Performance/Application/Services/TaskService.cs dosyas?n? aç

// ListByProjectAsync methodunda 32. sat?ra ?unu ekle:
var query = _db.Tasks
    .IgnoreQueryFilters() // ? EKLE
    .AsNoTracking()
    .Where(t => t.ProjectId == projectId);
```

**Build ve test et!**
