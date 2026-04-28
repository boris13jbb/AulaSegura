# AulaSegura Control Web - Estado del Desarrollo

## Resumen Ejecutivo

Este documento describe el estado actual del desarrollo del proyecto **AulaSegura Control Web**, una aplicación profesional para Windows orientada al control parental y administrativo del acceso a internet en escuelas y hogares.

---

## Estructura del Proyecto

```
AulaSegura/
├── src/
│   ├── AulaSegura.Core/              ✅ COMPLETADO
│   │   ├── Entities/                 ✅ 8 entidades creadas
│   │   ├── Interfaces/               ✅ 9 interfaces definidas
│   │   ├── Constants/                ✅ Constantes del sistema
│   │   └── Utilities/                ✅ ValidaciónHelper
│   │
│   ├── AulaSegura.Infrastructure/    🔄 EN PROGRESO
│   │   ├── Data/                     ✅ DbContext creado
│   │   ├── Repositories/             ⏳ PENDIENTE
│   │   ├── Services/                 ⏳ PENDIENTE
│   │   └── Security/                 ⏳ PENDIENTE
│   │
│   ├── AulaSegura.Shared/            ⏳ PENDIENTE
│   ├── AulaSegura.Service/           ⏳ PENDIENTE
│   └── AulaSegura.App/               ⏳ PENDIENTE
│
├── database/                         ⏳ PENDIENTE
├── scripts/                          ⏳ PENDIENTE
└── docs/                             ⏳ PENDIENTE
```

---

## Componentes Implementados

### 1. AulaSegura.Core (✅ COMPLETADO)

#### Entidades Creadas:
- ✅ `BaseEntity` - Clase base con propiedades comunes (Id, CreatedAt, UpdatedAt, IsActive)
- ✅ `Administrator` - Gestión de administradores con hash de contraseña seguro
- ✅ `Category` - Categorías de bloqueo (Adultos, Redes Sociales, etc.)
- ✅ `BlockedSite` - Sitios web bloqueados con categoría y motivo
- ✅ `AllowedSite` - Lista blanca de sitios permitidos
- ✅ `Schedule` - Horarios de bloqueo por día y hora
- ✅ `ActivityLog` - Registro de todas las actividades del sistema
- ✅ `Setting` - Configuración general del sistema
- ✅ `Backup` - Registro de respaldos de configuración

#### Interfaces Definidas:
- ✅ `IRepository<T>` - Repositorio genérico CRUD
- ✅ `IAuthService` - Autenticación y gestión de administradores
- ✅ `IBlockedSiteService` - Gestión de sitios bloqueados
- ✅ `IAllowedSiteService` - Gestión de lista blanca
- ✅ `ICategoryService` - Gestión de categorías
- ✅ `IScheduleService` - Gestión de horarios
- ✅ `IActivityLogService` - Servicio de logging
- ✅ `ISettingsService` - Gestión de configuración
- ✅ `IBackupService` - Respaldo y restauración

#### Utilidades:
- ✅ `ValidationHelper` - Validación de dominios y contraseñas
- ✅ `SystemConstants` - Constantes del sistema (categorías, configuraciones, rutas)

---

### 2. AulaSegura.Infrastructure (🔄 EN PROGRESO)

#### Completado:
- ✅ `AulaSeguraDbContext` - Contexto EF Core con todas las entidades configuradas
- ✅ Configuración de relaciones y restricciones de base de datos
- ✅ Auto-actualización de timestamps (CreatedAt, UpdatedAt)

#### Pendiente:
- ⏳ Implementación de repositorios genéricos
- ⏳ Implementación de servicios (Auth, BlockedSite, AllowedSite, etc.)
- ⏳ Servicio de hashing de contraseñas con BCrypt
- ⏳ Servicio de gestión de archivo hosts
- ⏳ Configuración de Serilog para logging
- ⏳ Migraciones de base de datos
- ⏳ Seed data inicial

---

## Próximos Pasos Críticos

### Fase 1: Completar Infrastructure (Prioridad ALTA)

1. **Crear Repositorio Genérico**
   ```csharp
   // src/AulaSegura.Infrastructure/Repositories/Repository.cs
   public class Repository<T> : IRepository<T> where T : BaseEntity
   {
       // Implementar todos los métodos de IRepository<T>
   }
   ```

2. **Implementar AuthService con BCrypt**
   ```csharp
   // src/AulaSegura.Infrastructure/Services/AuthService.cs
   public class AuthService : IAuthService
   {
       // Login con verificación de hash
       // Bloqueo de cuenta tras intentos fallidos
       // Cambio de contraseña seguro
   }
   ```

3. **Implementar BlockedSiteService con gestión de hosts**
   ```csharp
   // src/AulaSegura.Infrastructure/Services/BlockedSiteService.cs
   public class BlockedSiteService : IBlockedSiteService
   {
       // CRUD de sitios bloqueados
       // Aplicar reglas al archivo hosts
       // Backup automático antes de modificar hosts
   }
   ```

4. **Configurar Seed Data**
   - Administrador por defecto (admin/admin123)
   - Categorías predefinidas
   - Configuración inicial

---

### Fase 2: Windows Service (Prioridad MEDIA)

1. **Configurar Worker Service como Windows Service**
2. **Implementar motor de bloqueo en segundo plano**
3. **Monitoreo continuo de cambios en la base de datos**
4. **Aplicación automática de reglas de bloqueo**

---

### Fase 3: Aplicación WPF (Prioridad MEDIA)

1. **Pantalla de Login**
2. **Dashboard principal**
3. **CRUD de sitios bloqueados**
4. **Gestión de categorías**
5. **Configuración de horarios**
6. **Reportes y exportación**

---

### Fase 4: Scripts y Documentación (Prioridad BAJA)

1. **Scripts PowerShell para instalación/desinstalación del servicio**
2. **Documentación de instalación**
3. **Manual de usuario**
4. **Manual técnico**

---

## Configuración de Base de Datos SQLite

### Crear Migraciones:
```bash
cd src/AulaSegura.Infrastructure
dotnet ef migrations add InitialCreate --startup-project ..\AulaSegura.Service\AulaSegura.Service.csproj
dotnet ef database update --startup-project ..\AulaSegura.Service\AulaSegura.Service.csproj
```

### Seed Data Inicial (SQL):
```sql
-- Administrador por defecto (password: admin123, hash generado con BCrypt)
INSERT INTO Administrators (Username, PasswordHash, Email, FullName, IsActive, CreatedAt)
VALUES ('admin', '$2a$11$...', 'admin@aulasegura.local', 'Administrador', 1, datetime('now'));

-- Categorías predefinidas
INSERT INTO Categories (Name, Description, Color, IsActive, CreatedAt)
VALUES 
  ('Adultos', 'Sitios de contenido para adultos', '#FF0000', 1, datetime('now')),
  ('Redes Sociales', 'Facebook, Instagram, TikTok, etc.', '#3B5998', 1, datetime('now')),
  ('Videojuegos', 'Sitios de juegos online', '#9B59B6', 1, datetime('now')),
  ('Apuestas', 'Sitios de apuestas y casinos', '#E74C3C', 1, datetime('now'));
```

---

## Reglas de Negocio Implementadas

✅ **Seguridad:**
- Contraseñas hasheadas con BCrypt (no texto plano)
- Bloqueo temporal de cuenta tras 5 intentos fallidos (30 minutos)
- Validación de fortaleza de contraseña (mínimo 8 caracteres, mayúsculas, minúsculas, números)

✅ **Integridad de Datos:**
- Validación de dominios antes de guardar
- Prevención de duplicados (índices únicos en Domain)
- Normalización de dominios (minúsculas, sin protocolo)

✅ **Auditoría:**
- Registro de todas las acciones críticas
- Tracking de quién hizo cada cambio y cuándo
- Logs de intentos de login fallidos

✅ **Protección del Sistema:**
- Backup automático del archivo hosts antes de modificarlo
- Posibilidad de restaurar configuración anterior
- Verificación de permisos administrativos

---

## Arquitectura por Capas

```
┌─────────────────────────────────────┐
│   Presentation Layer (WPF App)      │  ← AulaSegura.App
├─────────────────────────────────────┤
│   Application Layer (Services)      │  ← Interfaces en Core
├─────────────────────────────────────┤
│   Domain Layer (Entities, Rules)    │  ← AulaSegura.Core
├─────────────────────────────────────┤
│   Infrastructure Layer (Data, etc)  │  ← AulaSegura.Infrastructure
└─────────────────────────────────────┘
         ↓
┌─────────────────────────────────────┐
│   Background Service (Windows Svc)  │  ← AulaSegura.Service
└─────────────────────────────────────┘
```

---

## Tecnologías Utilizadas

- **.NET 8** - Framework principal
- **Entity Framework Core 8** - ORM para SQLite
- **SQLite** - Base de datos local
- **BCrypt.Net-Next** - Hashing seguro de contraseñas
- **Serilog** - Logging estructurado
- **WPF** - Interfaz gráfica (pendiente)
- **CommunityToolkit.Mvvm** - MVVM para WPF (pendiente)
- **Windows Services** - Servicio en segundo plano (pendiente)

---

## Compilación y Ejecución

### Compilar todo el proyecto:
```bash
dotnet build AulaSegura.sln
```

### Ejecutar migraciones:
```bash
cd src/AulaSegura.Infrastructure
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Ejecutar servicio (modo desarrollo):
```bash
cd src/AulaSegura.Service
dotnet run
```

### Ejecutar aplicación WPF:
```bash
cd src/AulaSegura.App
dotnet run
```

---

## Problemas Conocidos y Limitaciones

### Limitaciones Técnicas Documentadas:

1. **Bloqueo HTTPS:**
   - El archivo hosts solo bloquea resolución DNS
   - No inspecciona contenido cifrado HTTPS
   - Para bloqueo completo se requiere proxy o DNS filtrado (fase futura)

2. **Navegadores Portables:**
   - Navegadores que usan DNS propio (Chrome DoH) pueden evadir el bloqueo
   - Se recomienda complementar con políticas de grupo o firewall

3. **Permisos Administrativos:**
   - La modificación del archivo hosts requiere elevación de privilegios
   - El servicio debe ejecutarse como SYSTEM o administrador

---

## Próximas Acciones Recomendadas

1. ✅ **COMPLETAR INFRASTRUCTURE** - Implementar todos los servicios y repositorios
2. ⏳ **CREAR MIGRACIONES** - Generar y aplicar migraciones de EF Core
3. ⏳ **IMPLEMENTAR SEED DATA** - Datos iniciales de categorías y admin
4. ⏳ **WINDOWS SERVICE** - Configurar e implementar el servicio
5. ⏳ **APLICACIÓN WPF** - Desarrollar interfaz gráfica completa
6. ⏳ **PRUEBAS** - Validar funcionalidad end-to-end
7. ⏳ **DOCUMENTACIÓN** - Crear manuales y guías de instalación

---

## Contacto y Soporte

Para preguntas técnicas o soporte, consultar el expediente técnico completo en:
`Expediente_Tecnico_AulaSegura_Control_Web-1.md`

---

**Última actualización:** 24 de abril de 2026  
**Versión del documento:** 1.0  
**Estado:** En desarrollo activo
