# ============================================================================
# Script de Instalación del Servicio Windows - AulaSegura Control Web
# ============================================================================
# Este script instala y configura el servicio Windows de AulaSegura
# REQUIERE: Ejecutar como Administrador
# ============================================================================

param(
    [string]$ServiceName = "AulaSeguraService",
    [string]$ServiceDisplayName = "AulaSegura Control Web Service",
    [string]$ServiceDescription = "Servicio de bloqueo web para control parental y administrativo. Aplica reglas de filtrado de contenido mediante modificación del archivo hosts.",
    [string]$ServicePath = "",
    [switch]$Force = $false
)

# Configurar política de ejecución
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass -Force

# Función para verificar permisos de administrador
function Test-Administrator {
    $currentUser = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
    return $currentUser.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

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

# ============================================================================
# Verificación de Permisos
# ============================================================================
Write-Log "Verificando permisos de administrador..." "INFO"

if (-not (Test-Administrator)) {
    Write-Log "ERROR: Este script requiere permisos de administrador" "ERROR"
    Write-Log "Ejecute PowerShell como Administrador e intente nuevamente" "ERROR"
    exit 1
}

Write-Log "Permisos de administrador verificados correctamente" "SUCCESS"

# ============================================================================
# Determinar ruta del ejecutable
# ============================================================================
if ([string]::IsNullOrEmpty($ServicePath)) {
    # Ruta por defecto relativa al script
    $scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
    $projectRoot = Split-Path -Parent $scriptDir
    $ServicePath = Join-Path $projectRoot "src\AulaSegura.Service\bin\Release\net8.0\AulaSegura.Service.exe"
}

Write-Log "Ruta del servicio: $ServicePath" "INFO"

# Verificar que el ejecutable existe
if (-not (Test-Path $ServicePath)) {
    Write-Log "ERROR: No se encontró el ejecutable del servicio en: $ServicePath" "ERROR"
    Write-Log "Compile el proyecto primero con: dotnet build --configuration Release" "ERROR"
    exit 1
}

Write-Log "Ejecutable encontrado correctamente" "SUCCESS"

# ============================================================================
# Verificar si el servicio ya está instalado
# ============================================================================
$existingService = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue

if ($existingService) {
    Write-Log "Advertencia: El servicio '$ServiceName' ya está instalado" "WARN"
    
    if (-not $Force) {
        $response = Read-Host "¿Desea reinstalar el servicio? (S/N)"
        if ($response -ne 'S' -and $response -ne 's') {
            Write-Log "Instalación cancelada por el usuario" "WARN"
            exit 0
        }
    }
    
    Write-Log "Deteniendo servicio existente..." "INFO"
    try {
        Stop-Service -Name $ServiceName -Force -ErrorAction SilentlyContinue
        Start-Sleep -Seconds 2
    } catch {
        Write-Log "Advertencia: No se pudo detener el servicio existente" "WARN"
    }
    
    Write-Log "Eliminando servicio existente..." "INFO"
    try {
        sc.exe delete $ServiceName | Out-Null
        Start-Sleep -Seconds 2
    } catch {
        Write-Log "ERROR: No se pudo eliminar el servicio existente" "ERROR"
        exit 1
    }
    
    Write-Log "Servicio anterior eliminado correctamente" "SUCCESS"
}

# ============================================================================
# Crear directorios necesarios
# ============================================================================
$serviceDir = Split-Path -Parent $ServicePath
$dataDir = Join-Path $serviceDir "Data"
$logsDir = Join-Path $serviceDir "Logs"
$backupsDir = Join-Path $serviceDir "Backups"

Write-Log "Creando directorios necesarios..." "INFO"

try {
    if (-not (Test-Path $dataDir)) {
        New-Item -ItemType Directory -Path $dataDir -Force | Out-Null
        Write-Log "Directorio Data creado: $dataDir" "INFO"
    }
    
    if (-not (Test-Path $logsDir)) {
        New-Item -ItemType Directory -Path $logsDir -Force | Out-Null
        Write-Log "Directorio Logs creado: $logsDir" "INFO"
    }
    
    if (-not (Test-Path $backupsDir)) {
        New-Item -ItemType Directory -Path $backupsDir -Force | Out-Null
        Write-Log "Directorio Backups creado: $backupsDir" "INFO"
    }
} catch {
    Write-Log "ERROR: No se pudieron crear los directorios necesarios" "ERROR"
    Write-Log $_.Exception.Message "ERROR"
    exit 1
}

# ============================================================================
# Instalar el Servicio Windows
# ============================================================================
Write-Log "Instalando servicio Windows..." "INFO"
Write-Log "Nombre: $ServiceName" "INFO"
Write-Log "Display Name: $ServiceDisplayName" "INFO"

try {
    # Usar sc.exe para instalar el servicio
    $result = sc.exe create $ServiceName `
        binPath= "`"$ServicePath`"" `
        start= auto `
        DisplayName= "$ServiceDisplayName" `
        obj= "LocalSystem" `
        type= own
    
    if ($LASTEXITCODE -ne 0) {
        throw "sc.exe falló con código de salida: $LASTEXITCODE"
    }
    
    # Establecer descripción del servicio
    sc.exe description $ServiceName "$ServiceDescription" | Out-Null
    
    Write-Log "Servicio instalado correctamente" "SUCCESS"
} catch {
    Write-Log "ERROR: Fallo al instalar el servicio" "ERROR"
    Write-Log $_.Exception.Message "ERROR"
    exit 1
}

# ============================================================================
# Configurar recuperación del servicio
# ============================================================================
Write-Log "Configurando recuperación automática del servicio..." "INFO"

try {
    # Configurar reinicio automático en caso de fallo
    sc.exe failure $ServiceName reset= 86400 actions= restart/5000/restart/10000/restart/30000 | Out-Null
    Write-Log "Recuperación automática configurada" "SUCCESS"
} catch {
    Write-Log "Advertencia: No se pudo configurar la recuperación automática" "WARN"
}

# ============================================================================
# Iniciar el Servicio
# ============================================================================
Write-Log "Iniciando servicio..." "INFO"

try {
    Start-Service -Name $ServiceName
    Start-Sleep -Seconds 3
    
    # Verificar estado del servicio
    $serviceStatus = Get-Service -Name $ServiceName
    
    if ($serviceStatus.Status -eq 'Running') {
        Write-Log "Servicio iniciado correctamente" "SUCCESS"
        Write-Log "Estado: $($serviceStatus.Status)" "INFO"
    } else {
        Write-Log "Advertencia: El servicio no se inició correctamente" "WARN"
        Write-Log "Estado actual: $($serviceStatus.Status)" "WARN"
        
        # Intentar obtener logs de errores
        $events = Get-EventLog -LogName Application -Source $ServiceName -Newest 5 -ErrorAction SilentlyContinue
        if ($events) {
            Write-Log "Últimos eventos del servicio:" "WARN"
            $events | ForEach-Object {
                Write-Log "  $($_.TimeGenerated): $($_.Message)" "WARN"
            }
        }
    }
} catch {
    Write-Log "ERROR: No se pudo iniciar el servicio" "ERROR"
    Write-Log $_.Exception.Message "ERROR"
    Write-Log "Puede intentar iniciarlo manualmente desde services.msc" "WARN"
}

# ============================================================================
# Verificar archivo hosts
# ============================================================================
Write-Log "Verificando archivo hosts de Windows..." "INFO"

$hostsPath = "C:\Windows\System32\drivers\etc\hosts"
if (Test-Path $hostsPath) {
    Write-Log "Archivo hosts encontrado: $hostsPath" "SUCCESS"
    
    # Verificar permisos de escritura
    try {
        $testContent = "# Test AulaSegura`r`n"
        Add-Content -Path $hostsPath -Value $testContent -ErrorAction Stop
        Write-Log "Permisos de escritura verificados correctamente" "SUCCESS"
        
        # Eliminar línea de prueba
        $content = Get-Content $hostsPath
        $content | Where-Object { $_ -ne "# Test AulaSegura" } | Set-Content $hostsPath
    } catch {
        Write-Log "Advertencia: No se tienen permisos de escritura en el archivo hosts" "WARN"
        Write-Log "El servicio debe ejecutarse como SYSTEM para modificar el archivo hosts" "WARN"
    }
} else {
    Write-Log "ERROR: Archivo hosts no encontrado" "ERROR"
}

# ============================================================================
# Resumen de Instalación
# ============================================================================
Write-Host "`n" 
Write-Host "========================================================================" -ForegroundColor Cyan
Write-Host "                    INSTALACIÓN COMPLETADA" -ForegroundColor Cyan
Write-Host "========================================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Servicio: $ServiceName" -ForegroundColor Green
Write-Host "Estado: $(Get-Service -Name $ServiceName).Status)" -ForegroundColor Green
Write-Host "Ruta: $ServicePath" -ForegroundColor Green
Write-Host ""
Write-Host "Directorios creados:" -ForegroundColor Yellow
Write-Host "  - Data: $dataDir" -ForegroundColor White
Write-Host "  - Logs: $logsDir" -ForegroundColor White
Write-Host "  - Backups: $backupsDir" -ForegroundColor White
Write-Host ""
Write-Host "Credenciales por defecto:" -ForegroundColor Yellow
Write-Host "  Usuario: admin" -ForegroundColor White
Write-Host "  Contraseña: Admin@123" -ForegroundColor White
Write-Host "  ¡CAMBIE LA CONTRASEÑA EN EL PRIMER INICIO DE SESIÓN!" -ForegroundColor Red
Write-Host ""
Write-Host "Comandos útiles:" -ForegroundColor Yellow
Write-Host "  Ver estado: Get-Service -Name $ServiceName" -ForegroundColor White
Write-Host "  Iniciar: Start-Service -Name $ServiceName" -ForegroundColor White
Write-Host "  Detener: Stop-Service -Name $ServiceName" -ForegroundColor White
Write-Host "  Ver logs: Get-Content $logsDir\aulasegura-*.log -Tail 50" -ForegroundColor White
Write-Host "  Desinstalar: .\uninstall-service.ps1" -ForegroundColor White
Write-Host ""
Write-Host "========================================================================" -ForegroundColor Cyan

exit 0
