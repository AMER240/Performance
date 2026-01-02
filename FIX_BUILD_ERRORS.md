# ?? BU ?LD HATALARI - HIZLI ÇÖZÜM

**2 Error var - Çok kolay düzeltme! 3 dakika**

---

## ?? HATALAR

```
MainForm.cs(397): error CS1503: Argument 2: cannot convert from 'UserEntity' to 'IProjectSuggestionService'
MainForm.cs(410): error CS1503: Argument 2: cannot convert from 'UserEntity' to 'IProjectSuggestionService'
```

---

## ? ÇÖZÜM: MainForm.cs Düzenle (2 yer)

### Dosya: `src/Performance.UI/MainForm.cs`

### 1?? Sat?r ~397: AddProject_Click Metodu

**ESK? (Sil):**
```csharp
private async Task AddProject_Click()
{
    using var scope = _serviceProvider.CreateScope();
    var projectService = scope.ServiceProvider.GetRequiredService<IProjectService>();
    var form = new ProjectEditForm(projectService, _authenticatedUser);
    if (form.ShowDialog(this) == DialogResult.OK)
    {
        await RefreshProjects(_txtSearch.Text);
        await _statsPanel.RefreshStatistics();
    }
}
```

**YEN? (Yap??t?r):**
```csharp
private async Task AddProject_Click()
{
    using var scope = _serviceProvider.CreateScope();
    var form = scope.ServiceProvider.GetRequiredService<ProjectEditForm>();
    if (form.ShowDialog(this) == DialogResult.OK)
    {
        await RefreshProjects(_txtSearch.Text);
        await _statsPanel.RefreshStatistics();
    }
}
```

---

### 2?? Sat?r ~410: EditProject_Click Metodu

**ESK? (Sil):**
```csharp
private async Task EditProject_Click()
{
    if (_selectedProject == null) return;
    using var scope = _serviceProvider.CreateScope();
    var projectService = scope.ServiceProvider.GetRequiredService<IProjectService>();
    var form = new ProjectEditForm(projectService, _authenticatedUser);
    form.LoadProject(_selectedProject);
    if (form.ShowDialog(this) == DialogResult.OK)
    {
        await RefreshProjects(_txtSearch.Text);
        await _statsPanel.RefreshStatistics();
    }
}
```

**YEN? (Yap??t?r):**
```csharp
private async Task EditProject_Click()
{
    if (_selectedProject == null) return;
    using var scope = _serviceProvider.CreateScope();
    var form = scope.ServiceProvider.GetRequiredService<ProjectEditForm>();
    form.LoadProject(_selectedProject);
    if (form.ShowDialog(this) == DialogResult.OK)
    {
        await RefreshProjects(_txtSearch.Text);
        await _statsPanel.RefreshStatistics();
    }
}
```

---

## ?? NE DE???T??

**Silinen sat?rlar:**
```csharp
var projectService = scope.ServiceProvider.GetRequiredService<IProjectService>();
var form = new ProjectEditForm(projectService, _authenticatedUser);
```

**Eklenen sat?r:**
```csharp
var form = scope.ServiceProvider.GetRequiredService<ProjectEditForm>();
```

**Neden?**  
ProjectEditForm art?k DI (Dependency Injection) kullan?yor. Tüm parametreleri otomatik al?yor.

---

## ?? HIZLI YAPMA

1. **MainForm.cs**'i aç
2. **Ctrl+F** ile ara: `new ProjectEditForm`
3. **2 sonuç** bulunacak
4. Her ikisinde de:
   - Üstteki 2 sat?r? sil
   - Tek sat?r ekle (yukar?daki gibi)
5. **Ctrl+S** kaydet
6. **Build et:**
   ```sh
   dotnet build Performance.sln
   ```

---

## ? BA?ARI KONTROLÜ

Build sonucu:
```
Build succeeded with 21 warning(s)
0 Error(s)
```

---

## ?? TAMAMLANDI!

**Yap?lmas? gereken:** 2 metod, 4 sat?r silme + 2 sat?r ekleme  
**Süre:** ~2 dakika  
**Sonuç:** Tüm AI özellikleri çal??acak! ??
