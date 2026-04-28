# ============================================================================
# Script de Desinstalación del Servicio Windows - AulaSegura Control Web
# ============================================================================
# Este script desinstala el servicio Windows de AulaSegura
# REQUIERE: Ejecutar como Administrador
# ============================================================================

param(
    [string]$ServiceName = "AulaSeguraService",
    [switch]$Force = $false,
    [switch]$RemoveData = $false
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
# Verificar si el servicio existe
# ============================================================================
$service = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue

if (-not $service) {
    Write-Log "El servicio '$ServiceName' no está instalado" "WARN"
    exit 0
}

Write-Log "Servicio encontrado: $($service.DisplayName)" "INFO"
Write-Log "Estado actual: $($service.Status)" "INFO"

# ============================================================================
# Confirmación del usuario
# ============================================================================
if (-not $Force) {
    Write-Host ""
    Write-Host "ADVERTENCIA: Esta acción desinstalará permanentemente el servicio" -ForegroundColor Yellow
    Write-Host "Nombre del servicio: $ServiceName" -ForegroundColor Yellow
    
    if ($RemoveData) {
        Write-Host "TAMBIÉN SE ELIMINARÁN TODOS LOS DATOS Y CONFIGURACIONES" -ForegroundColor Red
    }
    
    Write-Host ""
    $response = Read-Host "¿Está seguro que desea continuar? (S/N)"
    
    if ($response -ne 'S' -and $response -ne 's') {
        Write-Log "Desinstalación cancelada por el usuario" "WARN"
        exit 0
    }
}

# ============================================================================
# Detener el Servicio
# ============================================================================
Write-Log "Deteniendo servicio..." "INFO"

try {
    if ($service.Status -eq 'Running') {
        Stop-Service -Name $ServiceName -Force
        Start-Sleep -Seconds 3
        Write-Log "Servicio detenido correctamente" "SUCCESS"
    } else {
        Write-Log "El servicio ya estaba detenido" "INFO"
    }
} catch {
    Write-Log "Advertencia: No se pudo detener el servicio correctamente" "WARN"
    Write-Log $_.Exception.Message "WARN"
}

# ============================================================================
# Eliminar el Servicio
# ============================================================================
Write-Log "Eliminando servicio Windows..." "INFO"

try {
    # Usar sc.exe para eliminar el servicio
    $result = sc.exe delete $ServiceName
    
    if ($LASTEXITCODE -ne 0) {
        throw "sc.exe falló con código de salida: $LASTEXITCODE"
    }
    
    # Esperar a que el servicio se elimine completamente
    Start-Sleep -Seconds 3
    
    # Verificar que se eliminó
    $checkService = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
    if ($checkService) {
        throw "El servicio aún existe después de intentar eliminarlo"
    }
    
    Write-Log "Servicio eliminado correctamente" "SUCCESS"
} catch {
    Write-Log "ERROR: No se pudo eliminar el servicio" "ERROR"
    Write-Log $_.Exception.Message "ERROR"
    Write-Log "Puede intentar eliminarlo manualmente desde services.msc" "WARN"
    exit 1
}

# ============================================================================
# Restaurar archivo hosts (opcional)
# ============================================================================
Write-Log "Verificando respaldos del archivo hosts..." "INFO"

$hostsPath = "C:\Windows\System32\drivers\etc\hosts"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Split-Path -Parent $scriptDir
$backupsDir = Join-Path $projectRoot "src\AulaSegura.Service\Backups\hosts"

if (Test-Path $backupsDir) {
    $backupFiles = Get-ChildItem -Path $backupsDir -Filter "hosts_backup_*" | Sort-Object Name -Descending
    
    if ($backupFiles.Count -gt 0) {
        $latestBackup = $backupFiles[0]
        Write-Log "Respaldo más reciente encontrado: $($latestBackup.Name)" "INFO"
        
        if (-not $Force) {
            $restoreResponse = Read-Host "¿Desea restaurar el archivo hosts original? (S/N)"
            
            if ($restoreResponse -eq 'S' -or $restoreResponse -eq 's') {
                try {
                    Copy-Item -Path $latestBackup.FullName -Destination $hostsPath -Force
                    Write-Log "Archivo hosts restaurado desde respaldo" "SUCCESS"
                } catch {
                    Write-Log "ERROR: No se pudo restaurar el archivo hosts" "ERROR"
                    Write-Log $_.Exception.Message "ERROR"
                }
            }
        }
    } else {
        Write-Log "No se encontraron respaldos del archivo hosts" "INFO"
    }
}

# ============================================================================
# Eliminar datos (opcional)
# ============================================================================
if ($RemoveData) {
    Write-Log "Eliminando datos y configuraciones..." "WARN"
    
    $dataDirs = @(
        (Join-Path $projectRoot "src\AulaSegura.Service\Data"),
        (Join-Path $projectRoot "src\AulaSegura.Service\Logs"),
        (Join-Path $projectRoot "src\AulaSegura.Service\Backups")
    )
    
    foreach ($dir in $dataDirs) {
        if (Test-Path $dir) {
            try {
                Remove-Item -Path $dir -Recurse -Force
                Write-Log "Directorio eliminado: $dir" "SUCCESS"
            } catch {
                Write-Log "ERROR: No se pudo eliminar el directorio: $dir" "ERROR"
                Write-Log $_.Exception.Message "ERROR"
            }
        }
    }
    
    # Eliminar base de datos si existe en el directorio raíz
    $dbPath = Join-Path $projectRoot "aulasegura.db"
    if (Test-Path $dbPath) {
        try {
            Remove-Item -Path $dbPath -Force
            Write-Log "Base de datos eliminada: $dbPath" "SUCCESS"
        } catch {
            Write-Log "ERROR: No se pudo eliminar la base de datos" "ERROR"
        }
    }
}

# ============================================================================
# Resumen de Desinstalación
# ============================================================================
Write-Host "`n"
Write-Host "========================================================================" -ForegroundColor Cyan
Write-Host "                  DESINSTALACIÓN COMPLETADA" -ForegroundColor Cyan
Write-Host "========================================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Servicio: $ServiceName - ELIMINADO" -ForegroundColor Green
Write-Host ""

if ($RemoveData) {
    Write-Host "Datos y configuraciones: ELIMINADOS" -ForegroundColor Red
} else {
    Write-Host "Datos y configuraciones: CONSERVADOS" -ForegroundColor Yellow
    Write-Host "Para eliminarlos manualmente, ejecute:" -ForegroundColor Yellow
    Write-Host "  .\uninstall-service.ps1 -RemoveData" -ForegroundColor White
}

Write-Host ""
Write-Host "Archivos del programa: CONSERVADOS" -ForegroundColor Yellow
Write-Host "Para eliminarlos, borre manualmente la carpeta de instalación" -ForegroundColor Yellow
Write-Host ""

if (Test-Path $hostsPath) {
    Write-Host "Nota: Verifique el archivo hosts si experimenta problemas de conectividad" -ForegroundColor Yellow
    Write-Host "Ruta: $hostsPath" -ForegroundColor White
}

Write-Host ""
Write-Host "========================================================================" -ForegroundColor Cyan

exit 0
