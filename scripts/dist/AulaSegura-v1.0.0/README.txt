# AulaSegura Control Web v1.0.0

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
   `powershell
   # Abrir PowerShell COMO ADMINISTRADOR
   cd Scripts
   .\install-service.ps1
   `

3. **Iniciar la aplicación WPF**
   `powershell
   cd ..\App
   .\AulaSegura.App.exe
   `

## 🔑 Credenciales por Defecto

- **Usuario:** admin
- **Contraseña:** Admin@123
- ⚠️ **IMPORTANTE:** Cambiar la contraseña en el primer inicio de sesión

## 📁 Estructura de Carpetas

`
AulaSegura-v1.0.0/
├── Service/          # Servicio Windows (motor de bloqueo)
├── App/              # Aplicación WPF (interfaz gráfica)
├── Scripts/          # Scripts de instalación/desinstalación
├── Data/             # Base de datos SQLite y archivos de datos
├── Docs/             # Documentación completa
└── README.txt        # Este archivo
`

## 🛠️ Comandos Útiles

`powershell
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
`

## 📞 Soporte

Para soporte técnico, consulte la documentación en la carpeta Docs/:
- MANUAL_USUARIO.md - Guía para usuarios finales
- MANUAL_TECNICO.md - Documentación técnica detallada
- SEGURIDAD.md - Políticas de seguridad

## ℹ️ Información Técnica

- **Versión:** 1.0.0
- **Fecha de compilación:** 2026-04-24
- **.NET Version:** 8.0
- **Base de datos:** SQLite
- **Arquitectura:** Clean Architecture

---

**Desarrollado por:** Equipo AulaSegura  
**Licencia:** Propietaria  
**Estado:** ✅ Production Ready
