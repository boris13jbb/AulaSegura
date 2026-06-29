# 📦 AulaSegura Control Web - Guía de Instalación

**Versión:** 1.0.0  
**Fecha:** 24 de Abril, 2026  
**Sistema Operativo:** Windows 10/11 (64-bit)

---

## 📋 Tabla de Contenidos

1. [Requisitos del Sistema](#requisitos-del-sistema)
2. [Descarga e Instalación](#descarga-e-instalación)
3. [Instalación del Servicio Windows](#instalación-del-servicio-windows)
4. [Configuración de la Aplicación WPF](#configuración-de-la-aplicación-wpf)
5. [Verificación de la Instalación](#verificación-de-la-instalación)
6. [Solución de Problemas](#solución-de-problemas)
7. [Desinstalación](#desinstalación)

---

## 🔧 Requisitos del Sistema

### Hardware Mínimo
- **Procesador:** Intel Core i3 o equivalente (2 GHz)
- **Memoria RAM:** 4 GB
- **Espacio en Disco:** 500 MB libres
- **Resolución de Pantalla:** 1280x720

### Software Requerido
- **Sistema Operativo:** Windows 10 (versión 1903+) o Windows 11
- **.NET Runtime:** .NET 8.0 Runtime (se instala automáticamente)
- **Permisos:** Privilegios de Administrador para instalar el servicio

### Componentes Opcionales
- **.NET SDK 8.0:** Solo necesario para desarrollo o compilación desde código fuente
- **Visual Studio 2022:** Para desarrollo y depuración (Community Edition es gratuita)

---

## 📥 Descarga e Instalación

### Opción 1: Instalador Automático (Recomendado)

#### Paso 1: Descargar el Paquete
Descargue el archivo `AulaSegura-Setup.zip` desde el sitio oficial o repositorio.

#### Paso 2: Extraer Archivos
```powershell
# Crear directorio de instalación
mkdir C:\Program Files\AulaSegura

# Extraer archivos al directorio
Expand-Archive -Path "AulaSegura-Setup.zip" -DestinationPath "C:\Program Files\AulaSegura"
```

#### Paso 3: Ejecutar Script de Instalación
```powershell
# Abrir PowerShell como Administrador
# Navegar al directorio de scripts
cd "C:\Program Files\AulaSegura\scripts"

# Ejecutar script de instalación
.\install-service.ps1
```

El script realizará automáticamente:
- ✅ Verificación de permisos de administrador
- ✅ Creación de directorios necesarios (Data, Logs, Backups)
- ✅ Instalación del servicio Windows
- ✅ Configuración de recuperación automática
- ✅ Verificación del archivo hosts
- ✅ Inicio del servicio

---

### Opción 2: Instalación Manual

#### Paso 1: Preparar Directorios
```powershell
# Crear estructura de directorios
mkdir "C:\Program Files\AulaSegura\Service"
mkdir "C:\Program Files\AulaSegura\App"
mkdir "C:\Program Files\AulaSegura\Data"
mkdir "C:\Program Files\AulaSegura\Logs"
mkdir "C:\Program Files\AulaSegura\Backups"
```

#### Paso 2: Copiar Archivos del Servicio
Copie todos los archivos del proyecto `AulaSegura.Service` a:
```
C:\Program Files\AulaSegura\Service\
```

Archivos requeridos:
- `AulaSegura.Service.exe`
- `AulaSegura.Service.dll`
- `AulaSegura.Core.dll`
- `AulaSegura.Infrastructure.dll`
- `AulaSegura.Shared.dll`
- `appsettings.json`
- Todas las dependencias (.dll adicionales)

#### Paso 3: Copiar Archivos de la Aplicación WPF
Copie todos los archivos del proyecto `AulaSegura.App` a:
```
C:\Program Files\AulaSegura\App\
```

Archivos requeridos:
- `AulaSegura.App.exe`
- `AulaSegura.App.dll`
- `AulaSegura.Core.dll`
- `AulaSegura.Infrastructure.dll`
- `AulaSegura.Shared.dll`
- `appsettings.json`
- Todas las dependencias (.dll adicionales)

#### Paso 4: Instalar el Servicio Windows
```powershell
# Abrir PowerShell como Administrador

# Instalar servicio
sc.exe create "AulaSeguraService" `
    binPath= "C:\Program Files\AulaSegura\Service\AulaSegura.Service.exe" `
    start= auto `
    DisplayName= "AulaSegura Control Web Service" `
    obj= "LocalSystem" `
    type= own

# Configurar recuperación automática
sc.exe failure "AulaSeguraService" reset= 86400 actions= restart/5000/restart/10000/restart/30000

# Iniciar servicio
Start-Service "AulaSeguraService"

# Verificar estado
Get-Service "AulaSeguraService"
```

#### Paso 5: Verificar Permisos del Archivo Hosts
```powershell
# El servicio necesita permisos de escritura en el archivo hosts
$hostsPath = "C:\Windows\System32\drivers\etc\hosts"

# Verificar que el archivo existe
Test-Path $hostsPath

# Si no existe, crear uno básico
if (-not (Test-Path $hostsPath)) {
    "# Default hosts file`r`n127.0.0.1       localhost`r`n" | Out-File -FilePath $hostsPath -Encoding ASCII
}
```

---

## 🖥️ Instalación del Servicio Windows

### Usando el Script Automatizado (Recomendado)

```powershell
# 1. Abrir PowerShell como Administrador
#    (Click derecho → "Ejecutar como administrador")

# 2. Navegar al directorio de scripts
cd "C:\Program Files\AulaSegura\scripts"

# 3. Ejecutar el script de instalación
.\install-service.ps1
```

**El script mostrará progreso en tiempo real:**
```
========================================
  AulaSegura Control Web - Instalador
========================================

[INFO] Verificando permisos de administrador... ✓
[INFO] Verificando archivos del servicio... ✓
[INFO] Creando directorios necesarios... ✓
[INFO] Instalando servicio Windows... ✓
[INFO] Configurando recuperación automática... ✓
[INFO] Iniciando servicio... ✓
[INFO] Verificando estado del servicio... ✓
[INFO] Probando permisos del archivo hosts... ✓

========================================
  Instalación Completada Exitosamente
========================================

Servicio: AulaSeguraService
Estado: Running
Inicio: Automático

Credenciales por defecto:
  Usuario: admin
  Contraseña: SeedAdmin__Password o first-run-admin.txt

IMPORTANTE: Cambie la contraseña después del primer inicio de sesión.
```

### Instalación Manual (Avanzado)

Si prefiere control total sobre la instalación:

```powershell
# 1. Verificar que el ejecutable existe
Test-Path "C:\Program Files\AulaSegura\Service\AulaSegura.Service.exe"

# 2. Instalar servicio con configuración personalizada
sc.exe create "AulaSeguraService" `
    binPath= "`"C:\Program Files\AulaSegura\Service\AulaSegura.Service.exe`"" `
    start= auto `
    DisplayName= "AulaSegura Control Web Service" `
    obj= "LocalSystem" `
    type= own `
    description= "Servicio de filtrado web y control parental para escuelas y hogares"

# 3. Configurar política de recuperación
#    Reiniciar después de 5s, 10s, 30s en caso de fallos consecutivos
sc.exe failure "AulaSeguraService" reset= 86400 actions= restart/5000/restart/10000/restart/30000

# 4. Establecer descripción del servicio
sc.exe description "AulaSeguraService" "Servicio de filtrado web y control parental"

# 5. Iniciar servicio
net start "AulaSeguraService"

# 6. Verificar estado
sc.exe query "AulaSeguraService"
```

---

## ⚙️ Configuración de la Aplicación WPF

### Primer Inicio

1. **Navegar al directorio de la aplicación:**
   ```
   C:\Program Files\AulaSegura\App\
   ```

2. **Ejecutar la aplicación:**
   - Doble-click en `AulaSegura.App.exe`
   - O desde línea de comandos:
     ```powershell
     .\AulaSegura.App.exe
     ```

3. **Pantalla de Login aparecerá:**
   ```
   ┌─────────────────────────────┐
   │          🔒                 │
   │      AulaSegura             │
   │      Control Web            │
   │                             │
   │  Usuario: [____________]    │
   │  Contraseña: [__________]   │
   │                             │
   │  [🔑 Iniciar Sesión]        │
   │  [Salir]                    │
   │                             │
   │  © 2026 AulaSegura          │
   └─────────────────────────────┘
   ```

4. **Iniciar sesión con credenciales por defecto:**
   - **Usuario:** `admin`
   - **Contraseña:** `SeedAdmin__Password o first-run-admin.txt`

5. **¡Listo!** La aplicación mostrará el dashboard principal.

### Crear Acceso Directo en el Escritorio

Para facilitar el acceso diario:

```powershell
# Crear acceso directo en el escritorio
$WshShell = New-Object -ComObject WScript.Shell
$Shortcut = $WshShell.CreateShortcut("$env:USERPROFILE\Desktop\AulaSegura.lnk")
$Shortcut.TargetPath = "C:\Program Files\AulaSegura\App\AulaSegura.App.exe"
$Shortcut.WorkingDirectory = "C:\Program Files\AulaSegura\App"
$Shortcut.IconLocation = "C:\Program Files\AulaSegura\App\AulaSegura.App.exe,0"
$Shortcut.Save()
```

### Agregar al Menú de Inicio

```powershell
# Crear carpeta en Menú Inicio
$startMenuPath = "$env:APPDATA\Microsoft\Windows\Start Menu\Programs\AulaSegura"
mkdir $startMenuPath

# Crear acceso directo
$WshShell = New-Object -ComObject WScript.Shell
$Shortcut = $WshShell.CreateShortcut("$startMenuPath\AulaSegura Control Web.lnk")
$Shortcut.TargetPath = "C:\Program Files\AulaSegura\App\AulaSegura.App.exe"
$Shortcut.WorkingDirectory = "C:\Program Files\AulaSegura\App"
$Shortcut.Save()
```

---

## ✅ Verificación de la Instalación

### Checklist de Verificación

Marque cada ítem después de verificarlo:

- [ ] **Servicio Windows instalado**
  ```powershell
  Get-Service "AulaSeguraService"
  # Debe mostrar: Status = Running, StartType = Automatic
  ```

- [ ] **Directorios creados**
  ```powershell
  Test-Path "C:\Program Files\AulaSegura\Data"
  Test-Path "C:\Program Files\AulaSegura\Logs"
  Test-Path "C:\Program Files\AulaSegura\Backups"
  # Todos deben retornar: True
  ```

- [ ] **Base de datos creada**
  ```powershell
  Test-Path "C:\Program Files\AulaSegura\Data\aulasegura.db"
  # Debe retornar: True (después del primer inicio)
  ```

- [ ] **Archivo de log generado**
  ```powershell
  Get-ChildItem "C:\Program Files\AulaSegura\Logs\" -Filter "*.log"
  # Debe mostrar al menos un archivo de log
  ```

- [ ] **Aplicación WPF inicia correctamente**
  ```powershell
  Start-Process "C:\Program Files\AulaSegura\App\AulaSegura.App.exe"
  # La ventana de login debe aparecer
  ```

- [ ] **Login funciona con credenciales por defecto**
  - Usuario: `admin`
  - Contraseña: `SeedAdmin__Password o first-run-admin.txt`
  - Debe mostrar el dashboard

- [ ] **Archivo hosts tiene respaldo**
  ```powershell
  Get-ChildItem "C:\Program Files\AulaSegura\Backups\hosts\"
  # Debe mostrar archivos de backup con timestamps
  ```

- [ ] **Reglas de bloqueo aplicadas**
  ```powershell
  # Verificar que el servicio está activo
  Get-Service "AulaSeguraService" | Select-Object Status
  
  # Revisar logs del servicio
  Get-Content "C:\Program Files\AulaSegura\Logs\aulasegura-*.log" -Tail 20
  ```

### Comandos de Verificación Rápida

Ejecute este script para verificación automática:

```powershell
Write-Host "=== Verificación de Instalación AulaSegura ===" -ForegroundColor Cyan
Write-Host ""

# Verificar servicio
$service = Get-Service "AulaSeguraService" -ErrorAction SilentlyContinue
if ($service) {
    Write-Host "[✓] Servicio instalado: $($service.Status)" -ForegroundColor Green
} else {
    Write-Host "[✗] Servicio NO encontrado" -ForegroundColor Red
}

# Verificar directorios
$dirs = @("Data", "Logs", "Backups")
foreach ($dir in $dirs) {
    $path = "C:\Program Files\AulaSegura\$dir"
    if (Test-Path $path) {
        Write-Host "[✓] Directorio $dir existe" -ForegroundColor Green
    } else {
        Write-Host "[✗] Directorio $dir NO existe" -ForegroundColor Red
    }
}

# Verificar base de datos
$dbPath = "C:\Program Files\AulaSegura\Data\aulasegura.db"
if (Test-Path $dbPath) {
    Write-Host "[✓] Base de datos existe" -ForegroundColor Green
} else {
    Write-Host "[⚠] Base de datos aún no creada (normal en primera instalación)" -ForegroundColor Yellow
}

# Verificar aplicación
$appPath = "C:\Program Files\AulaSegura\App\AulaSegura.App.exe"
if (Test-Path $appPath) {
    Write-Host "[✓] Aplicación WPF instalada" -ForegroundColor Green
} else {
    Write-Host "[✗] Aplicación WPF NO encontrada" -ForegroundColor Red
}

Write-Host ""
Write-Host "=== Verificación Completa ===" -ForegroundColor Cyan
```

---

## 🔧 Solución de Problemas

### Problema 1: Error de Permisos de Administrador

**Síntoma:**
```
ERROR: Este script requiere privilegios de administrador
```

**Solución:**
1. Cerrar PowerShell
2. Click derecho en PowerShell → "Ejecutar como administrador"
3. Volver a ejecutar el script

---

### Problema 2: Servicio No Inicia

**Síntoma:**
```
Error 1053: El servicio no respondió a tiempo
```

**Solución:**
```powershell
# 1. Verificar logs del servicio
Get-Content "C:\Program Files\AulaSegura\Logs\aulasegura-*.log" -Tail 50

# 2. Verificar que .NET 8 está instalado
dotnet --version
# Debe mostrar: 8.0.x

# 3. Si falta .NET 8, descargar e instalar desde:
# https://dotnet.microsoft.com/download/dotnet/8.0

# 4. Reiniciar servicio
Restart-Service "AulaSeguraService"

# 5. Verificar estado
Get-Service "AulaSeguraService"
```

---

### Problema 3: Base de Datos No Se Crea

**Síntoma:**
La aplicación muestra error de conexión a la base de datos.

**Solución:**
```powershell
# 1. Verificar que el directorio Data existe
Test-Path "C:\Program Files\AulaSegura\Data"

# 2. Si no existe, crearlo
mkdir "C:\Program Files\AulaSegura\Data"

# 3. Verificar permisos de escritura
$acl = Get-Acl "C:\Program Files\AulaSegura\Data"
$acl.Access | Format-Table IdentityReference, FileSystemRights

# 4. El servicio se ejecuta como LocalSystem, debe tener permisos completos
#    Si hay problemas, otorgar permisos explícitos:
icacls "C:\Program Files\AulaSegura\Data" /grant "NT AUTHORITY\SYSTEM:(F)"

# 5. Reiniciar servicio
Restart-Service "AulaSeguraService"
```

---

### Problema 4: No Se Puede Modificar el Archivo Hosts

**Síntoma:**
Error en logs: "Permission denied accessing hosts file"

**Solución:**
```powershell
# 1. Verificar que el archivo hosts existe
Test-Path "C:\Windows\System32\drivers\etc\hosts"

# 2. Verificar permisos
$acl = Get-Acl "C:\Windows\System32\drivers\etc\hosts"
$acl.Access | Format-Table IdentityReference, FileSystemRights

# 3. Asegurar que LocalSystem tiene permisos de escritura
#    El servicio se ejecuta como LocalSystem, que debería tener acceso completo

# 4. Si hay antivirus bloqueando, agregar excepción:
#    - Windows Defender: Agregar exclusión para AulaSegura.Service.exe
#    - Otros antivirus: Consultar documentación específica

# 5. Probar escritura manual (como prueba)
"127.0.0.1       test.aulasegura.local # TEST" | Out-File -FilePath "C:\Windows\System32\drivers\etc\hosts" -Append -Encoding ASCII

# 6. Si funciona, remover línea de prueba manualmente
```

---

### Problema 5: Aplicación WPF No Abre

**Síntoma:**
Doble-click en `AulaSegura.App.exe` no hace nada.

**Solución:**
```powershell
# 1. Ejecutar desde línea de comandos para ver errores
cd "C:\Program Files\AulaSegura\App"
.\AulaSegura.App.exe

# 2. Verificar que .NET 8 Desktop Runtime está instalado
dotnet --list-runtimes
# Debe mostrar: Microsoft.WindowsDesktop.App 8.0.x

# 3. Si falta, descargar desde:
# https://dotnet.microsoft.com/download/dotnet/8.0
# Seleccionar: ".NET Desktop Runtime 8.0.x"

# 4. Verificar que la base de datos existe
Test-Path "..\Data\aulasegura.db"

# 5. Si no existe, iniciar el servicio primero para crearla
Start-Service "AulaSeguraService"
Start-Sleep -Seconds 10
Stop-Service "AulaSeguraService"

# 6. Intentar abrir la aplicación nuevamente
```

---

### Problema 6: Login Fallido con Credenciales Correctas

**Síntoma:**
Usuario y contraseña correctos, pero el login falla.

**Solución:**
```powershell
# 1. Verificar que el servicio está corriendo (crea la base de datos)
Get-Service "AulaSeguraService"

# 2. Si el servicio no ha corrido, iniciarlo
Start-Service "AulaSeguraService"
Start-Sleep -Seconds 10

# 3. Verificar que la base de datos existe
Test-Path "C:\Program Files\AulaSegura\Data\aulasegura.db"

# 4. Revisar logs para errores de autenticación
Get-Content "C:\Program Files\AulaSegura\Logs\aulasegura-*.log" -Tail 30 | Select-String "Login"

# 5. Si la cuenta está bloqueada (5 intentos fallidos), esperar 30 minutos
#    O resetear manualmente la base de datos (AVANZADO):
#    - Detener servicio
#    - Eliminar archivo .db
#    - Reiniciar servicio (recreará con seed data)
```

---

### Problema 7: Firewall Bloqueando Conexiones

**Síntoma:**
La aplicación no puede comunicarse con el servicio.

**Nota:** AulaSegura NO usa conexiones de red. Toda comunicación es local vía base de datos SQLite.

Si hay problemas de conectividad, verificar:
- El servicio está corriendo: `Get-Service "AulaSeguraService"`
- La base de datos es accesible: `Test-Path "C:\Program Files\AulaSegura\Data\aulasegura.db"`
- No hay procesos bloqueando el archivo .db

---

## 🗑️ Desinstalación

### Usando el Script de Desinstalación (Recomendado)

```powershell
# 1. Abrir PowerShell como Administrador

# 2. Navegar al directorio de scripts
cd "C:\Program Files\AulaSegura\scripts"

# 3. Ejecutar script de desinstalación
.\uninstall-service.ps1

# 4. Seguir las instrucciones en pantalla
#    - Confirmar detención del servicio
#    - Confirmar eliminación del servicio
#    - Opcional: Eliminar datos (base de datos, logs, backups)
```

**El script preguntará:**
```
¿Desea restaurar el archivo hosts desde el último backup? [Y/N]
¿Desea eliminar todos los datos (base de datos, logs, backups)? [Y/N]
```

### Desinstalación Manual

```powershell
# 1. Detener servicio
Stop-Service "AulaSeguraService"

# 2. Eliminar servicio
sc.exe delete "AulaSeguraService"

# 3. Opcional: Eliminar archivos de datos
Remove-Item "C:\Program Files\AulaSegura\Data" -Recurse -Force
Remove-Item "C:\Program Files\AulaSegura\Logs" -Recurse -Force
Remove-Item "C:\Program Files\AulaSegura\Backups" -Recurse -Force

# 4. Opcional: Restaurar archivo hosts desde backup
Copy-Item "C:\Program Files\AulaSegura\Backups\hosts\hosts-backup-*.txt" `
          "C:\Windows\System32\drivers\etc\hosts" -Force

# 5. Eliminar directorio de instalación
Remove-Item "C:\Program Files\AulaSegura" -Recurse -Force

# 6. Eliminar accesos directos
Remove-Item "$env:USERPROFILE\Desktop\AulaSegura.lnk" -Force
Remove-Item "$env:APPDATA\Microsoft\Windows\Start Menu\Programs\AulaSegura" -Recurse -Force
```

---

## 📞 Soporte Técnico

### Recursos de Documentación
- `QUICK_START.md` - Guía rápida de inicio
- `MANUAL_USUARIO.md` - Manual completo del usuario
- `MANUAL_TECNICO.md` - Referencia técnica para desarrolladores
- `SEGURIDAD.md` - Guía de seguridad y mejores prácticas

### Archivos de Log
Los logs contienen información detallada para troubleshooting:

```powershell
# Logs del servicio Windows
Get-Content "C:\Program Files\AulaSegura\Logs\aulasegura-YYYYMMDD.log"

# Logs más recientes (últimas 50 líneas)
Get-Content "C:\Program Files\AulaSegura\Logs\aulasegura-*.log" -Tail 50

# Buscar errores específicos
Get-Content "C:\Program Files\AulaSegura\Logs\aulasegura-*.log" | Select-String "ERROR"
```

### Información del Sistema para Soporte

Al contactar soporte, incluya esta información:

```powershell
# Versión de Windows
systeminfo | Select-String "OS Name", "OS Version"

# Versión de .NET
dotnet --version

# Estado del servicio
Get-Service "AulaSeguraService" | Format-List *

# Espacio en disco
Get-PSDrive C | Select-Object Used, Free

# Últimos errores del servicio
Get-EventLog -LogName Application -Source "AulaSeguraService" -Newest 10
```

---

## ✨ Resumen de Instalación

### Pasos Rápidos (Resumen)

1. ✅ Descargar y extraer archivos
2. ✅ Ejecutar `install-service.ps1` como Administrador
3. ✅ Verificar que el servicio está corriendo
4. ✅ Ejecutar `AulaSegura.App.exe`
5. ✅ Iniciar sesión con `admin` / `SeedAdmin__Password o first-run-admin.txt`
6. ✅ ¡Listo! Comenzar a configurar reglas de bloqueo

### Tiempos Estimados
- **Descarga:** 2-5 minutos (depende de conexión)
- **Instalación:** 1-2 minutos
- **Configuración inicial:** 5-10 minutos
- **Total:** ~15 minutos

---

**Documento creado:** 24 de Abril, 2026  
**Versión:** 1.0.0  
**Mantenimiento:** Equipo de Desarrollo AulaSegura
