# AulaSegura Control Web - Quick Start Guide

## 🚀 Getting Started in 5 Minutes

---

## Prerequisites

- ✅ Windows 10/11 (64-bit)
- ✅ .NET 8.0 SDK or Runtime
- ✅ Administrator privileges (for service installation)
- ✅ PowerShell (included with Windows)

---

## Option 1: Quick Test (Development Mode)

### Run Service in Console Mode (No Installation)

```bash
# Navigate to project root
cd "d:\APKICATIVO DE CONTROL DE PAGINAS\AulaSegura"

# Build the project
dotnet build --configuration Debug

# Run the service in console mode (for testing)
cd src/AulaSegura.Service
dotnet run
```

**What happens:**
- ✅ Database created automatically (`aulasegura.db`)
- ✅ Default admin account created
- ✅ Categories seeded
- ✅ Blocking rules applied to hosts file
- ✅ Service runs until you press Ctrl+C

**Default Credentials:**
- Username: `admin`
- Password: `SeedAdmin__Password o first-run-admin.txt`

**Logs Location:**
- Console output (real-time)
- `src/AulaSegura.Service/Logs/aulasegura-YYYYMMDD.log`

---

## Option 2: Production Installation

### Install as Windows Service

```powershell
# 1. Open PowerShell AS ADMINISTRATOR
#    (Right-click → "Run as Administrator")

# 2. Navigate to scripts directory
cd "d:\APKICATIVO DE CONTROL DE PAGINAS\AulaSegura\scripts"

# 3. Build the project first
cd ..
dotnet build --configuration Release

# 4. Install the service
cd scripts
.\install-service.ps1
```

**The script will:**
- ✅ Verify administrator privileges
- ✅ Create necessary directories
- ✅ Install Windows Service
- ✅ Configure automatic recovery
- ✅ Start the service
- ✅ Display installation summary

### Verify Installation

```powershell
# Check service status
Get-Service -Name AulaSeguraService

# Expected output:
# Status   Name               DisplayName
# ------   ----               -----------
# Running  AulaSeguraService  AulaSegura Control Web Service

# View service logs
Get-Content ..\src\AulaSegura.Service\Logs\aulasegura-*.log -Tail 50
```

### Manage the Service

```powershell
# Stop service
Stop-Service -Name AulaSeguraService

# Start service
Start-Service -Name AulaSeguraService

# Restart service
Restart-Service -Name AulaSeguraService

# View detailed status
Get-Service -Name AulaSeguraService | Format-List *
```

### Uninstall Service

```powershell
# Run as Administrator
cd "d:\APKICATIVO DE CONTROL DE PAGINAS\AulaSegura\scripts"
.\uninstall-service.ps1

# To also remove all data:
.\uninstall-service.ps1 -RemoveData
```

---

## 📁 Project Structure Overview

```
AulaSegura/
│
├── src/
│   ├── AulaSegura.Core/              # Business logic, entities, interfaces
│   ├── AulaSegura.Infrastructure/    # Database, repositories, services
│   ├── AulaSegura.Service/           # Windows Service (BlockingWorker)
│   ├── AulaSegura.App/               # WPF Application (pending)
│   └── AulaSegura.Shared/            # Shared utilities
│
├── scripts/
│   ├── install-service.ps1           # Installation script
│   └── uninstall-service.ps1         # Uninstallation script
│
├── docs/
│   ├── README.md                     # Project overview
│   ├── PROGRESS_REPORT_PHASE2.md     # This phase report
│   └── ...                           # Other documentation
│
└── AulaSegura.sln                    # Visual Studio solution
```

---

## 🔍 Troubleshooting

### Service Won't Start

**Check Event Viewer:**
```powershell
# View application logs
Get-EventLog -LogName Application -Newest 20 | Where-Object {$_.Source -like "*AulaSegura*"}
```

**Common Issues:**
1. **Permission Denied**: Ensure running as Administrator
2. **Port Conflict**: Not applicable (no network ports used)
3. **Database Lock**: Delete `aulasegura.db` and restart

### Hosts File Not Modified

**Check Permissions:**
```powershell
# Verify hosts file exists
Test-Path C:\Windows\System32\drivers\etc\hosts

# Check if service can write (should run as SYSTEM)
Get-Service -Name AulaSeguraService | Select-Object *
```

**Manual Test:**
```powershell
# Try writing to hosts file (as Administrator)
Add-Content -Path C:\Windows\System32\drivers\etc\hosts -Value "# Test"
# If this fails, check antivirus or group policies
```

### Database Errors

**Reset Database:**
```bash
# Stop service first
Stop-Service -Name AulaSeguraService

# Delete database file
Remove-Item "src\AulaSegura.Service\Data\aulasegura.db" -Force

# Restart service (will recreate database)
Start-Service -Name AulaSeguraService
```

### View Detailed Logs

```powershell
# Real-time log monitoring
Get-Content "src\AulaSegura.Service\Logs\aulasegura-*.log" -Wait -Tail 50

# Search for errors
Select-String -Path "src\AulaSegura.Service\Logs\aulasegura-*.log" -Pattern "ERROR"

# Today's logs only
Get-Content "src\AulaSegura.Service\Logs\aulasegura-$(Get-Date -Format 'yyyyMMdd').log"
```

---

## 🧪 Testing the Blocking Functionality

### Test 1: Add a Blocked Site

**Method 1: Direct Database Insert (Quick Test)**
```sql
-- Use SQLite browser or command line
INSERT INTO BlockedSites (Domain, CategoryId, Reason, IsActive, CreatedAt)
VALUES ('test-block.com', 1, 'Testing', 1, datetime('now'));
```

**Method 2: Via API (When WPF app is ready)**
- Use the WPF application to add blocked sites

### Test 2: Verify Hosts File Modification

```powershell
# Wait up to 60 seconds for service to apply rules
Start-Sleep -Seconds 60

# Check hosts file
Get-Content C:\Windows\System32\drivers\etc\hosts | Select-String "AULASEGURA"

# Expected output:
# 127.0.0.1       test-block.com # AULASEGURA BLOCKED
```

### Test 3: Verify DNS Blocking

```powershell
# Try to resolve blocked domain
nslookup test-block.com

# Expected: Should resolve to 127.0.0.1 (localhost)
# This effectively blocks access to the site
```

### Test 4: Check Service Logs

```powershell
# Look for rule application logs
Get-Content "src\AulaSegura.Service\Logs\*.log" | Select-String "Reglas de bloqueo"

# Expected:
# [Timestamp] [INFO] Reglas de bloqueo verificadas y aplicadas
```

---

## 📊 Monitoring & Maintenance

### Check Service Health

```powershell
# Create a health check script
$service = Get-Service -Name AulaSeguraService
Write-Host "Service Status: $($service.Status)"
Write-Host "Start Type: $($service.StartType)"

# Check if database exists
$dbPath = "src\AulaSegura.Service\Data\aulasegura.db"
if (Test-Path $dbPath) {
    $dbSize = (Get-Item $dbPath).Length / 1KB
    Write-Host "Database Size: $([math]::Round($dbSize, 2)) KB"
} else {
    Write-Host "Database: Not found"
}

# Check recent logs
$logPath = "src\AulaSegura.Service\Logs"
$recentErrors = Get-ChildItem "$logPath\*.log" | 
    Select-String "ERROR" | 
    Select-Object -Last 5
    
if ($recentErrors) {
    Write-Host "Recent Errors:" -ForegroundColor Red
    $recentErrors | ForEach-Object { Write-Host $_ }
} else {
    Write-Host "No recent errors" -ForegroundColor Green
}
```

### Backup Configuration

```powershell
# Manual backup
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$backupDir = "Backups\Manual"
New-Item -ItemType Directory -Path $backupDir -Force | Out-Null

# Copy database
Copy-Item "src\AulaSegura.Service\Data\aulasegura.db" "$backupDir\aulasegura_$timestamp.db"

# Copy hosts file backup
Copy-Item "src\AulaSegura.Service\Backups\hosts\*" "$backupDir\" -Recurse -ErrorAction SilentlyContinue

Write-Host "Backup created: $backupDir" -ForegroundColor Green
```

### Cleanup Old Logs

```powershell
# Remove logs older than 30 days
$cutoffDate = (Get-Date).AddDays(-30)
Get-ChildItem "src\AulaSegura.Service\Logs\*.log" | 
    Where-Object { $_.LastWriteTime -lt $cutoffDate } |
    Remove-Item -Force

Write-Host "Old logs cleaned up" -ForegroundColor Green
```

---

## 🎯 Next Steps: WPF Application Development

Once the service is running, the next phase is to build the WPF application for user interaction.

**Coming in Phase 3:**
- Login screen with authentication
- Dashboard with statistics
- Blocked sites management UI
- Category administration
- Schedule configuration
- Reports and export functionality

---

## 📞 Support & Resources

### Documentation
- [README.md](../README.md) - Project overview
- [PROGRESS_REPORT_PHASE2.md](./PROGRESS_REPORT_PHASE2.md) - Technical details
- [VERIFICACION_FINAL.md](./VERIFICACION_FINAL.md) - Verification checklist

### Useful Commands Reference

```powershell
# Service Management
Get-Service -Name AulaSeguraService
Start-Service -Name AulaSeguraService
Stop-Service -Name AulaSeguraService
Restart-Service -Name AulaSeguraService

# Log Monitoring
Get-Content "src\AulaSegura.Service\Logs\aulasegura-*.log" -Wait -Tail 50

# Database Location
ls src\AulaSegura.Service\Data\aulasegura.db

# Hosts File
notepad C:\Windows\System32\drivers\etc\hosts  # Run as Administrator
```

---

## ⚡ Quick Reference Card

| Task | Command |
|------|---------|
| **Build Project** | `dotnet build --configuration Release` |
| **Install Service** | `.\scripts\install-service.ps1` (as Admin) |
| **Uninstall Service** | `.\scripts\uninstall-service.ps1` (as Admin) |
| **Check Status** | `Get-Service -Name AulaSeguraService` |
| **View Logs** | `Get-Content src\...\Logs\*.log -Tail 50` |
| **Test Run** | `cd src\AulaSegura.Service && dotnet run` |
| **Default Login** | `admin` / `SeedAdmin__Password o first-run-admin.txt` |

---

**Last Updated:** April 24, 2026  
**Version:** 1.0.0-alpha  
**Status:** Backend Complete, WPF Pending
