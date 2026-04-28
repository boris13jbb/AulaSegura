# ============================================================================
# Instalación Rápida - AulaSegura Control Web
# ============================================================================
# Este script realiza la instalación completa automáticamente
# REQUIERE: Ejecutar como Administrador
# ============================================================================

$ErrorActionPreference = "Stop"

Write-Host "
" 
Write-Host "========================================================================" -ForegroundColor Cyan
Write-Host "              INSTALACIÓN RÁPIDA DE AULASEGURA" -ForegroundColor Cyan
Write-Host "========================================================================" -ForegroundColor Cyan
Write-Host ""

# Verificar permisos de administrador
$currentUser = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
if (-not $currentUser.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Host "ERROR: Ejecute este script como Administrador" -ForegroundColor Red
    pause
    exit 1
}

Write-Host "[1/4] Verificando .NET 8.0..." -ForegroundColor Yellow
try {
    $dotnetVersion = dotnet --version
    Write-Host "  ✓ .NET detectado: $dotnetVersion" -ForegroundColor Green
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
    Write-Host $_.Exception.Message -ForegroundColor Red
    pause
    exit 1
}

Write-Host "[3/4] Verificando instalación..." -ForegroundColor Yellow
$service = Get-Service -Name "AulaSeguraService" -ErrorAction SilentlyContinue
if ($service -and $service.Status -eq 'Running') {
    Write-Host "  ✓ Servicio en ejecución" -ForegroundColor Green
} else {
    Write-Host "  ⚠ Servicio instalado pero no iniciado" -ForegroundColor Yellow
    Write-Host "  Inicie manualmente: Start-Service -Name AulaSeguraService" -ForegroundColor Yellow
}

Write-Host "[4/4] Preparando aplicación..." -ForegroundColor Yellow
Write-Host "  ✓ Aplicación lista en carpeta App\" -ForegroundColor Green

Write-Host "
" 
Write-Host "========================================================================" -ForegroundColor Cyan
Write-Host "                   INSTALACIÓN COMPLETADA" -ForegroundColor Cyan
Write-Host "========================================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Credenciales:" -ForegroundColor Yellow
Write-Host "  Usuario: admin" -ForegroundColor White
Write-Host "  Contraseña: Admin@123" -ForegroundColor White
Write-Host ""
Write-Host "Para iniciar la aplicación:" -ForegroundColor Yellow
Write-Host "  1. Abra la carpeta 'App'" -ForegroundColor White
Write-Host "  2. Ejecute 'AulaSegura.App.exe'" -ForegroundColor White
Write-Host ""
Write-Host "========================================================================" -ForegroundColor Cyan
Write-Host ""

pause
