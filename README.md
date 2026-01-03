# Performance - Project Management System

A comprehensive project and task management system built with **.NET 8**, **Windows Forms**, **Clean Architecture**, and **AI-powered suggestions**.

> **? Clean Architecture + AI Features - January 2026**

---

## ?? Quick Start

### ?? Important: Startup Project Setup

This solution uses **Clean Architecture** with multiple projects. To run the application:

#### Option 1: Visual Studio
1. Open `Performance.sln`
2. In **Solution Explorer**, right-click **`Performance`** project (under `src/Performance.UI`)
3. Select **"Set as Startup Project"**
4. Press **F5** to run

#### Option 2: Command Line (Recommended)
```powershell
# Using batch file
Run-Performance.bat

# Or manually
dotnet run --project src/Performance.UI
```

#### Option 3: Build + Run
```powershell
dotnet build Performance.sln
dotnet run --project src/Performance.UI --no-build
```

---

## ?? Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Features](#features)
- [AI Features](#ai-features)
- [User Roles](#user-roles)
- [Database Schema](#database-schema)
- [Getting Started](#getting-started)
- [Project Structure](#project-structure)
- [UI Components](#ui-components)
- [Configuration](#configuration)
- [Common Issues](#common-issues)
- [Documentation](#documentation)

---

## ?? Overview

**Performance** is a modern desktop application for managing projects, tasks, and team members with:
- ??? **Clean Architecture** - Fully separated concerns across 4 layers
- ?? **AI-Powered Suggestions** - Gemini AI for project planning & task suggestions
- ?? **Role-Based Access Control** - Manager/Employee permissions
- ?? **Modern UI** - Dark-themed interface with DevExpress components
- ?? **Real-Time Dashboard** - Statistics, upcoming deadlines, task tracking

**Tech Stack:**
- .NET 8
- Windows Forms + DevExpress XtraForms v25.1
- Entity Framework Core 8.0.0
- SQL Server (LocalDB)
- **Google Gemini AI 2.5 Flash** (Cloud-based NLP)
- Clean Architecture Pattern
- Repository Pattern
- Dependency Injection
- SHA-256 Password Hashing

---

## ??? Architecture

**Clean Architecture** with 4 distinct layers:

```
Performance.sln
?
??? src/
?   ??? Performance.Domain/          ?? Core Layer (No dependencies)
?   ?   ??? Entities/
?   ?   ?   ??? UserEntity.cs       (Pure POCO + Sector field)
?   ?   ?   ??? ProjectEntity.cs    (Pure POCO + ManagerNotes)
?   ?   ?   ??? TaskEntity.cs       (Pure POCO + Relations)
?   ?   ??? Enums/
?   ?       ??? UserRole.cs
?   ?       ??? TaskStatus.cs
?   ?       ??? TaskPriority.cs
?   ?       ??? ProjectStatus.cs
?   ?
?   ??? Performance.Application/     ? Business Logic Layer
?   ?   ??? Interfaces/
?   ?   ?   ??? IProjectService.cs
?   ?   ?   ??? ITaskService.cs
?   ?   ?   ??? IUserService.cs
?   ?   ?   ??? ITaskSuggestionService.cs
?   ?   ?   ??? IRepository<T>.cs
?   ?   ??? Services/
?   ?       ??? ProjectService.cs
?   ?       ??? TaskService.cs
?   ?       ??? UserService.cs
?   ?       ??? EnhancedTaskSuggestionService.cs
?   ?
?   ??? Performance.Infrastructure/  ? Data Access Layer
?   ?   ??? Data/
?   ?   ?   ??? PerformanceDbContext.cs
?   ?   ??? Repositories/
?   ?   ?   ??? Repository<T>.cs    (Generic)
?   ?   ?   ??? ProjectRepository.cs
?   ?   ?   ??? TaskRepository.cs
?   ?   ?   ??? UserRepository.cs
?   ?   ??? Migrations/
?   ?
?   ??? Performance.UI/              ? Presentation Layer
?       ??? Forms/
?       ?   ??? LoginForm.cs
?       ?   ??? MainForm.cs
?       ?   ??? TaskListForm.cs
?       ?   ??? ProjectEditForm.cs
?       ?   ??? UserManagementForm.cs
?       ?   ??? UserProfileForm.cs
?       ??? UI/
?       ?   ??? UiColors.cs
?       ?   ??? UiHelpers.cs
?       ??? Program.cs              (DI Container)
?       ??? appsettings.json
?
??? Documentation/
    ??? CLEAN_ARCHITECTURE_COMPLETION.md
    ??? CLEAN_ARCHITECTURE_STATUS.md
    ??? WORKSPACE_INFORMATION.md
```

### Dependency Flow
```
UI ? Infrastructure ? Application ? Domain
                                      ?
                              (No dependencies!)
```

---

## ? Features

### 1. **Dashboard**
- ?? Project gallery view with cards
- ?? Statistics panel:
  - Total Projects
  - Total Tasks
  - Completion percentage
  - Upcoming deadlines (next 7 days)
  - My assigned tasks
- ?? Real-time project search
- ?? Modern dark-themed UI

### 2. **Project Management**
- ? Create, Read, Update, Delete (CRUD)
- ?? Manager notes (Manager only)
- ??? Task organization per project

### 3. **Task Management**
- ? Full CRUD operations
- ?? Status tracking (Todo, In Progress, Done)
- ? Priority levels (Low, Medium, High)
- ?? Deadline management with countdown
- ?? Estimated duration
- ?? User assignment
- ?? **AI-powered suggestions** (priority, duration, user)
- ?? Manager notes (Manager only)
- ?? Advanced filtering (status, priority, search)
- ?? Progress visualization

### 4. **User Management** (Manager Only)
- ?? Add/Edit/Delete users
- ?? Role assignment (Manager/Employee)
- ??? **View user profiles** (NEW)
- ?? User task statistics

### 5. **User Profile**
- ?? Email & Full Name
- ??? Profile photo upload (Base64)
- ?? **My Tasks** view (all assigned tasks)
- ?? Change password
- ?? Personal task dashboard

### 6. **Security**
- ?? SHA256 password hashing
- ?? Role-based access control (RBAC)
- ?? Session management
- ?? Prevent self-deletion

---

## ?? User Roles

### **Manager** ??
- ? Full project management (Add, Edit, Delete)
- ? Full task management (Add, Edit, Delete)
- ? User management
- ? View all user profiles
- ? Add manager notes
- ? View task details
- ? Change own password
- ? Edit own profile

### **Employee** ?????
- ? View projects (read-only)
- ? View tasks (read-only)
- ? Change task status (Todo ? In Progress ? Done)
- ? View task details
- ? Change own password
- ? Edit own profile
- ? View own assigned tasks
- ? Cannot add/edit/delete projects
- ? Cannot add/edit/delete tasks
- ? Cannot access user management

---

## ??? Database Schema

### **Users Table**
```sql
CREATE TABLE Users (
    Id NVARCHAR(450) PRIMARY KEY,
    UserName NVARCHAR(MAX) NOT NULL,
    PasswordHash NVARCHAR(MAX) NOT NULL,
    Role INT NOT NULL,
    Email NVARCHAR(MAX) NULL,
    FullName NVARCHAR(MAX) NULL,
    ProfilePhoto NVARCHAR(MAX) NULL
);
```

### **Projects Table**
```sql
CREATE TABLE Projects (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(MAX) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    ManagerNotes NVARCHAR(MAX) NULL,
    CreatedAt DATETIME2 NOT NULL
);
```

### **Tasks Table**
```sql
CREATE TABLE Tasks (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ProjectId INT NOT NULL,
    Title NVARCHAR(MAX) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    Status NVARCHAR(MAX) NOT NULL,
    Priority NVARCHAR(MAX) NOT NULL,
    Deadline DATETIME2 NULL,
    EstimatedDuration TIME NULL,
    AssignedToUserId NVARCHAR(MAX) NULL,
    ManagerNotes NVARCHAR(MAX) NULL,
    FOREIGN KEY (ProjectId) REFERENCES Projects(Id) ON DELETE CASCADE
);
```

### **Default Users**
```csharp
// Manager
Username: manager
Password: manager123

// Employee
Username: employee
Password: employee123
```

---

## ?? Getting Started

### Prerequisites
- **.NET 8 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Visual Studio 2022** (recommended)
- **SQL Server LocalDB** (included with Visual Studio)
- **DevExpress WinForms** components (v25.1.5)

### Installation

1. **Clone the repository:**
```bash
git clone https://github.com/AMER240/Performance.git
cd Performance
```

2. **Restore NuGet packages:**
```bash
dotnet restore Performance.sln
```

3. **Update connection string** (if needed):
   - File: `src/Performance.UI/appsettings.json`
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=PerformanceDb;Trusted_Connection=True;..."
     }
   }
   ```

4. **Build solution:**
```bash
dotnet build Performance.sln
```

5. **Run the application:**
```bash
# Option 1: Batch file
Run-Performance.bat

# Option 2: Command line
dotnet run --project src/Performance.UI

# Option 3: Visual Studio
# Set Performance (UI) as Startup Project, then press F5
```

### Database Migrations

The application will **automatically apply migrations** on startup via `Program.cs`.

If you need manual control:

```bash
# Add migration
dotnet ef migrations add MigrationName \
  --project src/Performance.Infrastructure \
  --startup-project src/Performance.UI

# Update database
dotnet ef database update \
  --project src/Performance.Infrastructure \
  --startup-project src/Performance.UI

# Remove last migration
dotnet ef migrations remove \
  --project src/Performance.Infrastructure \
  --startup-project src/Performance.UI
```

---

## ?? Project Structure

### **Clean Architecture Layers**

#### 1. **Domain Layer** (Core - No Dependencies)
- **Entities:** `UserEntity`, `ProjectEntity`, `TaskEntity` (Pure POCOs)
- **Enums:** `UserRole`, `TaskStatus`, `TaskPriority`, `ProjectStatus`
- **No framework dependencies** - Can be used anywhere

#### 2. **Application Layer** (Business Logic)
- **Interfaces:** `IProjectService`, `ITaskService`, `IUserService`, `ITaskSuggestionService`
- **Services:** Business logic implementations
- **Repository Abstractions:** `IRepository<T>`, `IProjectRepository`, etc.
- **Depends on:** Domain only

#### 3. **Infrastructure Layer** (Data Access)
- **DbContext:** `PerformanceDbContext` with Fluent API configuration
- **Repositories:** Generic + specific repository implementations
- **Migrations:** EF Core database migrations
- **Depends on:** Domain + Application

#### 4. **UI Layer** (Presentation)
- **Forms:** All Windows Forms UI
- **Helpers:** `UiColors`, `UiHelpers`
- **DI Container:** Configured in `Program.cs`
- **Depends on:** All other layers

### **Benefits of Clean Architecture**

? **Testability** - Each layer can be tested independently  
? **Maintainability** - Changes are isolated to specific layers  
? **Flexibility** - Easy to swap implementations (e.g., SQL ? NoSQL)  
? **Domain Purity** - Core business logic has zero dependencies  
? **Scalability** - Can add Web API, Mobile app without changing domain  

---

## ?? UI Components

### **Color Palette (UiColors.cs)**
```csharp
// Main colors
DarkGreen:      #2C5F2D
MediumGreen:    #4A7C59
LightGreen:     #97BC62

// Status colors
StatusTodo:     #6C757D
StatusInProgress: #0D6EFD
StatusDone:     #198754

// Priority colors
PriorityHigh:   #DC3545
PriorityMedium: #FFC107
PriorityLow:    #6C757D

// UI colors
Background:     #F5F5F5
PrimaryText:    #212529
SecondaryText:  #6C757D
Success:        #198754
Warning:        #FFC107
Error:          #DC3545
Info:           #0DCAF0
```

### **Dashboard Icons**
- ?? Total Projects (Folder)
- ?? Total Tasks (Clipboard)
- ? Completed (Checkmark)
- ? Due Soon (Alarm)
- ?? My Tasks (Person)

---

## ?? AI Features

### **Google Gemini AI Integration** ?

**AI Engine Type:** Cloud-based Natural Language Processing (Gemini 2.5 Flash)

#### **Services:**

1. **GeminiTaskSuggestionService**
   - AI-powered task analysis
   - Priority prediction (Low/Medium/High)
   - Estimated duration calculation
   - Smart user assignment
   - Context-aware explanations

2. **GeminiProjectSuggestionService**
   - Project planning assistance
   - Feature suggestions
   - Recommended tasks breakdown
   - Required roles analysis
   - Team composition recommendations

#### **Features:**
- ?? **Smart Priority Detection** - Analyzes description to determine urgency
- ? **Duration Estimation** - Predicts hours needed based on task complexity
- ?? **User Assignment** - Recommends best team member based on role & sector
- ?? **Project Analysis** - Generates complete project breakdown
- ?? **Historical Learning** - Uses past completed tasks for better accuracy
- ?? **Natural Language Understanding** - Gemini AI processes task descriptions

#### **Configuration:**
```json
{
  "GeminiAI": {
    "ApiKey": "YOUR_GEMINI_API_KEY_HERE",
    "Model": "gemini-2.5-flash"
  }
}
```

#### **API Endpoints Used:**
- `https://generativelanguage.googleapis.com/v1/models/gemini-2.5-flash:generateContent`
- `https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent` (fallback)

#### **Example AI Workflow:**

**Task Suggestion:**
```
Input:
  Task: "Implement payment gateway with Stripe"
  
AI Analysis:
  ? Priority: High
  ? Duration: 12.0 hours
  ? Assigned: Backend Developer (John Doe)
  ? Reasoning: "Payment integration is critical and requires backend expertise"
```

**Project Suggestion:**
```
Input:
  Project: "Mobile Banking App"
  Description: "iOS and Android mobile banking"
  
AI Analysis:
  ? Features: User auth, account management, transfers, notifications
  ? Tasks: UI/UX design, API development, security implementation
  ? Roles: Mobile developer, backend engineer, security specialist
  ? Team: 1 PM, 2 mobile devs, 1 backend dev, 1 QA engineer
```

#### **How to Get API Key:**
1. Visit [Google AI Studio](https://aistudio.google.com/app/apikey)
2. Sign in with Google account
3. Create API key
4. Add to `appsettings.json`

**Note:** Gemini AI API is free for development with generous rate limits!

---

## ?? Recent Updates

### **Latest Features & Fixes**

#### **January 2026 - Major Updates**

**? AI Integration (Google Gemini)**
- ?? Implemented GeminiTaskSuggestionService for smart task analysis
- ?? Added GeminiProjectSuggestionService for project planning
- ?? Real-time AI suggestions with explanations
- ? API key configuration support
- ? Fallback endpoints for reliability

**? Navigation Property Fixes**
- ?? Fixed "Unknown Project" issue by adding `.Include()` to repository
- ?? Fixed "Unassigned" issue with proper user loading
- ?? TaskRepository now loads Project and AssignedToUser automatically
- ?? Eliminated N+1 query problems

**? UI/UX Improvements**
- ?? Added null-check for `_statsPanel.RefreshStatistics()` calls
- ?? Fixed ObjectDisposedException in search timer
- ?? Improved CancellationTokenSource handling with closure capture
- ?? Better disposed form checks
- ?? TaskListForm filter logic implemented (Status, Priority, Search)

**? Code Quality**
- ?? Removed excessive disposed checks
- ?? Simplified BtnEdit_Click handlers
- ?? Added proper exception handling
- ?? Improved async/await patterns
- ?? Fixed memory leaks in timer disposal

**? Bug Fixes**
- ? Fixed "Task with ID X not found" error
- ? Fixed "Connection is closed" error
- ? Fixed "Cannot access a disposed object" error
- ? Fixed ApplyFilters method missing implementation
- ? Fixed search box causing ObjectDisposedException

### **Previous Commits**

#### **796f97e** - Button Layout Fix (Employee)
- ? TaskListForm: Move "View Details" and "Refresh" buttons left for employees
- Position: Left: 15 and Left: 155

#### **2448657** - Complete Role-Based Access Control
- ? MainForm: Hide project management buttons for employees
- ? MainForm: Move employee buttons to left positions
- ? UserManagementForm: Add "View Profile" button for managers

#### **9ed611a** - Improve Role-Based Access
- ? TaskDetailForm: Show username instead of user ID
- ? TaskDetailForm: Hide edit button for employees
- ? TaskListForm: Hide Add/Edit/Delete buttons for employees

### **Database Migrations**

1. **InitialCreate (20251226222927)**
   - Projects, Tasks, Users tables
   - Foreign keys and indexes

2. **AddUserProfileFields (20251228220216)**
   - Email, FullName, ProfilePhoto columns
   - User profile support

---

## ?? Configuration

### **appsettings.json**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=PerformanceDb;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

### **Program.cs (Dependency Injection)**
```csharp
services.AddDbContext<PerformanceDbContext>(options => 
    options.UseSqlServer(connectionString));

services.AddScoped<IProjectService, ProjectService>();
services.AddScoped<ITaskService, TaskService>();
services.AddScoped<IUserService, UserService>();
services.AddScoped<ITaskSuggestionService, EnhancedTaskSuggestionService>();
```

---

## ?? Common Issues

### Issue: "A project with an Output Type of Class Library cannot be started directly"

**Cause:** You're trying to run a Class Library project (Domain, Application, or Infrastructure) instead of the UI project.

**Solution:**

#### Visual Studio:
1. Right-click **`Performance`** project (under `src/Performance.UI`) in Solution Explorer
2. Select **"Set as Startup Project"**
3. Press F5

#### Command Line:
```bash
dotnet run --project src/Performance.UI
```

#### Using Batch File:
```bash
Run-Performance.bat
```

### Build Warnings

The solution may show 21-22 nullable reference warnings (CS8602, CS8618). These are **not critical** and don't affect functionality. They're related to C# 8.0's nullable reference types feature.

---

## ?? Statistics

- **Total Files:** 25+ C# files
- **Total Lines of Code:** ~8,000+
- **Forms:** 9 main forms
- **Services:** 6 service interfaces + implementations
- **Entities:** 3 database models
- **Migrations:** 2 migrations
- **Commits:** 10+ commits
- **Build Status:** ? Successful

---

## ?? Future Enhancements (Potential)

- [ ] Export tasks to Excel/PDF
- [ ] Email notifications for deadlines
- [ ] Task comments/attachments
- [ ] Gantt chart view
- [ ] Calendar view
- [ ] Mobile app (Xamarin/MAUI)
- [ ] Web dashboard (Blazor)
- [ ] Team chat integration
- [ ] Time tracking
- [ ] Reporting dashboard

---

## ????? Developer Notes

### **Important Files to Know:**

1. **UiColors.cs** - All color definitions
2. **UiHelpers.cs** - Styling methods
3. **PerformanceDbContext.cs** - Database context
4. **EnhancedTaskSuggestionService.cs** - AI engine
5. **MainForm.cs** - Main dashboard
6. **TaskListForm.cs** - Task management
7. **UserProfileForm.cs** - User profile + My Tasks

### **Adding New Features:**

1. **New Entity:**
   - Add to `Performance.Infrastructure/Entities`
   - Update `PerformanceDbContext.cs`
   - Create migration: `dotnet ef migrations add <Name>`
   - Apply: `dotnet ef database update`

2. **New Form:**
   - Create in `Performance/`
   - Inherit from `BaseForm` or `XtraForm`
   - Use `UiHelpers` and `UiColors` for styling
   - Add to DI in `Program.cs` if needed

3. **New Service:**
   - Create interface in `Performance.Application/Services`
   - Implement service class
   - Register in `Program.cs` DI

---

## ?? Support

For issues or questions:
- GitHub Issues: https://github.com/AMER240/Performance/issues
- Email: (Add your email)

---

## ?? License

(Add your license here - MIT, Apache, etc.)

---

## ?? Acknowledgments

- DevExpress for WinForms components
- Microsoft for .NET Framework
- Entity Framework Core team

---

## ?? Deployment

For deployment instructions, see:
- **[DEPLOYMENT.md](DEPLOYMENT.md)** - Complete .exe creation guide
- **[SECURITY.md](SECURITY.md)** - Security best practices for API keys

### Quick Publish

```powershell
# Option 1: Using script (Recommended)
.\publish.bat

# Option 2: Manual dotnet CLI
dotnet publish src\Performance.UI\Performance.csproj ^
  --configuration Release ^
  --runtime win-x64 ^
  --self-contained true ^
  --output publish ^
  -p:PublishSingleFile=true
```

**Important:** The `publish/` directory is ignored by Git and will NOT be uploaded to GitHub. This directory may contain sensitive configuration files with API keys.

---

## ?? Security

- `appsettings.json` is **NOT tracked** by Git (contains API keys)
- Use `appsettings.example.json` as template
- See **[SECURITY.md](SECURITY.md)** for security best practices

---

**Last Updated:** January 2, 2026  
**Version:** 1.1.0 (Gemini AI Integration)  
**Build:** ? Successful  
**Status:** ?? Production Ready  
**Architecture:** ??? Clean Architecture Compliant

---

## ?? Additional Documentation

- **[Deployment Guide](DEPLOYMENT.md)** - .exe creation and publishing instructions
- **[Security Guide](SECURITY.md)** - API key security and best practices
- **[Clean Architecture Completion Report](Documentation/CLEAN_ARCHITECTURE_COMPLETION.md)** - Detailed implementation guide
- **[Clean Architecture Status](Documentation/CLEAN_ARCHITECTURE_STATUS.md)** - Quick status summary  
- **[Workspace Information](Documentation/WORKSPACE_INFORMATION.md)** - Project details

---

## Quick Reference

### Login Credentials
```
Manager:
  Username: manager
  Password: manager123

Employee:
  Username: employee
  Password: employee123
```

### Connection String
```
Server=localhost;Database=PerformanceDb;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=true
```

## ??? Build Commands

### Build Solution
```bash
dotnet build Performance.sln
```

### Run Application
```bash
# Option 1: Batch file (easiest)
Run-Performance.bat

# Option 2: Direct run
dotnet run --project src/Performance.UI

# Option 3: Build then run
dotnet build Performance.sln --configuration Release
dotnet run --project src/Performance.UI --no-build
```

### Database Migrations
```bash
# Add migration
dotnet ef migrations add <MigrationName> \
  --project src/Performance.Infrastructure \
  --startup-project src/Performance.UI

# Update database
dotnet ef database update \
  --project src/Performance.Infrastructure \
  --startup-project src/Performance.UI

# Remove last migration
dotnet ef migrations remove \
  --project src/Performance.Infrastructure \
  --startup-project src/Performance.UI
```

### Clean & Rebuild
```bash
dotnet clean Performance.sln
dotnet build Performance.sln --no-incremental
```

---

**?? Happy Coding!**
