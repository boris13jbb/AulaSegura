# 🚀 AulaSegura - Quick Start Guide (WPF Application)

## Running the Desktop Application

### Prerequisites
- ✅ .NET 8 SDK installed
- ✅ Windows OS (10 or later recommended)
- ✅ Administrator privileges (for service installation)

---

## Option 1: Run from Visual Studio (Recommended for Development)

1. **Open Solution**
   ```
   File → Open → Project/Solution
   Select: AulaSegura.sln
   ```

2. **Set Startup Project**
   - Right-click on `AulaSegura.App` in Solution Explorer
   - Select "Set as Startup Project"

3. **Run Application**
   - Press `F5` or click "Start Debugging"
   - Or press `Ctrl+F5` to run without debugging

4. **Login Credentials**
   ```
   Username: admin
   Password: Admin@123
   ```

---

## Option 2: Run from Command Line

### Step 1: Navigate to Project Directory
```powershell
cd "d:\APKICATIVO DE CONTROL DE PAGINAS\AulaSegura\src\AulaSegura.App"
```

### Step 2: Build the Application
```powershell
dotnet build
```

### Step 3: Run the Application
```powershell
dotnet run
```

The login window should appear automatically.

---

## Option 3: Run Published Executable

### Step 1: Publish the Application
```powershell
cd "d:\APKICATIVO DE CONTROL DE PAGINAS\AulaSegura"
dotnet publish src/AulaSegura.App/AulaSegura.App.csproj -c Release -o publish/app
```

### Step 2: Run the Executable
```powershell
.\publish\app\AulaSegura.App.exe
```

---

## Testing the Login

### Valid Login
1. Enter username: `admin`
2. Enter password: `Admin@123`
3. Click "Iniciar Sesión" button
4. Should see dashboard with success message

### Invalid Login
1. Enter wrong credentials
2. Click "Iniciar Sesión"
3. Should see error message in red
4. Password field should be cleared

### Account Lockout Test
1. Attempt login with wrong password 5 times
2. On 6th attempt, should see lockout message
3. Must wait 30 minutes or reset manually

---

## Troubleshooting

### Issue: Database Not Found
**Error:** "SQLite database file not found"

**Solution:**
```powershell
# Ensure Service project has initialized database first
cd ..\AulaSegura.Service
dotnet run
# Let it run for 10 seconds to create database
# Then stop with Ctrl+C
# Now run App again
```

### Issue: Dependency Injection Error
**Error:** "Unable to resolve service for type..."

**Solution:**
```powershell
# Clean and rebuild solution
cd ..\..
dotnet clean
dotnet build
```

### Issue: Port/Service Conflict
**Error:** "Cannot access database file"

**Solution:**
- Ensure Windows Service is NOT running
- Stop service: `Stop-Service AulaSeguraService`
- Or close any other instance of the app

### Issue: UI Not Showing
**Error:** Application starts but no window appears

**Solution:**
- Check Task Manager for hidden processes
- Kill all `AulaSegura.App.exe` processes
- Rebuild and run again

---

## Understanding the Architecture

### Application Flow
```
App.xaml.cs (Startup)
    ↓
Initialize Database (seed data if needed)
    ↓
Show LoginView
    ↓
User enters credentials
    ↓
LoginViewModel validates input
    ↓
AuthService verifies credentials
    ↓
IF valid → Show MainWindow (Dashboard)
IF invalid → Show error message
```

### Key Components

#### ViewModels (`src/AulaSegura.App/ViewModels/`)
- `LoginViewModel.cs` - Handles login logic
- Future: DashboardViewModel, SettingsViewModel, etc.

#### Views (`src/AulaSegura.App/Views/`)
- `LoginView.xaml` - Login screen UI
- Future: DashboardView, SettingsView, etc.

#### Converters (`src/AulaSegura.App/Converters/`)
- `ValueConverters.cs` - UI binding helpers

---

## Development Tips

### Hot Reload
When running with debugging:
- Make XAML changes
- Save file
- Changes apply immediately (no restart needed)

### Debugging ViewModels
1. Set breakpoints in LoginViewModel.cs
2. Run with F5 (debug mode)
3. Inspect variables during execution

### Viewing Logs
Application logs are NOT configured yet (TODO).
For now, check Service logs at:
```
src\AulaSegura.Service\Logs\aulasegura-YYYYMMDD.log
```

---

## Next Steps After Login

Currently, after successful login you'll see:
```
✅ Login Exitoso
La interfaz gráfica está en desarrollo.
El servicio de bloqueo está activo en segundo plano.
```

This is a **placeholder**. Future phases will add:
- Dashboard with statistics
- Blocked sites management
- Allowed sites management
- Categories management
- Schedules configuration
- Activity logs viewer
- Settings page
- Backup/restore functionality

---

## Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| `Tab` | Move to next field |
| `Shift+Tab` | Move to previous field |
| `Enter` | Submit login form |
| `Esc` | Close window (if implemented) |

---

## Security Notes

### Password Storage
- Passwords are NEVER stored in plain text
- BCrypt hashing with work factor 11
- Hashed passwords stored in SQLite database

### Session Management
- No session tokens currently (desktop app)
- User identity passed via constructor to MainWindow
- Future: Implement proper session management

### Input Validation
- All inputs validated before processing
- SQL injection prevented by EF Core parameterization
- XSS not applicable (desktop app, not web)

---

## Support

### Documentation
- `docs/PROGRESS_REPORT_PHASE3.md` - Detailed implementation report
- `docs/ESTADO_DESARROLLO.md` - Overall development status
- `QUICK_START.md` - General quick start guide

### Common Issues
See troubleshooting section above.

### Contact
Refer to project documentation for support contacts.

---

## Summary

✅ **What Works:**
- Login screen displays correctly
- Authentication integrates with backend
- Database initializes automatically
- Navigation to dashboard works
- Error handling functional

⏳ **What's Pending:**
- Complete dashboard UI
- CRUD screens for all entities
- Settings page
- Reports and analytics
- Advanced features

🎯 **Current Status:**
The WPF application shell is complete and functional. The foundation is solid for building out the remaining UI components.

---

**Last Updated:** April 24, 2026  
**Version:** 1.0.0-alpha  
**Status:** Development Phase 3 Complete
