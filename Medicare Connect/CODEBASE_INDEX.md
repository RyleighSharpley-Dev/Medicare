# Medicare Connect — Codebase Index

## Overview
- ASP.NET Core MVC with Areas, Identity, and EF Core (SQL Server)
- Entry point: `Program.cs`
- Data context: `Data/ApplicationDbContext.cs` (inherits `IdentityDbContext`)
- Identity & seeding: `Data/IdentityDataSeeder.cs` (roles + demo users)

## Routing
- Area route: `{area:exists}/{controller=Dashboard}/{action=Index}/{id?}`
- Default route: `{controller=Home}/{action=Index}/{id?}`
- Razor Pages mapped for Identity (`/Areas/Identity/Pages`)

## Controllers and Actions

### Root (no area)
- `Controllers/HomeController`
  - `Index()`
  - `Privacy()`
  - `Error()`
- `Controllers/RoleRedirectController`
  - `RedirectToRoleDashboard()` — redirects user to the appropriate area dashboard based on role claims

### Area: Doctors
- `Areas/Doctors/Controllers/DashboardController`
  - `Index()`
- `Areas/Doctors/Controllers/AppointmentsController`
  - `Index()`
  - `Complete(Guid id)`
  - `Cancel(Guid id)`
- `Areas/Doctors/Controllers/PatientsController`
  - `Index()`

### Area: Patients
- `Areas/Patients/Controllers/DashboardController`
  - `Index()`
- `Areas/Patients/Controllers/AppointmentsController`
  - `Index()`
  - `Book(AppointmentCreateModel model)`
  - `Cancel(Guid id)`
- `Areas/Patients/Controllers/PaymentsController`
  - `Index()`
- `Areas/Patients/Controllers/PrescriptionsController`
  - `Index()`
- `Areas/Patients/Controllers/ProfileController`
  - `Index()` (GET)
  - `Index(PatientProfileViewModel model)` (POST, async)
  - `RemoveProfilePhoto()`
- `Areas/Patients/Controllers/RecordsController`
  - `Index()`
- `Areas/Patients/Controllers/RemindersController`
  - `Index()`
- `Areas/Patients/Controllers/SettingsController`
  - `Index()`

### Area: Nurses
- `Areas/Nurses/Controllers/DashboardController`
  - `Index()`

### Area: Pharmacists
- `Areas/Pharmacists/Controllers/DashboardController`
  - `Index()`

### Area: AdministrativeStaff
- `Areas/AdministrativeStaff/Controllers/DashboardController`
  - `Index()`

### Area: Managers
- `Areas/Managers/Controllers/DashboardController`
  - `Index()`

## Identity (Razor Pages)
- `Areas/Identity/Pages/Account/Login.cshtml.cs`
  - `OnPostAsync(string? returnUrl = null)`
- `Areas/Identity/Pages/Account/Register.cshtml.cs`
  - `OnPostAsync(string? returnUrl = null)`

## Data Layer
- `Data/ApplicationDbContext.cs`
  - Inherits `IdentityDbContext`
  - No additional `DbSet<>` declarations yet
- `Data/IdentityDataSeeder.cs`
  - Roles created with responsibility and region claims:
    - `Patients`, `Doctors`, `Nurses`, `Pharmacists`, `Administrative Staff`, `Managers/Admin`
  - Seed users (email → role):
    - `sibusiso@medicare.com` → Patients
    - `thandeka@medicare.com` → Doctors
    - `nomvula@medicare.com` → Nurses
    - `siyabonga@medicare.com` → Pharmacists
    - `zandile@medicare.com` → Administrative Staff
    - `thembinkosi@medicare.com` → Managers/Admin

## Models (by area)
- `Areas/Patients/Models`
  - `Appointments.cs`
  - `PatientProfileViewModel.cs`
  - `ProfilePhotoViewModel.cs`
- `Areas/Doctors/Models`
  - `DoctorPatientViewModel.cs`

## Views (key layout/navigation)
- Root: `Views/Shared/_Layout.cshtml`, `Views/Shared/_RoleBasedLayout.cshtml`, `Views/Shared/_LoginPartial.cshtml`
- Area layouts/nav:
  - Patients: `Areas/Patients/Views/Shared/_Layout.cshtml`, `_PatientNav.cshtml`
  - Doctors: `Areas/Doctors/Views/Shared/_Layout.cshtml`, `_DoctorNav.cshtml`
  - Nurses: `Areas/Nurses/Views/Shared/_Layout.cshtml`, `_NurseNav.cshtml`
  - Pharmacists: `Areas/Pharmacists/Views/Shared/_Layout.cshtml`, `_PharmacistNav.cshtml`
  - Managers: `Areas/Managers/Views/Shared/_Layout.cshtml`, `_ManagerNav.cshtml`
  - Administrative Staff: `Areas/AdministrativeStaff/Views/Shared/_Layout.cshtml`, `_AdminStaffNav.cshtml`

## Entry Point and Middleware
- `Program.cs`
  - EF Core SQL Server, Identity (with roles), MVC, Razor Pages
  - Developer exception pages in Development
  - HSTS in Production, HTTPS redirection, static files
  - Routing → Authentication → Authorization
  - Area and default routes mapped
  - Identity seeding run at startup

## Documentation
- Roles and capability status: `medical_center_roles.md` 