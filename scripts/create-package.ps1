# ============================================================================
# Script de Empaquetado - AulaSegura Control Web
# ============================================================================
# Este script crea un paquete de instalación portable para distribuir
# en otras computadoras
# ============================================================================

param(
    [string]$OutputDir = ".\dist",
    [string]$Version = "1.0.0",
    [switch]$Clean = $false
)

# Configurar política de ejecución
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass -Force

# Función para escribir logs con colores
function Write-Log {
    param(
        [string]$Message,
        [string]$Level = "INFO"
    )
    
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    
    switch ($Level) {
        "INFO" { Write-Host "[$timestamp] [INFO] $Message" -ForegroundColor Green }
        "WARN" { Write-Host "[$timestamp] [WARN] $Message" -ForegroundColor Yellow }
        "ERROR" { Write-Host "[$timestamp] [ERROR] $Message" -ForegroundColor Red }
        "SUCCESS" { Write-Host "[$timestamp] [SUCCESS] $Message" -ForegroundColor Cyan }
    }
}

Write-Host "`n" 
Write-Host "========================================================================" -ForegroundColor Cyan
Write-Host "         EMPAQUETADO DE AULASEGURA CONTROL WEB v$Version" -ForegroundColor Cyan
Write-Host "========================================================================" -ForegroundColor Cyan
Write-Host ""

# Definir directorio raíz del proyecto
$projectRoot = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)

# ============================================================================
# Paso 1: Limpiar directorio de salida (si se solicita)
# ============================================================================
if ($Clean -and (Test-Path $OutputDir)) {
    Write-Log "Limpiando directorio de salida..." "INFO"
    Remove-Item -Path $OutputDir -Recurse -Force
}

if (-not (Test-Path $OutputDir)) {
    New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
    Write-Log "Directorio de salida creado: $OutputDir" "SUCCESS"
}

# ============================================================================
# Paso 2: Compilar el proyecto en modo Release
# ============================================================================
Write-Log "Compilando proyecto en modo Release..." "INFO"

# Mantenerse en el directorio del script, no cambiar
try {
    $buildOutput = dotnet build "$projectRoot\AulaSegura.sln" --configuration Release 2>&1
    
    if ($LASTEXITCODE -ne 0) {
        Write-Log "Salida de compilación:" "ERROR"
        Write-Host $buildOutput -ForegroundColor Red
        throw "Error de compilación con código: $LASTEXITCODE"
    }
    
    Write-Log "Compilación exitosa" "SUCCESS"
} catch {
    Write-Log "ERROR: Fallo en la compilación" "ERROR"
    if ($_.Exception.Message -notmatch "Error de compilación") {
        Write-Log $_.Exception.Message "ERROR"
    }
    exit 1
}

# ============================================================================
# Paso 3: Crear estructura de carpetas del paquete
# ============================================================================
Write-Log "Creando estructura de carpetas del paquete..." "INFO"

$packageDir = Join-Path $OutputDir "AulaSegura-v$Version"
$serviceDir = Join-Path $packageDir "Service"
$appDir = Join-Path $packageDir "App"
$scriptsDir = Join-Path $packageDir "Scripts"
$dataDir = Join-Path $packageDir "Data"
$docsDir = Join-Path $packageDir "Docs"

# Crear directorios
New-Item -ItemType Directory -Path $serviceDir -Force | Out-Null
New-Item -ItemType Directory -Path $appDir -Force | Out-Null
New-Item -ItemType Directory -Path $scriptsDir -Force | Out-Null
New-Item -ItemType Directory -Path $dataDir -Force | Out-Null
New-Item -ItemType Directory -Path $docsDir -Force | Out-Null

Write-Log "Estructura de carpetas creada" "SUCCESS"

# ============================================================================
# Paso 4: Copiar archivos del servicio Windows
# ============================================================================
Write-Log "Copiando archivos del servicio..." "INFO"

$serviceSource = Join-Path $projectRoot "src\AulaSegura.Service\bin\Release\net8.0"
Copy-Item -Path "$serviceSource\*" -Destination $serviceDir -Recurse -Force

Write-Log "Archivos del servicio copiados" "SUCCESS"

# ============================================================================
# Paso 5: Copiar archivos de la aplicación WPF
# ============================================================================
Write-Log "Copiando archivos de la aplicación WPF..." "INFO"

$appSource = Join-Path $projectRoot "src\AulaSegura.App\bin\Release\net8.0-windows"
Copy-Item -Path "$appSource\*" -Destination $appDir -Recurse -Force

Write-Log "Archivos de la aplicación copiados" "SUCCESS"

# ============================================================================
# Paso 6: Copiar scripts de instalación
# ============================================================================
Write-Log "Copiando scripts de instalación..." "INFO"

Copy-Item -Path (Join-Path $projectRoot "scripts\install-service.ps1") -Destination $scriptsDir -Force
Copy-Item -Path (Join-Path $projectRoot "scripts\uninstall-service.ps1") -Destination $scriptsDir -Force

Write-Log "Scripts copiados" "SUCCESS"

# ============================================================================
# Paso 7: Copiar documentación
# ============================================================================
Write-Log "Copiando documentación..." "INFO"

$docFiles = @(
    "README.md",
    "INSTALACION.md",
    "MANUAL_USUARIO.md",
    "MANUAL_TECNICO.md",
    "SEGURIDAD.md",
    "VERIFICACION_FINAL.md"
)

foreach ($doc in $docFiles) {
    if (Test-Path $doc) {
        Copy-Item -Path $doc -Destination $docsDir -Force
    }
}

Write-Log "Documentación copiada" "SUCCESS"

# ============================================================================
# Paso 8: Crear archivo README del paquete
# ============================================================================
Write-Log "Creando README del paquete..." "INFO"

$readmeContent = @"
# AulaSegura Control Web v$Version

## 📦 Paquete de Instalación

Este paquete contiene todos los archivos necesarios para instalar AulaSegura Control Web en una nueva computadora.

## 🚀 Instalación Rápida

### Requisitos:
- Windows 10/11 (64-bit)
- .NET 8.0 Runtime o SDK
- Permisos de Administrador

### Pasos:

1. **Instalar .NET 8.0 Runtime** (si no está instalado)
   - Descargar desde: https://dotnet.microsoft.com/download/dotnet/8.0
   - Ejecutar el instalador

2. **Ejecutar el instalador como Administrador**
   ```powershell
   # Abrir PowerShell COMO ADMINISTRADOR
   cd Scripts
   .\install-service.ps1
   ```

3. **Iniciar la aplicación WPF**
   ```powershell
   cd ..\App
   .\AulaSegura.App.exe
   ```

## 🔑 Credenciales Iniciales

- **Usuario:** admin
- **Contraseña:** configure `SeedAdmin__Password` antes del primer inicio o revise `first-run-admin.txt`
- ⚠️ **IMPORTANTE:** Cambiar la contraseña en el primer inicio de sesión

## 📁 Estructura de Carpetas

```
AulaSegura-v$Version/
├── Service/          # Servicio Windows (motor de bloqueo)
├── App/              # Aplicación WPF (interfaz gráfica)
├── Scripts/          # Scripts de instalación/desinstalación
├── Data/             # Base de datos SQLite y archivos de datos
├── Docs/             # Documentación completa
└── README.txt        # Este archivo
```

## 🛠️ Comandos Útiles

```powershell
# Ver estado del servicio
Get-Service -Name AulaSeguraService

# Iniciar servicio
Start-Service -Name AulaSeguraService

# Detener servicio
Stop-Service -Name AulaSeguraService

# Ver logs
Get-Content "Service\Logs\aulasegura-*.log" -Tail 50

# Desinstalar
cd Scripts
.\uninstall-service.ps1
```

## 📞 Soporte

Para soporte técnico, consulte la documentación en la carpeta `Docs/`:
- MANUAL_USUARIO.md - Guía para usuarios finales
- MANUAL_TECNICO.md - Documentación técnica detallada
- SEGURIDAD.md - Políticas de seguridad

## ℹ️ Información Técnica

- **Versión:** $Version
- **Fecha de compilación:** $(Get-Date -Format "yyyy-MM-dd")
- **.NET Version:** 8.0
- **Base de datos:** SQLite
- **Arquitectura:** Clean Architecture

---

**Desarrollado por:** Equipo AulaSegura  
**Licencia:** Propietaria  
**Estado:** ✅ Production Ready
"@

$readmeContent | Out-File -FilePath (Join-Path $packageDir "README.txt") -Encoding UTF8

Write-Log "README creado" "SUCCESS"

# ============================================================================
# Paso 9: Crear script de instalación rápida
# ============================================================================
Write-Log "Creando script de instalación rápida..." "INFO"

$quickInstallContent = @"
# ============================================================================
# Instalación Rápida - AulaSegura Control Web
# ============================================================================
# Este script realiza la instalación completa automáticamente
# REQUIERE: Ejecutar como Administrador
# ============================================================================

`$ErrorActionPreference = "Stop"

Write-Host "`n" 
Write-Host "========================================================================" -ForegroundColor Cyan
Write-Host "              INSTALACIÓN RÁPIDA DE AULASEGURA" -ForegroundColor Cyan
Write-Host "========================================================================" -ForegroundColor Cyan
Write-Host ""

# Verificar permisos de administrador
`$currentUser = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
if (-not `$currentUser.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Host "ERROR: Ejecute este script como Administrador" -ForegroundColor Red
    pause
    exit 1
}

Write-Host "[1/4] Verificando .NET 8.0..." -ForegroundColor Yellow
try {
    `$dotnetVersion = dotnet --version
    Write-Host "  ✓ .NET detectado: `$dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "  ✗ .NET 8.0 no está instalado" -ForegroundColor Red
    Write-Host "  Descargue desde: https://dotnet.microsoft.com/download/dotnet/8.0" -ForegroundColor Yellow
    pause
    exit 1
}

Write-Host "[2/4] Instalando servicio Windows..." -ForegroundColor Yellow
try {
    & ".\install-service.ps1" -Force
    Write-Host "  ✓ Servicio instalado correctamente" -ForegroundColor Green
} catch {
    Write-Host "  ✗ Error al instalar el servicio" -ForegroundColor Red
    Write-Host `$_.Exception.Message -ForegroundColor Red
    pause
    exit 1
}

Write-Host "[3/4] Verificando instalación..." -ForegroundColor Yellow
`$service = Get-Service -Name "AulaSeguraService" -ErrorAction SilentlyContinue
if (`$service -and `$service.Status -eq 'Running') {
    Write-Host "  ✓ Servicio en ejecución" -ForegroundColor Green
} else {
    Write-Host "  ⚠ Servicio instalado pero no iniciado" -ForegroundColor Yellow
    Write-Host "  Inicie manualmente: Start-Service -Name AulaSeguraService" -ForegroundColor Yellow
}

Write-Host "[4/4] Preparando aplicación..." -ForegroundColor Yellow
Write-Host "  ✓ Aplicación lista en carpeta App\" -ForegroundColor Green

Write-Host "`n" 
Write-Host "========================================================================" -ForegroundColor Cyan
Write-Host "                   INSTALACIÓN COMPLETADA" -ForegroundColor Cyan
Write-Host "========================================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Credenciales iniciales:" -ForegroundColor Yellow
Write-Host "  Usuario: admin (configurable con SeedAdmin__Username)" -ForegroundColor White
Write-Host "  Contraseña: use SeedAdmin__Password o revise first-run-admin.txt tras el primer inicio" -ForegroundColor White
Write-Host ""
Write-Host "Para iniciar la aplicación:" -ForegroundColor Yellow
Write-Host "  1. Abra la carpeta 'App'" -ForegroundColor White
Write-Host "  2. Ejecute 'AulaSegura.App.exe'" -ForegroundColor White
Write-Host ""
Write-Host "========================================================================" -ForegroundColor Cyan
Write-Host ""

pause
"@

$quickInstallContent | Out-File -FilePath (Join-Path $scriptsDir "INSTALAR_RAPIDO.ps1") -Encoding UTF8

Write-Log "Script de instalación rápida creado" "SUCCESS"

# ============================================================================
# Paso 10: Crear archivo ZIP del paquete
# ============================================================================
Write-Log "Creando archivo ZIP del paquete..." "INFO"

$zipFileName = "AulaSegura-v$Version.zip"
$zipPath = Join-Path $OutputDir $zipFileName

if (Test-Path $zipPath) {
    Remove-Item $zipPath -Force
}

try {
    Compress-Archive -Path "$packageDir\*" -DestinationPath $zipPath -Force
    Write-Log "Archivo ZIP creado: $zipPath" "SUCCESS"
    
    # Obtener tamaño del archivo
    $zipSize = (Get-Item $zipPath).Length / 1MB
    Write-Log "Tamaño del paquete: $([math]::Round($zipSize, 2)) MB" "INFO"
} catch {
    Write-Log "Advertencia: No se pudo crear el archivo ZIP" "WARN"
    Write-Log "Puede comprimir manualmente la carpeta: $packageDir" "WARN"
}

# ============================================================================
# Resumen Final
# ============================================================================
Write-Host "`n" 
Write-Host "========================================================================" -ForegroundColor Cyan
Write-Host "                  EMPAQUETADO COMPLETADO" -ForegroundColor Cyan
Write-Host "========================================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Paquete creado en: $packageDir" -ForegroundColor Green
Write-Host "Archivo ZIP: $zipPath" -ForegroundColor Green
Write-Host ""
Write-Host "Contenido del paquete:" -ForegroundColor Yellow
Write-Host "  ✓ Servicio Windows (motor de bloqueo)" -ForegroundColor White
Write-Host "  ✓ Aplicación WPF (interfaz gráfica)" -ForegroundColor White
Write-Host "  ✓ Scripts de instalación" -ForegroundColor White
Write-Host "  ✓ Documentación completa" -ForegroundColor White
Write-Host "  ✓ README con instrucciones" -ForegroundColor White
Write-Host ""
Write-Host "Para distribuir:" -ForegroundColor Yellow
Write-Host "  1. Copie el archivo ZIP a la computadora destino" -ForegroundColor White
Write-Host "  2. Extraiga el contenido" -ForegroundColor White
Write-Host "  3. Ejecute como Administrador: Scripts\INSTALAR_RAPIDO.ps1" -ForegroundColor White
Write-Host ""
Write-Host "========================================================================" -ForegroundColor Cyan
Write-Host ""

exit 0
