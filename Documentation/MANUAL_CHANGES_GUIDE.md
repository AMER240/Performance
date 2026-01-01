# ?? Manual Code Changes Required

## Overview
This document outlines the manual code changes needed to complete the AI Enhancement update. Some files were open in the editor and could not be automatically modified.

---

## 1?? UserProfileForm.cs - Add Sector Field

### **File:** `Performance/UserProfileForm.cs`

### **Step 1: Add Sector TextBox field**
**Location:** Around line 19 (field declarations)

```csharp
// BEFORE:
private TextBox _txtUsername = null!;
private TextBox _txtEmail = null!;
private TextBox _txtFullName = null!;
private PictureBox _picProfile = null!;
private DataGridView _gridMyTasks = null!;

// AFTER:
private TextBox _txtUsername = null!;
private TextBox _txtEmail = null!;
private TextBox _txtFullName = null!;
private TextBox _txtSector = null!;  // ? ADD THIS LINE
private PictureBox _picProfile = null!;
private DataGridView _gridMyTasks = null!;
```

---

### **Step 2: Increase content panel height**
**Location:** Inside `InitializeComponent()` method (~line 45)

```csharp
// BEFORE:
var contentPanel = new Panel()
{
    Left = 20,
    Top = 80,
    Width = 860,
    Height = 250,  // ? OLD
    BackColor = Color.White,
    BorderStyle = BorderStyle.FixedSingle
};

// AFTER:
var contentPanel = new Panel()
{
    Left = 20,
    Top = 80,
    Width = 860,
    Height = 290,  // ? CHANGED to 290
    BackColor = Color.White,
    BorderStyle = BorderStyle.FixedSingle
};
```

---

### **Step 3: Add Sector label and textbox**
**Location:** After `_txtFullName` setup, before `lblRole` (~line 130)

```csharp
// ADD THIS BLOCK:
var lblSector = new Label()
{
    Text = "Sector/Field:",
    Left = 200,
    Top = 160,
    Width = 120,
    ForeColor = UiColors.PrimaryText,
    Font = new Font("Segoe UI", 10F, FontStyle.Bold)
};

_txtSector = new TextBox()
{
    Left = 330,
    Top = 157,
    Width = 500
};
UiHelpers.StyleTextBox(_txtSector);
```

---

### **Step 4: Update Role label and textbox positions**
**Location:** Update existing `lblRole` and `txtRole` (~line 145)

```csharp
// BEFORE:
var lblRole = new Label()
{
    Text = "Role:",
    Left = 200,
    Top = 160,  // ? OLD
    Width = 120,
    ForeColor = UiColors.PrimaryText,
    Font = new Font("Segoe UI", 10F, FontStyle.Bold)
};

var txtRole = new TextBox()
{
    Left = 330,
    Top = 157,  // ? OLD
    Width = 200,
    ReadOnly = true,
    BackColor = UiColors.Lighten(UiColors.Background, 0.5f)
};

// AFTER:
var lblRole = new Label()
{
    Text = "Role:",
    Left = 200,
    Top = 205,  // ? CHANGED to 205
    Width = 120,
    ForeColor = UiColors.PrimaryText,
    Font = new Font("Segoe UI", 10F, FontStyle.Bold)
};

var txtRole = new TextBox()
{
    Left = 330,
    Top = 202,  // ? CHANGED to 202
    Width = 200,
    ReadOnly = true,
    BackColor = UiColors.Lighten(UiColors.Background, 0.5f)
};
```

---

### **Step 5: Add Sector to content panel controls**
**Location:** After adding `_txtFullName`, before `lblRole` (~line 165)

```csharp
// BEFORE:
contentPanel.Controls.Add(lblFullName);
contentPanel.Controls.Add(_txtFullName);
contentPanel.Controls.Add(lblRole);
contentPanel.Controls.Add(txtRole);

// AFTER:
contentPanel.Controls.Add(lblFullName);
contentPanel.Controls.Add(_txtFullName);
contentPanel.Controls.Add(lblSector);    // ? ADD THIS LINE
contentPanel.Controls.Add(_txtSector);   // ? ADD THIS LINE
contentPanel.Controls.Add(lblRole);
contentPanel.Controls.Add(txtRole);
```

---

### **Step 6: Adjust My Tasks section position**
**Location:** Around line 180

```csharp
// BEFORE:
var lblMyTasks = new Label()
{
    Text = "MY TASKS",
    Left = 20,
    Top = 350,  // ? OLD
    Font = new Font("Segoe UI", 12F, FontStyle.Bold),
    ForeColor = UiColors.PrimaryText,
    AutoSize = true
};

_gridMyTasks = new DataGridView()
{
    Left = 20,
    Top = 380,  // ? OLD
    Width = 860,
    Height = 240,  // ? OLD

// AFTER:
var lblMyTasks = new Label()
{
    Text = "MY TASKS",
    Left = 20,
    Top = 390,  // ? CHANGED to 390
    Font = new Font("Segoe UI", 12F, FontStyle.Bold),
    ForeColor = UiColors.PrimaryText,
    AutoSize = true
};

_gridMyTasks = new DataGridView()
{
    Left = 20,
    Top = 420,  // ? CHANGED to 420
    Width = 860,
    Height = 200,  // ? CHANGED to 200
```

---

### **Step 7: Increase form height**
**Location:** Start of `InitializeComponent()` (~line 37)

```csharp
// BEFORE:
this.ClientSize = new System.Drawing.Size(900, 700);

// AFTER:
this.ClientSize = new System.Drawing.Size(900, 740);  // ? CHANGED to 740
```

---

### **Step 8: Load Sector value**
**Location:** Load data section (~line 330)

```csharp
// BEFORE:
_txtUsername.Text = _user.UserName;
_txtEmail.Text = _user.Email ?? string.Empty;
_txtFullName.Text = _user.FullName ?? string.Empty;
txtRole.Text = _user.Role.ToString();

// AFTER:
_txtUsername.Text = _user.UserName;
_txtEmail.Text = _user.Email ?? string.Empty;
_txtFullName.Text = _user.FullName ?? string.Empty;
_txtSector.Text = _user.Sector ?? string.Empty;  // ? ADD THIS LINE
txtRole.Text = _user.Role.ToString();
```

---

### **Step 9: Save Sector value**
**Location:** In `SaveProfile_Click()` method (~line 460)

```csharp
// BEFORE:
_user.Email = _txtEmail.Text.Trim();
_user.FullName = _txtFullName.Text.Trim();

// AFTER:
_user.Email = _txtEmail.Text.Trim();
_user.FullName = _txtFullName.Text.Trim();
_user.Sector = _txtSector.Text.Trim();  // ? ADD THIS LINE
```

---

## 2?? ProjectEditForm.cs - Add AI Suggestion

### **File:** `Performance/ProjectEditForm.cs`

### **Step 1: Add IProjectSuggestionService dependency**
**Location:** Class fields and constructor (~line 12)

```csharp
// BEFORE:
public partial class ProjectEditForm : BaseForm
{
    private readonly IProjectService _projectService;
    private readonly UserEntity? _currentUser;
    private ProjectEntity? _project;

    private TextBox txtName = new TextBox();
    private TextBox txtDesc = new TextBox();
    private TextBox txtManagerNotes = new TextBox();

    public ProjectEditForm(IProjectService projectService, UserEntity? currentUser = null)
    {
        _projectService = projectService;
        _currentUser = currentUser;
        InitializeComponent();
    }

// AFTER:
public partial class ProjectEditForm : BaseForm
{
    private readonly IProjectService _projectService;
    private readonly IProjectSuggestionService _projectSuggestionService;  // ? ADD THIS
    private readonly UserEntity? _currentUser;
    private ProjectEntity? _project;

    private TextBox txtName = new TextBox();
    private TextBox txtDesc = new TextBox();
    private TextBox txtManagerNotes = new TextBox();

    public ProjectEditForm(IProjectService projectService, IProjectSuggestionService projectSuggestionService, UserEntity? currentUser = null)  // ? ADD PARAMETER
    {
        _projectService = projectService;
        _projectSuggestionService = projectSuggestionService;  // ? ADD THIS
        _currentUser = currentUser;
        InitializeComponent();
    }
```

---

### **Step 2: Increase form height**
**Location:** In `InitializeComponent()` (~line 35)

```csharp
// BEFORE:
bool isManager = _currentUser?.Role == UserRole.Manager;
int height = isManager ? 400 : 300;
this.ClientSize = new System.Drawing.Size(600, height);

// AFTER:
bool isManager = _currentUser?.Role == UserRole.Manager;
int height = isManager ? 650 : 550;  // ? INCREASED heights
this.ClientSize = new System.Drawing.Size(700, height);  // ? INCREASED width to 700
```

---

### **Step 3: Add AI Suggestion Panel (MODERN VERSION)**
**Location:** After description textbox, before Manager Notes (~line 105)

**?? NEW: Professional AI Panel with Colors & Categories**

See complete code in `MODERN_AI_PANEL_CODE.cs` for the full implementation.

**Key features:**
- ? Color-coded categories (Blue, Green, Orange, Purple, Teal)
- ? Progress bar with loading animation
- ? Scrollable results with auto-sizing
- ? Professional layout with section separators
- ? Bullet-formatted lists
- ? Error handling with visual feedback
- ? Empty state message

**Quick implementation:**
Copy the entire content from `MODERN_AI_PANEL_CODE.cs` and paste it after `txtDesc` setup.

**OR use the original simple version:**

```csharp
// ADD THIS ENTIRE BLOCK after txtDesc:

// AI Suggestion Panel
var aiPanel = new Panel()
{
    Left = 0,
    Top = 190,
    Width = 660,
    Height = 240,
    BackColor = UiColors.Lighten(UiColors.Info, 0.8f),
    BorderStyle = BorderStyle.FixedSingle
};

var lblAiTitle = new Label()
{
    Text = "*** AI PROJECT SUGGESTIONS ***",
    Left = 10,
    Top = 5,
    Width = 640,
    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
    ForeColor = UiColors.DarkGreen
};

var btnGetSuggestions = new Button()
{
    Text = "Get AI Suggestions",
    Left = 10,
    Top = 30,
    Width = 160,
    Height = 30
};
UiHelpers.ApplyButtonStyle(btnGetSuggestions);
btnGetSuggestions.BackColor = UiColors.Info;

var txtSuggestions = new TextBox()
{
    Left = 10,
    Top = 70,
    Width = 640,
    Height = 160,
    Multiline = true,
    ReadOnly = true,
    ScrollBars = ScrollBars.Vertical,
    Font = new Font("Segoe UI", 9F)
};
UiHelpers.StyleTextBox(txtSuggestions);
txtSuggestions.BackColor = Color.White;

btnGetSuggestions.Click += async (s, e) =>
{
    btnGetSuggestions.Enabled = false;
    btnGetSuggestions.Text = "Loading...";
    try
    {
        var result = await _projectSuggestionService.SuggestProjectDetailsAsync(
            txtName.Text,
            txtDesc.Text
        );

        var suggestions = new System.Text.StringBuilder();
        suggestions.AppendLine($"[*] SUGGESTED FEATURES:");
        suggestions.AppendLine(result.SuggestedFeatures);
        suggestions.AppendLine();
        suggestions.AppendLine($"[+] RECOMMENDED TASKS:");
        suggestions.AppendLine(result.RecommendedTasks);
        suggestions.AppendLine();
        suggestions.AppendLine($"[#] REQUIRED EMPLOYEE TYPES:");
        suggestions.AppendLine(result.RequiredEmployeeTypes);
        suggestions.AppendLine();
        suggestions.AppendLine($"[@] TEAM COMPOSITION:");
        suggestions.AppendLine(result.TeamComposition);
        suggestions.AppendLine();
        suggestions.AppendLine($"[!] {result.Explanation}");

        txtSuggestions.Text = suggestions.ToString();
    }
    catch (Exception ex)
    {
        txtSuggestions.Text = $"[X] Error: {ex.Message}";
    }
    finally
    {
        btnGetSuggestions.Enabled = true;
        btnGetSuggestions.Text = "Get AI Suggestions";
    }
};

aiPanel.Controls.Add(lblAiTitle);
aiPanel.Controls.Add(btnGetSuggestions);
aiPanel.Controls.Add(txtSuggestions);

mainPanel.Controls.Add(aiPanel);
```

---

### **Step 4: Adjust Manager Notes position**
**Location:** If Manager Notes section exists (~line 110)

```csharp
// Update currentTop from 190 to 440:
int currentTop = 440;  // ? CHANGED from 190

if (isManager)
{
    var lblNotes = new Label() 
    { 
        Text = "Manager Notes:", 
        Left = 0, 
        Top = currentTop,  // Now 440
        // ... rest of code
    };
```

---

## 3?? Apply Database Migration

### **Run this command in terminal:**

```bash
dotnet ef database update --startup-project Performance
```

This will apply the `AddUserSectorField` migration to add the `Sector` column to the Users table.

---

## 4?? Configure Gemini API Key

### **File:** `Performance/appsettings.json`

**Already updated automatically!** But verify it looks like this:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=PerformanceDb;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  },
  "GeminiAI": {
    "ApiKey": "YOUR_GEMINI_API_KEY_HERE",
    "Model": "gemini-1.5-flash"
  }
}
```

### **?? Get Your Free Gemini API Key:**

1. Visit: https://makersuite.google.com/app/apikey
2. Sign in with Google account
3. Click "Create API Key"
4. Copy the key
5. Replace `YOUR_GEMINI_API_KEY_HERE` with your actual key

---

## 5?? Using Forms with New Services

### **TaskEditForm Usage** (No changes needed)
Already configured to use `ITaskSuggestionService` - it will automatically use `GeminiTaskSuggestionService` now!

### **ProjectEditForm Usage** (Update instantiation)

**Find where ProjectEditForm is created** (likely in `MainForm.cs`):

```csharp
// BEFORE:
var form = new ProjectEditForm(_projectService, _currentUser);

// AFTER:
var form = new ProjectEditForm(_projectService, _projectSuggestionService, _currentUser);
```

Or use DI if available:

```csharp
// If using dependency injection:
var form = serviceProvider.GetRequiredService<ProjectEditForm>();
```

---

## ? Verification Checklist

- [ ] UserProfileForm updated with all 9 changes
- [ ] ProjectEditForm updated with all 4 changes
- [ ] Database migration applied (`dotnet ef database update`)
- [ ] Gemini API key added to appsettings.json
- [ ] Project builds successfully (`dotnet build`)
- [ ] Test Sector field in User Profile
- [ ] Test AI Task Suggestions (should use Gemini now)
- [ ] Test AI Project Suggestions

---

## ?? Testing Guide

### **Test 1: User Profile Sector**
1. Login as manager or employee
2. Click "My Profile"
3. Enter a sector (e.g., "IT", "Marketing", "Finance")
4. Click "Save Changes"
5. Reopen profile - sector should be saved

### **Test 2: Task AI Suggestions**
1. Create/Edit a task
2. Enter a description
3. Click "AI Suggest"
4. Should see ? Gemini AI suggestions with priority, duration, user assignment

### **Test 3: Project AI Suggestions**
1. Create/Edit a project
2. Enter name and description
3. Click "Get AI Suggestions"
4. Should see:
   - Suggested Features
   - Recommended Tasks
   - Required Employee Types
   - Team Composition
   - Explanation

---

## ?? Troubleshooting

### **Error: "Gemini API key not configured"**
- Check appsettings.json has valid API key
- Ensure file is in Performance folder
- Restart application after changing config

### **Error: "Column 'Sector' does not exist"**
- Run: `dotnet ef database update --startup-project Performance`
- Check migration was applied: Look in database for Sector column in Users table

### **Error: "Could not load file HttpClient"**
- Already handled - using built-in `System.Net.Http.HttpClient`
- No additional packages needed

---

## ?? Summary of Changes

### **New Files Created:**
1. `Performance/Application/Services/GeminiTaskSuggestionService.cs` ?
2. `Performance/Application/Services/IProjectSuggestionService.cs` ?
3. `Performance/Application/Services/GeminiProjectSuggestionService.cs` ?
4. `Performance.Infrastructure/Migrations/20241230000000_AddUserSectorField.cs` ?

### **Modified Files:**
1. `Performance/appsettings.json` ? (Gemini config)
2. `Performance.Infrastructure/Entities/UserEntity.cs` ? (Sector field)
3. `Performance.Infrastructure/Migrations/PerformanceDbContextModelSnapshot.cs` ?
4. `Performance/Program.cs` ? (Service registration)
5. `Performance/UserProfileForm.cs` ?? (MANUAL CHANGES NEEDED)
6. `Performance/ProjectEditForm.cs` ?? (MANUAL CHANGES NEEDED)

---

**Ready to implement! ??**
