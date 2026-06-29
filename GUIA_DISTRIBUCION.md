# 📦 Guía de Distribución - AulaSegura Control Web

## Ubicación del Paquete de Instalación

El paquete de instalación está listo para distribuir en:

```
d:\APKICATIVO DE CONTROL DE PAGINAS\AulaSegura\scripts\dist\AulaSegura-v1.0.0.zip
```

**Tamaño:** ~52 MB

---

## 🚀 Instrucciones para Instalar en Otra Computadora

### Paso 1: Copiar el Paquete

Copie el archivo `AulaSegura-v1.0.0.zip` a la computadora destino usando:
- USB drive
- Red compartida
- Email (si el tamaño lo permite)
- Cloud storage (Google Drive, OneDrive, etc.)

### Paso 2: Requisitos Previos

Antes de instalar, verifique que la computadora destino tenga:

✅ **Windows 10 o Windows 11** (64-bit recomendado)  
✅ **.NET 8.0 Runtime** instalado  

**Para instalar .NET 8.0:**
1. Descargue desde: https://dotnet.microsoft.com/download/dotnet/8.0
2. Seleccione ".NET Runtime 8.0.x" para Windows x64
3. Ejecute el instalador
4. Reinicie la computadora si se solicita

### Paso 3: Extraer el Paquete

1. Haga clic derecho en `AulaSegura-v1.0.0.zip`
2. Seleccione "Extraer todo..."
3. Elija una ubicación (ejemplo: `C:\Program Files\AulaSegura`)
4. Desmarque "Mostrar archivos extraídos" si no es necesario
5. Haga clic en "Extraer"

### Paso 4: Ejecutar Instalación

**Opción A - Instalación Automática (Recomendada):**

1. Abra la carpeta extraída
2. Navegue a la carpeta `Scripts`
3. Haga clic derecho en `INSTALAR_RAPIDO.ps1`
4. Seleccione **"Ejecutar con PowerShell"**
5. Si aparece una advertencia de seguridad, seleccione "Más información" → "Ejecutar de todas formas"
6. El script se ejecutará automáticamente e instalará todo

**Opción B - Instalación Manual:**

1. Abra **PowerShell como Administrador**:
   - Presione `Win + X`
   - Seleccione "Windows PowerShell (Admin)" o "Terminal (Admin)"
   
2. Navegue a la carpeta Scripts:
   ```powershell
   cd "C:\ruta\a\AulaSegura-v1.0.0\Scripts"
   ```

3. Ejecute el instalador:
   ```powershell
   .\install-service.ps1
   ```

4. Siga las instrucciones en pantalla

### Paso 5: Verificar Instalación

Después de la instalación, verifique que el servicio esté funcionando:

```powershell
# Abrir PowerShell
Get-Service -Name AulaSeguraService
```

Debe mostrar:
```
Status   Name               DisplayName
------   ----               -----------
Running  AulaSeguraService  AulaSegura Control Web Service
```

### Paso 6: Iniciar la Aplicación

1. Navegue a la carpeta `App` dentro del paquete extraído
2. Haga doble clic en `AulaSegura.App.exe`
3. La aplicación WPF se abrirá
4. Inicie sesión con las credenciales por defecto

---

## 🔑 Credenciales por Defecto

**PRIMER INICIO DE SESIÓN:**
- **Usuario:** `admin`
- **Contraseña:** `SeedAdmin__Password o first-run-admin.txt`

⚠️ **IMPORTANTE:** 
- Cambie la contraseña inmediatamente después del primer inicio de sesión
- Vaya a Configuración → Cambiar Contraseña
- Use una contraseña segura (mínimo 6 caracteres)

---

## 📁 Estructura del Paquete Extraído

```
AulaSegura-v1.0.0/
├── Service/              # Servicio Windows (motor de bloqueo)
│   ├── AulaSegura.Service.exe
│   ├── Data/            # Base de datos SQLite
│   ├── Logs/            # Archivos de log
│   └── Backups/         # Respaldos automáticos
├── App/                 # Aplicación WPF (interfaz gráfica)
│   ├── AulaSegura.App.exe
│   └── ... (archivos de dependencias)
├── Scripts/             # Scripts de instalación
│   ├── install-service.ps1
│   ├── uninstall-service.ps1
│   └── INSTALAR_RAPIDO.ps1
├── Data/                # Datos compartidos (vacío inicialmente)
├── Docs/                # Documentación (si se incluye)
└── README.txt           # Instrucciones rápidas
```

---

## 🛠️ Comandos Útiles para el Administrador

### Gestionar el Servicio

```powershell
# Ver estado del servicio
Get-Service -Name AulaSeguraService

# Iniciar servicio
Start-Service -Name AulaSeguraService

# Detener servicio
Stop-Service -Name AulaSeguraService

# Reiniciar servicio
Restart-Service -Name AulaSeguraService

# Ver detalles del servicio
Get-Service -Name AulaSeguraService | Format-List *
```

### Ver Logs del Servicio

```powershell
# Ver últimas 50 líneas del log
Get-Content "C:\ruta\a\Service\Logs\aulasegura-*.log" -Tail 50

# Ver logs en tiempo real
Get-Content "C:\ruta\a\Service\Logs\aulasegura-*.log" -Wait -Tail 20
```

### Desinstalar el Servicio

```powershell
# Abrir PowerShell como Administrador
cd "C:\ruta\a\Scripts"
.\uninstall-service.ps1
```

---

## ❓ Solución de Problemas

### Problema 1: ".NET no está instalado"

**Síntoma:** Error al ejecutar los scripts o la aplicación

**Solución:**
1. Descargue .NET 8.0 Runtime desde: https://dotnet.microsoft.com/download/dotnet/8.0
2. Instale el runtime
3. Reinicie la computadora
4. Intente la instalación nuevamente

### Problema 2: "Permisos insuficientes"

**Síntoma:** Error al ejecutar scripts de PowerShell

**Solución:**
1. Asegúrese de ejecutar PowerShell **como Administrador**
2. Haga clic derecho en PowerShell → "Ejecutar como administrador"
3. O use: `Run as Administrator` en el menú contextual

### Problema 3: "El servicio no se inicia"

**Síntoma:** El servicio se instala pero no arranca

**Solución:**
```powershell
# Ver logs de errores
Get-EventLog -LogName Application -Source "AulaSeguraService" -Newest 10

# Verificar que el archivo existe
Test-Path "C:\ruta\a\Service\AulaSegura.Service.exe"

# Intentar iniciar manualmente
Start-Service -Name AulaSeguraService -Verbose
```

### Problema 4: "Bloqueo de PowerShell"

**Síntoma:** Los scripts no se ejecutan por política de seguridad

**Solución:**
```powershell
# Ejecutar como Administrador
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass

# Luego ejecutar el script
.\INSTALAR_RAPIDO.ps1
```

### Problema 5: "Archivo hosts no se modifica"

**Síntoma:** El servicio no bloquea sitios web

**Solución:**
1. Verifique que el servicio esté ejecutándose como SYSTEM
2. Revise los permisos del archivo hosts:
   ```
   C:\Windows\System32\drivers\etc\hosts
   ```
3. Consulte los logs del servicio para más detalles

---

## 📞 Soporte Técnico

Para asistencia adicional:

1. **Revise la documentación** en la carpeta `Docs/` (si está incluida)
2. **Consulte los logs** del servicio en `Service\Logs\`
3. **Verifique el estado** del servicio con `Get-Service`

**Documentación disponible:**
- MANUAL_USUARIO.md - Guía para usuarios finales
- MANUAL_TECNICO.md - Documentación técnica detallada
- SEGURIDAD.md - Políticas y recomendaciones de seguridad

---

## ✅ Checklist de Verificación Post-Instalación

Después de instalar, verifique:

- [ ] .NET 8.0 Runtime está instalado
- [ ] Servicio Windows está en estado "Running"
- [ ] Puede iniciar la aplicación WPF
- [ ] Puede iniciar sesión con credenciales admin
- [ ] Cambió la contraseña por defecto
- [ ] El servicio aparece en services.msc
- [ ] Los directorios Data, Logs y Backups existen
- [ ] No hay errores en los logs del servicio

---

## 🔒 Recomendaciones de Seguridad

1. **Cambie la contraseña** inmediatamente después de la instalación
2. **Mantenga actualizado** .NET Runtime
3. **Revise los logs** regularmente
4. **Realice respaldos** periódicos de la base de datos
5. **Restrinja el acceso** físico a la computadora
6. **Use contraseñas fuertes** para cuentas de administrador de Windows

---

## 📊 Información Técnica

- **Versión:** 1.0.0
- **Fecha de compilación:** 2026-04-24
- **Framework:** .NET 8.0
- **Base de datos:** SQLite
- **Arquitectura:** Clean Architecture
- **Patrón UI:** MVVM con WPF
- **Servicio:** Windows Service

---

**Desarrollado por:** Equipo AulaSegura  
**Estado:** ✅ Production Ready  
**Licencia:** Propietaria

---

*Última actualización: 24 de Abril de 2026*
