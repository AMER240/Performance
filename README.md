# Performance - Project Management System

A comprehensive project and task management system built with .NET 8, Windows Forms, and DevExpress components.

---

## ?? Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Features](#features)
- [User Roles](#user-roles)
- [Database Schema](#database-schema)
- [Getting Started](#getting-started)
- [Project Structure](#project-structure)
- [UI Components](#ui-components)
- [AI Features](#ai-features)
- [Recent Updates](#recent-updates)
- [Git Commits](#git-commits)

---

## ?? Overview

**Performance** is a desktop application for managing projects, tasks, and team members with role-based access control, AI-powered task suggestions, and a modern dark-themed UI.

**Tech Stack:**
- .NET 8
- Windows Forms + DevExpress XtraForms
- Entity Framework Core 8.0.0
- SQL Server (LocalDB)
- Clean Architecture Pattern

---

## ??? Architecture

```
Performance/
??? Performance/                    # Main UI Project (Windows Forms)
?   ??? LoginForm.cs               # User authentication
?   ??? MainForm.cs                # Dashboard with project gallery
?   ??? TaskListForm.cs            # Task management
?   ??? TaskEditForm.cs            # Task CRUD with AI suggestions
?   ??? TaskDetailForm.cs          # Task details view
?   ??? ProjectEditForm.cs         # Project CRUD
?   ??? UserManagementForm.cs      # User administration (Manager only)
?   ??? UserProfileForm.cs         # User profile + My Tasks
?   ??? DashboardStatsPanel.cs     # Statistics cards
?   ??? UI/                        # UI Helpers
?       ??? UiColors.cs            # Color palette
?       ??? UiHelpers.cs           # Styling utilities
?       ??? BaseForm.cs            # Base form class
?
??? Performance.Application/        # Business Logic Layer
?   ??? Services/
?       ??? IProjectService.cs     
?       ??? ProjectService.cs
?       ??? ITaskService.cs
?       ??? TaskService.cs
?       ??? IUserService.cs
?       ??? UserService.cs
?       ??? ITaskSuggestionService.cs
?       ??? EnhancedTaskSuggestionService.cs  # AI Engine
?
??? Performance.Infrastructure/     # Data Access Layer
    ??? Application/
    ?   ??? PerformanceDbContext.cs
    ??? Entities/
    ?   ??? ProjectEntity.cs
    ?   ??? TaskEntity.cs
    ?   ??? UserEntity.cs
    ??? Migrations/
        ??? 20251226222927_InitialCreate.cs
        ??? 20251228220216_AddUserProfileFields.cs
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
- .NET 8 SDK
- Visual Studio 2022
- SQL Server LocalDB
- DevExpress WinForms components

### Installation

1. **Clone the repository:**
```bash
git clone https://github.com/AMER240/Performance.git
cd Performance
```

2. **Restore NuGet packages:**
```bash
dotnet restore
```

3. **Update connection string** (if needed):
   - File: `Performance/appsettings.json`
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=PerformanceDb;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
     }
   }
   ```

4. **Apply migrations:**
```bash
dotnet ef database update --project Performance.Infrastructure --startup-project Performance
```

5. **Run the application:**
```bash
dotnet run --project Performance
```

---

## ?? Project Structure

### **Performance (UI)**
- **Forms:** All Windows Forms (Login, Main, Task, Project, User)
- **UI Folder:** Styling helpers, colors, base forms
- **Program.cs:** Entry point with DI configuration

### **Performance.Application (Business Logic)**
- **Services:** Business logic interfaces and implementations
- **IProjectService, ITaskService, IUserService**
- **ITaskSuggestionService:** AI suggestion engine

### **Performance.Infrastructure (Data Access)**
- **Entities:** Database models (Project, Task, User)
- **PerformanceDbContext:** EF Core DbContext
- **Migrations:** Database schema versions

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

### **EnhancedTaskSuggestionService**

**AI Engine Type:** On-Premise Machine Learning (No external API)

#### **Techniques Used:**
1. **Jaccard Similarity Algorithm**
   - Word-based text comparison
   - Minimum 30% similarity threshold
   - Analyzes top 100 completed tasks

2. **Historical Data Analysis**
   - Learns from completed tasks
   - Pattern recognition
   - User performance tracking

3. **Natural Language Processing (NLP)**
   - Keyword detection (urgent, critical, research, bug)
   - Text preprocessing & tokenization
   - Context understanding

4. **Statistical Analysis**
   - Average duration calculation
   - Priority trend analysis
   - User assignment optimization

#### **Features:**
- ? **Priority Suggestion** (Low, Medium, High)
- ? **Duration Estimation** (hours)
- ? **User Assignment** (best fit based on history)
- ? **Explanation** (reasoning behind suggestions)

#### **Algorithm Flow:**
```
1. Get task description
    ?
2. Fetch last 100 completed tasks
    ?
3. Calculate similarity (Jaccard)
    ?
4. Find top 10 similar tasks
    ?
5. Analyze patterns:
   - Average priority
   - Average duration
   - Successful assignees
    ?
6. Apply keyword rules:
   - "urgent" ? High priority
   - "research" ? Low priority, 2x duration
   - "bug" ? High priority
    ?
7. Return suggestion + explanation
```

---

## ?? Recent Updates

### **Latest Commits (Last 10)**

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

#### **a83bbcc** - Dashboard Icon Update
- ? Unicode emojis: ?? ?? ? ? ??

#### **ee4f611** - Refactor Password Management
- ? Remove "Change Password" from UserManagementForm
- ? Available in MainForm for all users

#### **2960ace** - Dashboard + Change Password
- ? Fix dashboard emoji display
- ? Add "Change Password" button for all users

#### **6231297** - UI Improvements
- ? Remove default credentials from LoginForm
- ? Hide ID columns in grids

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

## ?? Known Issues

None currently! All features tested and working.

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

**Last Updated:** December 28, 2024  
**Version:** 1.0.0  
**Build:** ? Successful  
**Status:** ?? Production Ready

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

### Build Commands
```bash
# Restore packages
dotnet restore

# Build solution
dotnet build Performance.sln

# Run application
dotnet run --project Performance

# Create migration
dotnet ef migrations add <MigrationName> --project Performance.Infrastructure --startup-project Performance

# Update database
dotnet ef database update --project Performance.Infrastructure --startup-project Performance

# Remove last migration
dotnet ef migrations remove --project Performance.Infrastructure --startup-project Performance
```

---

**?? Happy Coding!**
