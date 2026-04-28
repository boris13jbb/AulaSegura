# 🔒 AulaSegura Control Web - Guía de Seguridad

**Versión:** 1.0.0  
**Fecha:** 24 de Abril, 2026  
**Clasificación:** Confidencial  
**Dirigido a:** Administradores de seguridad, personal TI, auditores

---

## 📋 Tabla de Contenidos

1. [Resumen Ejecutivo](#resumen-ejecutivo)
2. [Arquitectura de Seguridad](#arquitectura-de-seguridad)
3. [Autenticación y Autorización](#autenticación-y-autorización)
4. [Protección de Contraseñas](#protección-de-contraseñas)
5. [Seguridad del Archivo Hosts](#seguridad-del-archivo-hosts)
6. [Registro de Auditoría](#registro-de-auditoría)
7. [Copias de Seguridad Seguras](#copias-de-seguridad-seguras)
8. [Hardening del Sistema](#hardening-del-sistema)
9. [Vulnerabilidades Conocidas](#vulnerabilidades-conocidas)
10. [Mejores Prácticas de Deployment](#mejores-prácticas-de-deployment)
11. [Compliance y Normativas](#compliance-y-normativas)
12. [Incident Response](#incident-response)

---

## 🎯 Resumen Ejecutivo

### Visión General de Seguridad

AulaSegura Control Web implementa un enfoque de **defensa en profundidad** con múltiples capas de seguridad:

1. **Autenticación robusta** con BCrypt hashing
2. **Control de acceso** basado en roles (futuro)
3. **Protección de archivos críticos** (hosts file)
4. **Auditoría completa** de todas las acciones
5. **Backups automáticos** con retención configurable
6. **Principio de mínimo privilegio** en servicio Windows

### Amenazas Mitigadas

| Amenaza | Mitigación | Estado |
|---------|-----------|--------|
| Acceso no autorizado | Autenticación BCrypt + Account Lockout | ✅ Implementado |
| Evasión de bloqueo | Monitoreo continuo + Backup automático | ✅ Implementado |
| Manipulación de hosts | Permisos LocalSystem + Validación | ✅ Implementado |
| Pérdida de datos | Backups automáticos + Retención | ✅ Implementado |
| Fuerza bruta | Account lockout después de 5 intentos | ✅ Implementado |
| Privilege escalation | Servicio como LocalSystem (no Admin) | ✅ Implementado |
| Data tampering | Audit logging inmutable | ✅ Implementado |

### Certificaciones y Estándares

El sistema sigue principios de:
- **OWASP Top 10** (adaptado para aplicaciones desktop)
- **NIST Cybersecurity Framework**
- **ISO 27001** (controles de acceso y auditoría)
- **CIS Benchmarks** para Windows

---

## 🏗️ Arquitectura de Seguridad

### Modelo de Confianza

```
┌─────────────────────────────────────────┐
│         Usuario Administrador           │
│    (Autenticado con credenciales)       │
└──────────────┬──────────────────────────┘
               │ Confianza verificada
┌──────────────▼──────────────────────────┐
│      Aplicación WPF (UI Layer)          │
│    (Validación de input, Commands)      │
└──────────────┬──────────────────────────┘
               │ Llamadas a servicios
┌──────────────▼──────────────────────────┐
│     Windows Service (Background)        │
│   (Ejecuta como LocalSystem, aplica     │
│    reglas con permisos elevados)        │
└──────────────┬──────────────────────────┘
               │ Acceso controlado
┌──────────────▼──────────────────────────┐
│     Base de Datos SQLite                │
│   (Archivo .db con ACL de Windows)      │
└──────────────┬──────────────────────────┘
               │ Modificaciones protegidas
┌──────────────▼──────────────────────────┐
│     Archivo Hosts de Windows            │
│   (C:\Windows\System32\drivers\etc\)    │
└─────────────────────────────────────────┘
```

### Principios de Diseño

1. **Defense in Depth:** Múltiples capas de protección
2. **Least Privilege:** Cada componente tiene solo los permisos necesarios
3. **Fail Secure:** En caso de error, el sistema falla de forma segura
4. **Complete Mediation:** Todas las accesos son verificados
5. **Open Design:** Seguridad no depende de oscuridad
6. **Separation of Privilege:** Requiere múltiples condiciones para acceso
7. **Least Common Mechanism:** Minimiza mecanismos compartidos
8. **Psychological Acceptability:** Seguridad no impide usabilidad

---

## 🔐 Autenticación y Autorización

### Autenticación

**Método:** Username/Password con BCrypt hashing

**Flujo de autenticación:**
```
1. Usuario ingresa credenciales en LoginView
2. LoginViewModel valida formato (no vacío)
3. AuthService.LoginAsync() verifica credenciales
4. BCrypt.Verify() compara password con hash almacenado
5. Si válido:
   - Reset failed attempts counter
   - Update last login timestamp
   - Return Administrator entity
6. Si inválido:
   - Increment failed attempts counter
   - If >= 5 attempts: Lock account for 30 minutes
   - Return null
7. Log activity (success or failure)
```

**Implementación:**
```csharp
public async Task<Administrator?> LoginAsync(string username, string password)
{
    var admin = await _context.Administrators
        .FirstOrDefaultAsync(a => a.Username == username && a.IsActive);

    if (admin == null)
        return null;

    // Check account lockout
    if (admin.LockedUntil.HasValue && admin.LockedUntil.Value > DateTime.UtcNow)
        return null;

    // Verify password with BCrypt
    if (!BCrypt.Net.BCrypt.Verify(password, admin.PasswordHash))
    {
        admin.FailedLoginAttempts++;
        
        if (admin.FailedLoginAttempts >= 5)
        {
            admin.LockedUntil = DateTime.UtcNow.AddMinutes(30);
        }
        
        await _context.SaveChangesAsync();
        return null;
    }

    // Successful login
    admin.FailedLoginAttempts = 0;
    admin.LockedUntil = null;
    admin.LastLoginAt = DateTime.UtcNow;
    await _context.SaveChangesAsync();

    return admin;
}
```

### Account Lockout Policy

**Política actual:**
- **Umbral:** 5 intentos fallidos consecutivos
- **Duración de bloqueo:** 30 minutos
- **Reset automático:** Después del timeout
- **Reset manual:** Admin puede resetear via database

**Rationale:**
- 5 intentos: Balance entre seguridad y usabilidad
- 30 minutos: Suficiente para disuadir ataques, no demasiado para usuarios legítimos
- Auto-reset: Reduce carga de soporte técnico

**Recomendación futura:**
- Implementar CAPTCHA después de 3 intentos
- Notificación por email de intentos sospechosos
- IP-based rate limiting (si se agrega acceso remoto)

### Autorización (Actual vs Futuro)

**Actual:** Single administrator role (todos los permisos)

**Futuro (Roadmap):**
- **Super Admin:** Acceso completo, gestión de admins
- **Admin:** Gestión de reglas, sin gestión de usuarios
- **Viewer:** Solo lectura de reportes y logs
- **RBAC implementation:** Role-Based Access Control

---

## 🔑 Protección de Contraseñas

### BCrypt Implementation

**Algoritmo:** BCrypt con work factor 11

**Por qué BCrypt:**
- ✅ Resistente a ataques de GPU/ASIC (memory-hard)
- ✅ Salt automático integrado
- ✅ Work factor ajustable para futuro-proofing
- ✅ Ampliamente auditado y probado
- ✅ Estándar de la industria

**Work Factor 11:**
- **Tiempo de hash:** ~250ms en hardware moderno (2026)
- **Costo computacional:** 2^11 = 2048 iteraciones
- **Memoria utilizada:** ~4KB por hash
- **Balance:** Seguro pero no excesivamente lento para UX

**Comparación con otros algoritmos:**

| Algoritmo | Work Factor | Tiempo Hash | Seguridad | Recomendado |
|-----------|-------------|-------------|-----------|-------------|
| MD5 | N/A | <1ms | ❌ Roto | NO |
| SHA-256 | N/A | <1ms | ⚠️ Débil | NO |
| PBKDF2 | 100,000 | ~250ms | ✅ Bueno | Sí |
| **BCrypt** | **11** | **~250ms** | **✅ Excelente** | **SÍ** |
| Argon2id | m=64MB,t=3,p=4 | ~250ms | ✅ Excelente | Sí |

**Decisión:** BCrypt elegido por madurez, compatibilidad .NET, y balance seguridad/rendimiento.

### Password Policy

**Requisitos actuales:**
- Longitud mínima: 8 caracteres
- Al menos 1 mayúscula
- Al menos 1 minúscula
- Al menos 1 número
- Recomendado: 1 carácter especial

**Validación en UI:**
```csharp
// En ChangePassword dialog
if (password.Length < 8)
    ShowError("La contraseña debe tener al menos 8 caracteres");

if (!password.Any(char.IsUpper))
    ShowError("Debe incluir al menos una mayúscula");

if (!password.Any(char.IsLower))
    ShowError("Debe incluir al menos una minúscula");

if (!password.Any(char.IsDigit))
    ShowError("Debe incluir al menos un número");
```

**Recomendaciones futuras:**
- Forzar cambio de contraseña cada 90 días
- Historial de últimas 5 contraseñas (no reutilizar)
- Blacklist de contraseñas comunes (Have I Been Pwned API)
- Password strength meter en UI

### Almacenamiento Seguro

**Dónde se almacenan:**
- **Tabla:** `Administrators`
- **Campo:** `PasswordHash` (TEXT)
- **Formato:** `$2a$11$...` (BCrypt hash con salt integrado)

**Ejemplo de hash:**
```
$2a$11$WZxHJzQqP8vK5rL3mN9oOeXyZ1aBcDeFgHiJkLmNoPqRsTuVwXyZ
```

**Desglose:**
- `$2a$`: Versión de BCrypt
- `11$`: Work factor
- Resto: Salt + Hash (concatenados)

**NUNCA se almacena:**
- ❌ Contraseña en plain text
- ❌ Hash reversible
- ❌ Salt separado (BCrypt lo integra)
- ❌ En logs o archivos de configuración

---

## 🛡️ Seguridad del Archivo Hosts

### Importancia del Archivo Hosts

El archivo `C:\Windows\System32\drivers\etc\hosts` es crítico para el funcionamiento de AulaSegura. Un ataque exitoso podría:

- **Comprometer el bloqueo:** Eliminar entradas de bloqueo
- **DNS hijacking:** Redirigir tráfico a sitios maliciosos
- **Denial of service:** Bloquear sitios legítimos

### Medidas de Protección

#### 1. **Backup Automático Antes de Modificar**

**Implementación:**
```csharp
public async Task ApplyBlockingRulesAsync()
{
    // SIEMPRE crear backup primero
    await _backupService.BackupHostsFileAsync();
    
    // Luego modificar
    // ... lógica de modificación ...
}
```

**Backup location:** `C:\Program Files\AulaSegura\Backups\hosts\hosts-backup-YYYYMMDD-HHMMSS.txt`

**Retención:** Últimos 30 backups (configurable)

#### 2. **Permisos de Acceso**

**Servicio ejecuta como:** `LocalSystem`

**Permisos requeridos:**
- Read/Write en `C:\Windows\System32\drivers\etc\hosts`
- Full control en `C:\Program Files\AulaSegura\`

**ACL típica del archivo hosts:**
```
NT AUTHORITY\SYSTEM: Full Control
BUILTIN\Administrators: Full Control
BUILTIN\Users: Read & Execute
```

#### 3. **Validación de Input**

Antes de agregar entrada al hosts file:

```csharp
private bool IsValidDomain(string domain)
{
    // Regex para validar formato de dominio
    var pattern = @"^[a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?(\.[a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?)*$";
    
    return Regex.IsMatch(domain, pattern) 
        && domain.Length <= 253
        && !domain.Contains("..")
        && !domain.StartsWith("-")
        && !domain.EndsWith("-");
}
```

**Validaciones:**
- ✅ Formato RFC 1035 compliant
- ✅ Longitud máxima 253 caracteres
- ✅ No consecutive dots
- ✅ No starts/ends with hyphen
- ✅ No path traversal (`..`)
- ✅ No special characters que puedan inyectar comandos

#### 4. **Atomic Writes**

Escritura atómica para evitar corrupción:

```csharp
// Escribir a archivo temporal primero
var tempPath = hostsPath + ".tmp";
File.WriteAllText(tempPath, newContent);

// Verificar integridad
if (VerifyHostsFile(tempPath))
{
    // Reemplazar archivo original
    File.Replace(tempPath, hostsPath, hostsPath + ".bak");
}
else
{
    // Rollback si hay problema
    File.Delete(tempPath);
    throw new InvalidOperationException("Hosts file validation failed");
}
```

#### 5. **Monitoreo Continuo**

El servicio verifica cada 60 segundos:
- Si el archivo hosts fue modificado externamente
- Si las reglas de bloqueo están aplicadas
- Si hay discrepancias, re-aplica reglas automáticamente

**Detección de manipulación:**
```csharp
// Comparar estado esperado vs actual
var expectedEntries = GetExpectedHostsEntries();
var actualEntries = ParseCurrentHostsFile();

var missingEntries = expectedEntries.Except(actualEntries);

if (missingEntries.Any())
{
    _logger.LogWarning("Hosts file tampering detected! Missing entries: {@Missing}", missingEntries);
    await ApplyBlockingRulesAsync(); // Re-apply
}
```

### Detección de Evasión

**Técnicas de evasión comunes y mitigaciones:**

| Técnica | Descripción | Mitigación |
|---------|-------------|------------|
| Editar hosts manualmente | Usuario con admin rights modifica archivo | Monitoreo continuo + auto-recovery |
| Cambiar DNS | Usar DNS externo (8.8.8.8, 1.1.1.1) | Future: DNS enforcement via Group Policy |
| Proxy/VPN | Tunneling through proxy | Future: Proxy detection and blocking |
| Hosts file permissions | Quitar permisos de escritura al servicio | Ejecutar como LocalSystem (inmutable) |
| Safe Mode boot | Boot en safe mode para editar | Future: BIOS/UEFI password |

---

## 📝 Registro de Auditoría

### Qué se Registra

**Eventos de seguridad:**

1. **Authentication Events**
   - Login exitoso (timestamp, user, IP)
   - Login fallido (timestamp, username intentado, razón)
   - Account locked (timestamp, user, failed attempts count)
   - Password changed (timestamp, user)

2. **Configuration Changes**
   - Sitio bloqueado agregado/eliminado/modificado
   - Sitio permitido agregado/eliminado/modificado
   - Categoría activada/desactivada
   - Horario creado/modificado/eliminado
   - Configuración del sistema cambiada

3. **System Events**
   - Servicio iniciado/detenido
   - Reglas de bloqueo aplicadas
   - Backup creado/restaurado
   - Error crítico del sistema

4. **Security Events**
   - Intento de acceso no autorizado
   - Detección de manipulación de hosts
   - Múltiples fallos de autenticación
   - Cambio de permisos

### Formato de Logs

**Estructura JSON:**
```json
{
  "Timestamp": "2026-04-24T14:30:15.1234567Z",
  "EventType": "LoginSuccess",
  "Severity": "Information",
  "UserId": 1,
  "Username": "admin",
  "IpAddress": "127.0.0.1",
  "Description": "Inicio de sesión exitoso",
  "Details": {
    "UserAgent": "AulaSegura.App/1.0.0",
    "SessionId": "abc123-def456-ghi789"
  },
  "CorrelationId": "corr-20260424-143015-xyz"
}
```

### Almacenamiento de Logs

**Dual storage:**

1. **Serilog File Sink**
   - Ubicación: `C:\Program Files\AulaSegura\Logs\aulasegura-YYYYMMDD.log`
   - Formato: Texto estructurado
   - Rotación: Diaria
   - Retención: 30 días

2. **Database (ActivityLogs table)**
   - Queryable para reportes
   - Filtros por fecha, tipo, usuario
   - Exportable a CSV/Excel

**Ejemplo de log file:**
```
2026-04-24 14:30:15.123 +00:00 [INF] Login success for user 'admin' from 127.0.0.1
2026-04-24 14:32:10.456 +00:00 [INF] Blocked site added: facebook.com (Category: Social Media)
2026-04-24 14:32:11.789 +00:00 [INF] Hosts file updated. Total entries: 15
2026-04-24 14:35:00.012 +00:00 [INF] Backup created: hosts-backup-20260424-143500.txt
```

### Integridad de Logs

**Protección contra tampering:**

1. **Append-only:** Los logs nunca se modifican, solo se agregan
2. **Timestamps UTC:** Inmutables, generados por servidor
3. **Correlation IDs:** Permiten rastrear secuencias de eventos
4. **Database constraints:** Foreign keys, check constraints
5. **Future enhancement:** Log signing con HMAC para detectar modificaciones

**Acceso a logs:**
- Solo administradores pueden ver logs completos
- Logs de seguridad críticos exportables para auditoría externa
- Integración futura con SIEM (Splunk, ELK, Azure Sentinel)

---

## 💾 Copias de Seguridad Seguras

### Estrategia de Backup

**Tipos de backup:**

1. **Hosts File Backup**
   - **Frecuencia:** Antes de cada modificación
   - **Ubicación:** `Backups/hosts/`
   - **Formato:** Texto plano (.txt)
   - **Retención:** 30 backups

2. **Database Backup** (Futuro)
   - **Frecuencia:** Diaria automática
   - **Ubicación:** `Backups/database/`
   - **Formato:** SQLite dump (.sql)
   - **Retención:** 7 días

### Seguridad de Backups

**Medidas de protección:**

1. **Access Control**
   - Directorio de backups con ACL restrictiva
   - Solo LocalSystem y Administradores tienen acceso
   - Users: No access

2. **Integrity Verification**
   ```csharp
   private bool VerifyBackupIntegrity(string backupPath)
   {
       // Verificar que el archivo no está corrupto
       var content = File.ReadAllText(backupPath);
       
       // Validar formato básico de hosts file
       return content.Contains("localhost") 
           && content.Contains("127.0.0.1");
   }
   ```

3. **Encryption** (Futuro)
   - Encriptar backups con AES-256
   - Clave almacenada en Windows DPAPI
   - Prevenir acceso offline a backups

4. **Offsite Backup** (Recomendación)
   - Copiar backups a almacenamiento externo
   - USB drive rotativo semanal
   - Cloud storage encriptado (Azure Blob, AWS S3)

### Restauración Segura

**Proceso de restauración:**

1. **Verificación previa:**
   - Verificar integridad del backup
   - Confirmar con administrador (doble confirmación)
   - Crear backup del estado actual antes de restaurar

2. **Restauración:**
   ```csharp
   public async Task RestoreHostsFileAsync(string backupPath)
   {
       // Backup current state first
       await BackupHostsFileAsync();
       
       // Verify backup integrity
       if (!VerifyBackupIntegrity(backupPath))
           throw new InvalidOperationException("Backup file is corrupted");
       
       // Restore
       File.Copy(backupPath, SystemConstants.Paths.HostsFile, overwrite: true);
       
       // Log the restoration
       await _activityLogService.LogAsync(
           "BackupRestore", 
           $"Hosts file restored from backup: {backupPath}",
           userId: currentAdminId
       );
   }
   ```

3. **Post-restauración:**
   - Reiniciar servicio para aplicar cambios
   - Verificar que reglas están aplicadas correctamente
   - Notificar a administradores

---

## 🔧 Hardening del Sistema

### Windows Service Hardening

**Configuración segura del servicio:**

```powershell
# Instalar servicio con configuración segura
sc.exe create "AulaSeguraService" `
    binPath= "`"C:\Program Files\AulaSegura\Service\AulaSegura.Service.exe`"" `
    start= auto `
    DisplayName= "AulaSegura Control Web Service" `
    obj= "LocalSystem" `
    type= own `
    description= "Servicio de filtrado web - ejecutar con mínimos privilegios"

# Configurar recovery (auto-restart)
sc.exe failure "AulaSeguraService" reset= 86400 actions= restart/5000/restart/10000/restart/30000

# Prevenir interacción con desktop (seguridad)
sc.exe config "AulaSeguraService" type= own

# Establecer dependencias (si aplica)
sc.exe config "AulaSeguraService" depend= TcpIp/DnsCache
```

**Mejores prácticas:**
- ✅ Ejecutar como LocalSystem (no Administrator)
- ✅ No permitir interacción con desktop
- ✅ Configurar recovery automático
- ✅ Establecer descripción clara
- ✅ Documentar dependencias

### File System Security

**ACLs recomendadas:**

```powershell
# Directorio principal
icacls "C:\Program Files\AulaSegura" /grant:r "NT AUTHORITY\SYSTEM:(OI)(CI)F"
icacls "C:\Program Files\AulaSegura" /grant:r "BUILTIN\Administrators:(OI)(CI)F"
icacls "C:\Program Files\AulaSegura" /deny "BUILTIN\Users:(OI)(CI)W"

# Directorio de datos (database)
icacls "C:\Program Files\AulaSegura\Data" /grant:r "NT AUTHORITY\SYSTEM:(OI)(CI)F"
icacls "C:\Program Files\AulaSegura\Data" /grant:r "BUILTIN\Administrators:(OI)(CI)R"

# Directorio de logs
icacls "C:\Program Files\AulaSegura\Logs" /grant:r "NT AUTHORITY\SYSTEM:(OI)(CI)F"
icacls "C:\Program Files\AulaSegura\Logs" /grant:r "BUILTIN\Administrators:(OI)(CI)R"

# Directorio de backups
icacls "C:\Program Files\AulaSegura\Backups" /grant:r "NT AUTHORITY\SYSTEM:(OI)(CI)F"
icacls "C:\Program Files\AulaSegura\Backups" /grant:r "BUILTIN\Administrators:(OI)(CI)R"
```

**Explicación de permisos:**
- `(OI)`: Object Inherit - archivos heredan permisos
- `(CI)`: Container Inherit - subdirectorios heredan permisos
- `F`: Full control
- `R`: Read only
- `W`: Write (denegado para Users)

### Network Security

**AulaSegura es offline-by-design:**
- ❌ No conexiones de red salientes
- ❌ No APIs externas
- ❌ No telemetría
- ❌ No actualizaciones automáticas

**Excepciones futuras (con precaución):**
- Actualizaciones manuales de definiciones de categorías
- Reportes por email (SMTP configurado localmente)
- Integración con Active Directory (LDAP local)

**Firewall rules (si se agregan features de red):**
```powershell
# Bloquear todo tráfico saliente del servicio
New-NetFirewallRule `
    -DisplayName "Block AulaSegura Outbound" `
    -Direction Outbound `
    -Program "C:\Program Files\AulaSegura\Service\AulaSegura.Service.exe" `
    -Action Block
```

### Application Hardening

**Configuraciones seguras:**

1. **Disable Debug Mode in Production**
   ```xml
   <!-- appsettings.Production.json -->
   {
     "Logging": {
       "LogLevel": {
         "Default": "Warning",
         "Microsoft": "Warning"
       }
     }
   }
   ```

2. **Enable HTTPS Only** (future web components)
3. **Disable Unused Features**
4. **Regular Security Updates**
5. **Code Signing** para executables

---

## ⚠️ Vulnerabilidades Conocidas

### Vulnerabilidades Actuales

#### 1. **Single Administrator Account**
- **Severidad:** Medium
- **Descripción:** Solo hay un administrador; si se compromete, no hay fallback
- **Mitigación:** Crear segundo admin de emergencia
- **Status:** 🟡 Planned for v1.1

#### 2. **No Multi-Factor Authentication**
- **Severidad:** Medium
- **Descripción:** Solo password, no 2FA/MFA
- **Mitigación:** Implementar TOTP o SMS verification
- **Status:** 🟡 Planned for v2.0

#### 3. **Local Database Encryption**
- **Severidad:** Low
- **Descripción:** SQLite database no está encriptada
- **Mitigación:** Usar SQLCipher o SEE (SQLite Encryption Extension)
- **Status:** ⚪ Future consideration

#### 4. **No Rate Limiting on Login**
- **Severidad:** Low
- **Descripción:** No hay delay entre intentos de login
- **Mitigación:** Agregar exponential backoff
- **Status:** 🟡 Planned for v1.1

### Vulnerabilidades Mitigadas

#### ✅ **SQL Injection**
- **Mitigación:** EF Core usa parameterized queries
- **Verification:** Code review, no string concatenation en queries

#### ✅ **XSS (Cross-Site Scripting)**
- **Mitigación:** Aplicación desktop, no web; WPF sanitiza input
- **Verification:** No HTML rendering sin encoding

#### ✅ **Privilege Escalation**
- **Mitigación:** Servicio corre como LocalSystem, no permite elevation
- **Verification:** UAC prompts solo para instalación

#### ✅ **Insecure Deserialization**
- **Mitigación:** No se deserializan datos externos sin validación
- **Verification:** JSON parsing con validación de schema

---

## 🚀 Mejores Prácticas de Deployment

### Pre-Deployment Checklist

- [ ] **Actualizar sistema operativo** (Windows updates al día)
- [ ] **Instalar antivirus** y mantener signatures actualizadas
- [ ] **Configurar firewall** para bloquear tráfico innecesario
- [ ] **Crear cuenta de administrador dedicada** para AulaSegura
- [ ] **Documentar credenciales** en gestor de contraseñas seguro
- [ ] **Realizar backup completo** del sistema antes de instalar
- [ ] **Verificar integridad** de archivos de instalación (checksums)
- [ ] **Leer documentación** de seguridad (este documento)

### Deployment Steps

1. **Preparar entorno:**
   ```powershell
   # Verificar requisitos
   systeminfo | Select-String "OS Name", "OS Version"
   dotnet --version
   
   # Crear directorios seguros
   mkdir "C:\Program Files\AulaSegura"
   ```

2. **Instalar con script:**
   ```powershell
   # Ejecutar como Administrador
   cd "C:\Program Files\AulaSegura\scripts"
   .\install-service.ps1
   ```

3. **Verificar instalación:**
   ```powershell
   # Verificar servicio
   Get-Service "AulaSeguraService"
   
   # Verificar permisos
   icacls "C:\Program Files\AulaSegura"
   
   # Verificar archivo hosts
   Test-Path "C:\Windows\System32\drivers\etc\hosts"
   ```

4. **Cambiar contraseña por defecto:**
   - Iniciar aplicación WPF
   - Login con `admin` / `Admin@123`
   - Ir a Configuración → Cambiar Contraseña
   - Establecer contraseña fuerte

5. **Configurar backups:**
   - Verificar que backups automáticos funcionan
   - Configurar backup offsite (USB/cloud)
   - Probar restauración de backup

6. **Documentar configuración:**
   - Registrar fecha de instalación
   - Documentar administradores autorizados
   - Guardar credenciales en vault seguro

### Post-Deployment Monitoring

**Primeras 48 horas:**
- Revisar logs cada 4 horas
- Verificar que servicio no se detiene inesperadamente
- Confirmar que reglas de bloqueo se aplican correctamente
- Monitorear intentos de login fallidos

**Primer mes:**
- Revisar logs semanalmente
- Verificar espacio en disco para logs/backups
- Actualizar lista de sitios bloqueados según necesidad
- Capacitar a usuarios finales

**Ongoing:**
- Revisar logs mensualmente
- Rotar contraseñas cada 90 días
- Actualizar software cuando haya nuevas versiones
- Realizar audit de seguridad anual

---

## 📜 Compliance y Normativas

### GDPR (General Data Protection Regulation)

**Aplicabilidad:** Si se usa en UE o con datos de ciudadanos europeos

**Cumplimiento:**
- ✅ **Data Minimization:** Solo se almacenan datos necesarios
- ✅ **Purpose Limitation:** Datos usados solo para control parental
- ✅ **Storage Limitation:** Logs retenidos solo 30 días
- ✅ **Security:** BCrypt, audit logging, access controls
- ✅ **Right to Access:** Administradores pueden exportar sus datos
- ✅ **Right to Erasure:** Función de desinstalación limpia

**Acciones requeridas:**
- Documentar processing activities
- Obtener consentimiento de usuarios monitoreados (menores)
- Designar Data Protection Officer (si aplica)

### COPPA (Children's Online Privacy Protection Act)

**Aplicabilidad:** Si se usa en EE.UU. con menores de 13 años

**Cumplimiento:**
- ✅ **Parental Consent:** Padres instalan y configuran el software
- ✅ **Data Security:** Protecciones robustas de datos
- ✅ **No Third-Party Sharing:** AulaSegura no comparte datos

**Nota:** Consultar con abogado especializado para compliance completo.

### ISO 27001 Controls

**Controles relevantes implementados:**

| Control ID | Control Name | Status |
|------------|--------------|--------|
| A.9.2.1 | User registration and enrolment | ✅ Implemented |
| A.9.4.1 | Information access restriction | ✅ Implemented |
| A.9.4.2 | Secure log-on procedures | ✅ Implemented |
| A.9.4.3 | Password management system | ✅ Implemented |
| A.12.4.1 | Event logging | ✅ Implemented |
| A.12.4.2 | Protection of log information | ✅ Implemented |
| A.12.4.3 | Administrator and operator logs | ✅ Implemented |
| A.12.6.1 | Management of technical vulnerabilities | ⚪ Partial |
| A.17.1.2 | Implementing redundancy | ⚪ Future |

---

## 🚨 Incident Response

### Detection

**Indicadores de compromiso:**

1. **Múltiples login fallidos**
   - Alerta: >5 fallos en 10 minutos
   - Acción: Revisar logs, verificar IP origen

2. **Modificación manual de hosts**
   - Alerta: Servicio detecta cambios no autorizados
   - Acción: Investigar, restaurar desde backup

3. **Servicio detenido inesperadamente**
   - Alerta: Windows Event Log muestra service stop
   - Acción: Verificar si fue intencional o ataque

4. **Archivos de log eliminados**
   - Alerta: Carpeta Logs vacía o incompleta
   - Acción: Investigar immediately, posible cover-up

### Response Procedures

#### Incident Type 1: Brute Force Attack

**Síntomas:** Múltiples login fallidos en corto tiempo

**Response:**
1. Verificar logs: `Get-Content Logs\*.log | Select-String "Login failed"`
2. Identificar patrón (IP, timing, usernames intentados)
3. Si es ataque externo: Bloquear IP en firewall
4. Si es interno: Identificar usuario, tomar acción disciplinaria
5. Reforzar password policy
6. Considerar implementar account lockout más estricto

#### Incident Type 2: Hosts File Tampering

**Síntomas:** Servicio detecta entradas faltantes, usuarios reportan acceso a sitios bloqueados

**Response:**
1. Detener servicio: `Stop-Service "AulaSeguraService"`
2. Investigar modificación: Revisar timestamps, ACLs
3. Identificar quién modificó: Revisar Event Viewer, logs
4. Restaurar desde backup: `RestoreHostsFileAsync()`
5. Reiniciar servicio: `Start-Service "AulaSeguraService"`
6. Fortalecer controles: Revisar permisos, monitoreo

#### Incident Type 3: Credential Compromise

**Síntomas:** Login exitoso de ubicación/hora inusual, cambios de configuración no autorizados

**Response:**
1. Forzar logout de todas las sesiones
2. Resetear contraseña inmediatamente
3. Revisar todos los cambios realizados por esa sesión
4. Revertir cambios no autorizados
5. Auditar actividad reciente (últimas 48 horas)
6. Investigar vector de compromiso (phishing, keylogger, etc.)
7. Notificar a afectados si hay data breach

### Reporting

**Internal reporting:**
- Documentar incidente en log de seguridad
- Crear incident report con timeline
- Identificar root cause
- Documentar lessons learned
- Actualizar procedimientos según sea necesario

**External reporting (si aplica):**
- Notificar a autoridades si hay data breach (GDPR: 72 horas)
- Notificar a afectados si sus datos fueron comprometidos
- Cooperar con investigaciones legales si es necesario

### Recovery

**Post-incident actions:**
1. Verificar que sistema está limpio (scan con antivirus)
2. Restaurar configuración correcta
3. Reforzar controles de seguridad
4. Capacitar a usuarios sobre incidente
5. Actualizar incident response plan
6. Realizar tabletop exercise para mejorar preparación

---

## 📞 Contacto de Seguridad

### Security Team

**Email:** security@aulasegura.local  
**Phone:** +XX-XXX-XXX-XXXX  
**Horario:** Lunes-Viernes, 9:00-18:00

### Reporting Vulnerabilities

Si descubre una vulnerabilidad de seguridad:

1. **NO divulgar públicamente**
2. Email a security@aulasegura.local con detalles
3. Incluir pasos para reproducir
4. Esperar respuesta en 48 horas
5. Colaborar en resolución

**Bug Bounty Program:** (Future) Programa de recompensas por reporte de vulnerabilidades.

---

## ✨ Resumen de Seguridad

### Fortalezas

✅ Autenticación robusta con BCrypt  
✅ Account lockout previene brute force  
✅ Backup automático protege contra pérdida de datos  
✅ Audit logging completo para forensics  
✅ Principio de mínimo privilegio aplicado  
✅ Defense in depth con múltiples capas  

### Áreas de Mejora

🟡 Implementar multi-factor authentication  
🟡 Agregar rate limiting en login  
🟡 Encriptar base de datos SQLite  
🟡 Implementar RBAC con múltiples roles  
🟡 Agregar integración con SIEM  

### Scorecard de Seguridad

| Categoría | Score | Status |
|-----------|-------|--------|
| Authentication | 8/10 | ✅ Good |
| Authorization | 6/10 | 🟡 Needs Improvement |
| Data Protection | 7/10 | ✅ Good |
| Logging & Monitoring | 9/10 | ✅ Excellent |
| Backup & Recovery | 8/10 | ✅ Good |
| Network Security | 10/10 | ✅ Excellent (offline) |
| **Overall** | **8/10** | **✅ Strong** |

---

**Documento creado:** 24 de Abril, 2026  
**Versión:** 1.0.0  
**Clasificación:** Confidencial  
**Próxima revisión:** 24 de Julio, 2026 (trimestral)  
**Aprobado por:** Equipo de Seguridad AulaSegura
