# 🔧 AulaSegura Control Web - Manual Técnico

**Versión:** 1.0.0  
**Fecha:** 24 de Abril, 2026  
**Dirigido a:** Desarrolladores, administradores de sistemas, personal TI

---

## 📋 Tabla de Contenidos

1. [Arquitectura del Sistema](#arquitectura-del-sistema)
2. [Tecnologías Utilizadas](#tecnologías-utilizadas)
3. [Estructura del Proyecto](#estructura-del-proyecto)
4. [Base de Datos](#base-de-datos)
5. [Servicios e Interfaces](#servicios-e-interfaces)
6. [Windows Service](#windows-service)
7. [Aplicación WPF](#aplicación-wpf)
8. [Seguridad](#seguridad)
9. [Extensibilidad](#extensibilidad)
10. [Desarrollo y Compilación](#desarrollo-y-compilación)

---

## 🏗️ Arquitectura del Sistema

### Arquitectura en Capas (Clean Architecture)

AulaSegura sigue el patrón **Clean Architecture** con separación clara de responsabilidades:

```
┌─────────────────────────────────────────┐
│         Presentation Layer              │
│    (AulaSegura.App - WPF UI)            │
└──────────────┬──────────────────────────┘
               │ Depends on
┌──────────────▼──────────────────────────┐
│         Application Layer               │
│    (AulaSegura.Service - Windows Svc)   │
└──────────────┬──────────────────────────┘
               │ Depends on
┌──────────────▼──────────────────────────┐
│      Infrastructure Layer               │
│  (Data Access, External Services)       │
└──────────────┬──────────────────────────┘
               │ Depends on
┌──────────────▼──────────────────────────┐
│           Core Layer                    │
│   (Entities, Interfaces, Business Rules)│
└─────────────────────────────────────────┘
```

**Principio de Dependencia:** Las capas externas dependen de las internas, nunca al revés.

### Componentes Principales

#### 1. **Core Layer** (`AulaSegura.Core`)
- **Responsabilidad:** Entidades de dominio, interfaces, reglas de negocio
- **Dependencias:** Ninguna (capa más interna)
- **Contenido:**
  - Entities: Administrator, BlockedSite, AllowedSite, Category, etc.
  - Interfaces: IRepository<T>, IAuthService, IBlockedSiteService, etc.
  - Constants: SystemConstants, ValidationHelper

#### 2. **Infrastructure Layer** (`AulaSegura.Infrastructure`)
- **Responsabilidad:** Implementación de acceso a datos, servicios externos
- **Dependencias:** Core
- **Contenido:**
  - Data: AulaSeguraDbContext (EF Core), Repository<T>
  - Services: AuthService, BlockedSiteService, BackupService, etc.
  - Seeding: DatabaseSeeder

#### 3. **Service Layer** (`AulaSegura.Service`)
- **Responsabilidad:** Windows Service background worker
- **Dependencias:** Core, Infrastructure
- **Contenido:**
  - BlockingWorker: BackgroundService con ciclo de monitoreo
  - Program.cs: Configuración de host, Serilog, DI

#### 4. **Application Layer** (`AulaSegura.App`)
- **Responsabilidad:** Interfaz gráfica WPF
- **Dependencias:** Core, Infrastructure
- **Contenido:**
  - Views: LoginView, MainWindow (XAML)
  - ViewModels: LoginViewModel (MVVM)
  - Converters: ValueConverters
  - App.xaml.cs: DI configuration, startup logic

#### 5. **Shared Layer** (`AulaSegura.Shared`)
- **Responsabilidad:** DTOs, utilidades compartidas
- **Dependencias:** Ninguna o mínimas
- **Contenido:** (Pendiente de implementación completa)

---

## 💻 Tecnologías Utilizadas

### Framework y Lenguaje
- **.NET 8.0** - Framework principal
- **C# 12** - Lenguaje de programación
- **WPF** - Windows Presentation Foundation para UI

### Base de Datos
- **SQLite** - Motor de base de datos embebido
- **Entity Framework Core 8.0.11** - ORM para acceso a datos
- **Fluent API** - Configuración de entidades

### Seguridad
- **BCrypt.Net-Next 4.0.3** - Hashing de contraseñas (work factor 11)
- **Windows ACL** - Control de acceso al archivo hosts

### Logging
- **Serilog 3.1.1** - Logging estructurado
- **Serilog.Sinks.File** - Sink para archivos con rotación diaria

### UI/UX
- **CommunityToolkit.Mvvm 8.2.2** - MVVM toolkit
- **LiveChartsCore 2.0.0-rc2** - Gráficos y visualización (pendiente de uso)

### Hosting
- **Microsoft.Extensions.Hosting 8.0.1** - Dependency injection, configuration
- **Microsoft.Extensions.Hosting.WindowsServices** - Windows Service hosting

### Build & Deployment
- **MSBuild** - Sistema de build
- **PowerShell** - Scripts de instalación/desinstalación
- **sc.exe** - Windows Service Controller

---

## 📁 Estructura del Proyecto

```
AulaSegura/
├── src/
│   ├── AulaSegura.Core/                # Capa de dominio
│   │   ├── Entities/
│   │   │   ├── BaseEntity.cs
│   │   │   ├── Administrator.cs
│   │   │   ├── BlockedSite.cs
│   │   │   ├── AllowedSite.cs
│   │   │   ├── Category.cs
│   │   │   ├── Schedule.cs
│   │   │   ├── ActivityLog.cs
│   │   │   ├── Setting.cs
│   │   │   └── Backup.cs
│   │   ├── Interfaces/
│   │   │   ├── IRepository.cs
│   │   │   ├── IAuthService.cs
│   │   │   ├── IBlockedSiteService.cs
│   │   │   ├── IAllowedSiteService.cs
│   │   │   ├── ICategoryService.cs
│   │   │   ├── IScheduleService.cs
│   │   │   ├── IActivityLogService.cs
│   │   │   ├── ISettingsService.cs
│   │   │   └── IBackupService.cs
│   │   ├── Constants/
│   │   │   └── SystemConstants.cs
│   │   ├── Utilities/
│   │   │   └── ValidationHelper.cs
│   │   └── AulaSegura.Core.csproj
│   │
│   ├── AulaSegura.Infrastructure/      # Capa de infraestructura
│   │   ├── Data/
│   │   │   └── AulaSeguraDbContext.cs
│   │   ├── Repositories/
│   │   │   └── Repository.cs
│   │   ├── Services/
│   │   │   ├── AuthService.cs
│   │   │   ├── BlockedSiteService.cs
│   │   │   ├── AllowedSiteService.cs
│   │   │   ├── CategoryService.cs
│   │   │   ├── ScheduleService.cs
│   │   │   ├── ActivityLogService.cs
│   │   │   ├── SettingsService.cs
│   │   │   └── BackupService.cs
│   │   ├── Seeding/
│   │   │   └── DatabaseSeeder.cs
│   │   ├── InfrastructureConfiguration.cs
│   │   └── AulaSegura.Infrastructure.csproj
│   │
│   ├── AulaSegura.Service/             # Windows Service
│   │   ├── Worker.cs                   # BlockingWorker
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   └── AulaSegura.Service.csproj
│   │
│   ├── AulaSegura.App/                 # Aplicación WPF
│   │   ├── ViewModels/
│   │   │   └── LoginViewModel.cs
│   │   ├── Views/
│   │   │   ├── LoginView.xaml
│   │   │   └── LoginView.xaml.cs
│   │   ├── Converters/
│   │   │   └── ValueConverters.cs
│   │   ├── MainWindow.xaml
│   │   ├── MainWindow.xaml.cs
│   │   ├── App.xaml
│   │   ├── App.xaml.cs
│   │   ├── appsettings.json
│   │   └── AulaSegura.App.csproj
│   │
│   └── AulaSegura.Shared/              # Utilidades compartidas
│       └── AulaSegura.Shared.csproj
│
├── scripts/
│   ├── install-service.ps1
│   └── uninstall-service.ps1
│
├── docs/
│   ├── INSTALACION.md
│   ├── MANUAL_USUARIO.md
│   ├── MANUAL_TECNICO.md
│   ├── SEGURIDAD.md
│   ├── PROGRESS_REPORT_PHASE2.md
│   ├── PROGRESS_REPORT_PHASE3.md
│   └── VERIFICACION_FINAL.md
│
├── AulaSegura.sln
├── README.md
├── QUICK_START.md
└── PHASE3_COMPLETION_SUMMARY.md
```

---

## 🗄️ Base de Datos

### Schema de SQLite

#### Tabla: Administrators
```sql
CREATE TABLE Administrators (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Username TEXT NOT NULL UNIQUE,
    PasswordHash TEXT NOT NULL,
    Email TEXT NOT NULL,
    FullName TEXT NOT NULL,
    FailedLoginAttempts INTEGER DEFAULT 0,
    LastLoginAt DATETIME,
    LockedUntil DATETIME,
    IsActive BOOLEAN DEFAULT 1,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
);
```

#### Tabla: Categories
```sql
CREATE TABLE Categories (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL UNIQUE,
    Description TEXT,
    Color TEXT DEFAULT '#666666',
    IsActive BOOLEAN DEFAULT 1,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
);
```

#### Tabla: BlockedSites
```sql
CREATE TABLE BlockedSites (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Domain TEXT NOT NULL UNIQUE,
    CategoryId INTEGER,
    Reason TEXT,
    BlockSubdomains BOOLEAN DEFAULT 1,
    IsActive BOOLEAN DEFAULT 1,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
);
```

#### Tabla: AllowedSites
```sql
CREATE TABLE AllowedSites (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Domain TEXT NOT NULL UNIQUE,
    CategoryId INTEGER,
    Reason TEXT,
    IsActive BOOLEAN DEFAULT 1,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
);
```

#### Tabla: Schedules
```sql
CREATE TABLE Schedules (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    DaysOfWeek TEXT NOT NULL,  -- JSON array: [1,2,3,4,5]
    StartTime TEXT NOT NULL,    -- "08:00"
    EndTime TEXT NOT NULL,      -- "15:00"
    CategoryIds TEXT,           -- JSON array de IDs
    IsActive BOOLEAN DEFAULT 1,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
);
```

#### Tabla: ActivityLogs
```sql
CREATE TABLE ActivityLogs (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP,
    EventType TEXT NOT NULL,    -- Login, ConfigChange, HostsUpdate, etc.
    Description TEXT NOT NULL,
    UserId INTEGER,
    IpAddress TEXT,
    Details TEXT,               -- JSON con detalles adicionales
    FOREIGN KEY (UserId) REFERENCES Administrators(Id)
);
```

#### Tabla: Settings
```sql
CREATE TABLE Settings (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Key TEXT NOT NULL UNIQUE,
    Value TEXT NOT NULL,
    Description TEXT,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
);
```

#### Tabla: Backups
```sql
CREATE TABLE Backups (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    BackupPath TEXT NOT NULL,
    BackupType TEXT NOT NULL,   -- Hosts, Database
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    CreatedBy INTEGER,
    FileSize INTEGER,
    FOREIGN KEY (CreatedBy) REFERENCES Administrators(Id)
);
```

### Seed Data

Al inicializar la base de datos por primera vez, se crean:

#### Administrador por Defecto
```csharp
Username: admin
Password: SeedAdmin__Password o first-run-admin.txt (hashed con BCrypt, work factor 11)
Email: admin@aulasegura.local
FullName: Administrador Principal
```

#### Categorías Predefinidas
1. Adultos (#F44336 - rojo)
2. Redes Sociales (#2196F3 - azul)
3. Juegos (#4CAF50 - verde)
4. Apuestas (#FF9800 - naranja)
5. Entretenimiento (#9C27B0 - púrpura)
6. Streaming (#00BCD4 - cyan)
7. Chat (#795548 - marrón)
8. Personalizadas (#607D8B - gris azulado)

#### Configuraciones del Sistema
- InstitutionName: "Mi Institución"
- Mode: "School" (School/Home)
- ProtectionLevel: "High" (Low/Medium/High/Maximum)
- AutoBackupEnabled: "true"
- CheckIntervalSeconds: "60"
- MaxBackupRetention: "30"
- EnableAuditLog: "true"
- DefaultLanguage: "es-ES"

---

## 🔌 Servicios e Interfaces

### IRepository<T>
Interfaz genérica para acceso a datos.

```csharp
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task<int> CountAsync();
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
}
```

**Implementación:** `Repository<T>` en Infrastructure layer usa EF Core.

### IAuthService
Servicio de autenticación y gestión de administradores.

```csharp
public interface IAuthService
{
    Task<Administrator?> LoginAsync(string username, string password);
    Task<Administrator?> RegisterAsync(string username, string password, string email, string fullName);
    Task<bool> ChangePasswordAsync(int adminId, string currentPassword, string newPassword);
    Task<bool> IsAccountLockedAsync(int adminId);
    Task ResetFailedAttemptsAsync(int adminId);
}
```

**Características:**
- BCrypt hashing con work factor 11
- Account lockout después de 5 intentos fallidos
- Lockout duration: 30 minutos
- Logging de intentos de login

### IBlockedSiteService
Gestión de sitios bloqueados y aplicación de reglas.

```csharp
public interface IBlockedSiteService
{
    Task<BlockedSite?> GetByIdAsync(int id);
    Task<IEnumerable<BlockedSite>> GetAllAsync();
    Task AddAsync(BlockedSite site);
    Task UpdateAsync(BlockedSite site);
    Task DeleteAsync(int id);
    Task ApplyBlockingRulesAsync();  // Aplica reglas al archivo hosts
    Task<bool> IsBlockedAsync(string domain);
}
```

**Método clave:** `ApplyBlockingRulesAsync()`
1. Crea backup del archivo hosts
2. Lee sitios bloqueados activos
3. Lee sitios permitidos (whitelist)
4. Genera nuevo contenido del hosts
5. Escribe archivo hosts
6. Registra actividad

### IAllowedSiteService
Gestión de lista blanca (whitelist).

```csharp
public interface IAllowedSiteService
{
    Task<AllowedSite?> GetByIdAsync(int id);
    Task<IEnumerable<AllowedSite>> GetAllAsync();
    Task AddAsync(AllowedSite site);
    Task UpdateAsync(AllowedSite site);
    Task DeleteAsync(int id);
}
```

**Prioridad:** Los sitios en whitelist tienen precedencia sobre blocked sites.

### ICategoryService
Gestión de categorías de contenido.

```csharp
public interface ICategoryService
{
    Task<Category?> GetByIdAsync(int id);
    Task<IEnumerable<Category>> GetAllAsync();
    Task<IEnumerable<Category>> GetActiveAsync();
    Task AddAsync(Category category);
    Task UpdateAsync(Category category);
    Task DeleteAsync(int id);
    Task ToggleBlockAsync(int categoryId, bool block);
}
```

### IScheduleService
Gestión de horarios de bloqueo.

```csharp
public interface IScheduleService
{
    Task<Schedule?> GetByIdAsync(int id);
    Task<IEnumerable<Schedule>> GetAllAsync();
    Task<IEnumerable<Schedule>> GetActiveSchedulesForDayAsync(DayOfWeek day);
    Task AddAsync(Schedule schedule);
    Task UpdateAsync(Schedule schedule);
    Task DeleteAsync(int id);
    Task<bool> IsCurrentlyActiveAsync(int scheduleId);
}
```

### IActivityLogService
Registro de actividad del sistema.

```csharp
public interface IActivityLogService
{
    Task LogAsync(string eventType, string description, int? userId = null, string details = null);
    Task<IEnumerable<ActivityLog>> GetRecentLogsAsync(int count = 100);
    Task<IEnumerable<ActivityLog>> GetLogsByDateRangeAsync(DateTime start, DateTime end);
    Task<IEnumerable<ActivityLog>> GetLogsByEventTypeAsync(string eventType);
    Task ExportLogsAsync(string filePath);
}
```

### ISettingsService
Gestión de configuraciones del sistema.

```csharp
public interface ISettingsService
{
    Task<string?> GetSettingAsync(string key);
    Task SetSettingAsync(string key, string value);
    Task<IEnumerable<Setting>> GetAllSettingsAsync();
    Task ResetToDefaultsAsync();
}
```

### IBackupService
Gestión de copias de seguridad.

```csharp
public interface IBackupService
{
    Task BackupHostsFileAsync();
    Task RestoreHostsFileAsync(string backupPath);
    Task<IEnumerable<Backup>> GetBackupsAsync();
    Task DeleteBackupAsync(int backupId);
    Task CleanupOldBackupsAsync(int retentionDays = 30);
}
```

---

## ⚙️ Windows Service

### BlockingWorker

El servicio Windows es un `BackgroundService` que ejecuta un ciclo de monitoreo continuo.

#### Ciclo de Vida

```csharp
public class BlockingWorker : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // 1. Inicializar base de datos
        await InitializeDatabaseAsync();

        // 2. Aplicar reglas iniciales
        await blockedSiteService.ApplyBlockingRulesAsync();

        // 3. Ciclo de monitoreo
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Verificar y aplicar cambios cada 60 segundos
                await blockedSiteService.ApplyBlockingRulesAsync();
                await Task.Delay(checkInterval, stoppingToken);
            }
            catch (Exception ex)
            {
                // Manejo de errores con retry
                _logger.LogError(ex, "Error en ciclo de monitoreo");
                await Task.Delay(retryDelay, stoppingToken);
            }
        }
    }
}
```

#### Características

- **Check Interval:** Configurable (default: 60 segundos)
- **Retry Logic:** 
  - UnauthorizedAccessException: retry en 1 minuto
  - General errors: retry en 30 segundos
- **Logging:** Serilog con rolling files diarios
- **Graceful Shutdown:** Respeta CancellationToken

#### Configuración del Servicio

```csharp
// Program.cs
builder.Services.Configure<WindowsServiceLifetimeOptions>(options =>
{
    options.ServiceName = "AulaSeguraService";
});
```

**Service Name:** `AulaSeguraService`  
**Display Name:** `AulaSegura Control Web Service`  
**Start Type:** Automatic  
**Log On As:** LocalSystem

---

## 🖥️ Aplicación WPF

### MVVM Pattern

La aplicación WPF sigue el patrón MVVM (Model-View-ViewModel):

#### Model
- Entidades de Core layer (Administrator, BlockedSite, etc.)

#### View
- Archivos XAML (LoginView.xaml, MainWindow.xaml)
- Definición de UI declarativa
- Data binding a ViewModels

#### ViewModel
- Clases C# (LoginViewModel)
- Lógica de presentación
- Commands para acciones del usuario
- Observable properties para UI reactiva

### Dependency Injection

Configurado en `App.xaml.cs`:

```csharp
public partial class App : Application
{
    private readonly IHost _host;

    public App()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Infrastructure
                services.AddInfrastructure(context.Configuration);
                
                // ViewModels
                services.AddTransient<LoginViewModel>();
                
                // Views
                services.AddTransient<LoginView>();
            })
            .Build();
    }
}
```

### Navigation

Actualmente simple (Login → MainWindow). Futuras versiones implementarán:
- NavigationService
- Region-based navigation
- History/back navigation

---

## 🔒 Seguridad

### BCrypt Password Hashing

**Implementación:**
```csharp
// Hash password
string hash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 11);

// Verify password
bool valid = BCrypt.Net.BCrypt.Verify(password, hash);
```

**Work Factor 11:**
- Balance entre seguridad y rendimiento
- ~250ms para hash en hardware moderno
- Resistente a ataques de fuerza bruta

### Account Lockout

**Política:**
- 5 intentos fallidos → cuenta bloqueada
- Duración de bloqueo: 30 minutos
- Reset automático después del timeout
- Admin puede resetear manualmente

### Hosts File Protection

**Medidas de seguridad:**
1. **Backup antes de modificar:** Siempre se crea backup
2. **Atomic writes:** Escritura completa, no parcial
3. **Validation:** Validación de formato de dominio
4. **Permissions:** Servicio corre como LocalSystem
5. **Rollback:** Capacidad de restaurar desde backup

### Audit Logging

**Qué se registra:**
- Todos los inicios de sesión (exitosos y fallidos)
- Cambios en configuración (CRUD operations)
- Modificaciones al archivo hosts
- Backups creados/restaurados
- Errores del sistema

**Formato:**
```json
{
  "Timestamp": "2026-04-24T14:30:15Z",
  "EventType": "Login",
  "Description": "Inicio de sesión exitoso",
  "UserId": 1,
  "IpAddress": "127.0.0.1",
  "Details": "{\"username\":\"admin\"}"
}
```

---

## 🔌 Extensibilidad

### Agregar Nuevo Servicio

1. **Definir interfaz en Core:**
```csharp
// AulaSegura.Core/Interfaces/INewService.cs
public interface INewService
{
    Task DoSomethingAsync();
}
```

2. **Implementar en Infrastructure:**
```csharp
// AulaSegura.Infrastructure/Services/NewService.cs
public class NewService : INewService
{
    private readonly AulaSeguraDbContext _context;
    
    public NewService(AulaSeguraDbContext context)
    {
        _context = context;
    }
    
    public async Task DoSomethingAsync()
    {
        // Implementación
    }
}
```

3. **Registrar en DI:**
```csharp
// AulaSegura.Infrastructure/InfrastructureConfiguration.cs
services.AddScoped<INewService, NewService>();
```

4. **Usar en ViewModel o Worker:**
```csharp
public class MyViewModel : ObservableObject
{
    private readonly INewService _newService;
    
    public MyViewModel(INewService newService)
    {
        _newService = newService;
    }
}
```

### Agregar Nueva Entidad

1. **Crear clase en Core/Entities:**
```csharp
public class NewEntity : BaseEntity
{
    public string Name { get; set; }
    public string Description { get; set; }
}
```

2. **Agregar DbSet en DbContext:**
```csharp
public DbSet<NewEntity> NewEntities { get; set; }
```

3. **Configurar en Fluent API:**
```csharp
modelBuilder.Entity<NewEntity>(entity =>
{
    entity.HasKey(e => e.Id);
    entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
});
```

4. **Crear interfaz de repositorio:**
```csharp
public interface INewEntityRepository : IRepository<NewEntity>
{
    // Métodos específicos si son necesarios
}
```

### Agregar Nueva Pantalla WPF

1. **Crear ViewModel:**
```csharp
// ViewModels/NewFeatureViewModel.cs
public partial class NewFeatureViewModel : ObservableObject
{
    [ObservableProperty]
    private string _data;
    
    public ICommand SaveCommand { get; }
    
    public NewFeatureViewModel(IService service)
    {
        SaveCommand = new AsyncRelayCommand(SaveAsync);
    }
    
    private async Task SaveAsync()
    {
        // Lógica de guardado
    }
}
```

2. **Crear View (XAML):**
```xml
<!-- Views/NewFeatureView.xaml -->
<Window x:Class="...NewFeatureView" ...>
    <Grid>
        <TextBox Text="{Binding Data}" />
        <Button Command="{Binding SaveCommand}" Content="Guardar" />
    </Grid>
</Window>
```

3. **Registrar en DI:**
```csharp
services.AddTransient<NewFeatureViewModel>();
services.AddTransient<NewFeatureView>();
```

---

## 🛠️ Desarrollo y Compilación

### Requisitos de Desarrollo

- **Visual Studio 2022** (Community, Professional, o Enterprise)
- **.NET 8 SDK** (incluido con VS 2022 17.8+)
- **Git** para control de versiones

### Compilar desde Línea de Comandos

```powershell
# Navegar al directorio del solution
cd "d:\APKICATIVO DE CONTROL DE PAGINAS\AulaSegura"

# Restaurar paquetes NuGet
dotnet restore

# Compilar solución completa
dotnet build AulaSegura.sln

# Compilación limpia (sin caché)
dotnet build AulaSegura.sln --no-incremental

# Compilar en modo Release
dotnet build AulaSegura.sln -c Release
```

### Ejecutar Proyectos Individuales

```powershell
# Ejecutar Windows Service
dotnet run --project src/AulaSegura.Service

# Ejecutar aplicación WPF
dotnet run --project src/AulaSegura.App
```

### Publicar para Producción

```powershell
# Publicar Windows Service
dotnet publish src/AulaSegura.Service/AulaSegura.Service.csproj \
    -c Release \
    -o publish/service \
    -r win-x64 \
    --self-contained false

# Publicar aplicación WPF
dotnet publish src/AulaSegura.App/AulaSegura.App.csproj \
    -c Release \
    -o publish/app \
    -r win-x64 \
    --self-contained false
```

### Testing

*(Pendiente de implementación)*

Futuras versiones incluirán:
- Unit tests con xUnit
- Integration tests
- UI tests con Playwright

---

## 📊 Monitoreo y Diagnóstico

### Logs del Servicio

**Ubicación:** `C:\Program Files\AulaSegura\Logs\`

**Formato:**
```
2026-04-24 14:30:15.123 +00:00 [INF] === AulaSegura Control Web Service Iniciado ===
2026-04-24 14:30:15.456 +00:00 [INF] Database initialized successfully
2026-04-24 14:30:16.789 +00:00 [INF] Blocking rules applied. Total blocked: 15 sites
2026-04-24 14:31:16.012 +00:00 [DBG] Monitoring cycle completed
```

**Niveles de log:**
- **DBG:** Debug information (ciclos de monitoreo)
- **INF:** Information (inicio, cambios aplicados)
- **WRN:** Warnings (problemas menores)
- **ERR:** Errors (fallos críticos)
- **FTL:** Fatal (servicio no puede continuar)

### Event Viewer de Windows

El servicio también escribe en Windows Event Log:

```powershell
# Ver eventos del servicio
Get-EventLog -LogName Application -Source "AulaSeguraService" -Newest 20

# Ver solo errores
Get-EventLog -LogName Application -Source "AulaSeguraService" -EntryType Error -Newest 10
```

### Health Checks

*(Pendiente de implementación)*

Futuras versiones incluirán endpoints de health check para monitoreo remoto.

---

## 🚀 Deployment

### Prerrequisitos de Producción

1. **Windows 10/11** (64-bit)
2. **.NET 8 Runtime** (no SDK)
3. **Permisos de Administrador** (para instalar servicio)
4. **Espacio en disco:** 500 MB mínimo

### Pasos de Deployment

1. **Copiar archivos publicados** a `C:\Program Files\AulaSegura\`
2. **Ejecutar script de instalación:**
   ```powershell
   cd C:\Program Files\AulaSegura\scripts
   .\install-service.ps1
   ```
3. **Verificar instalación:**
   ```powershell
   Get-Service "AulaSeguraService"
   ```
4. **Iniciar aplicación WPF** y cambiar contraseña por defecto

### Rollback

Si hay problemas:

```powershell
# Detener servicio
Stop-Service "AulaSeguraService"

# Restaurar backup anterior
.\uninstall-service.ps1 -RemoveData:$false

# Reinstalar versión anterior
.\install-service.ps1
```

---

## 📚 Recursos Adicionales

### Documentación Oficial
- [.NET Documentation](https://docs.microsoft.com/dotnet/)
- [Entity Framework Core](https://docs.microsoft.com/ef/core/)
- [WPF Documentation](https://docs.microsoft.com/dotnet/desktop/wpf/)
- [Serilog Documentation](https://serilog.net/)

### Código Fuente
- Repositorio GitHub (interno)
- Branch principal: `main`
- Branches de feature: `feature/*`
- Releases: `v1.0.0`, `v1.1.0`, etc.

### Soporte
- Issues en GitHub
- Email: soporte@aulasegura.local
- Documentación: Carpeta `docs/`

---

**Documento creado:** 24 de Abril, 2026  
**Versión:** 1.0.0  
**Mantenimiento:** Equipo de Desarrollo AulaSegura  
**Última actualización:** Según cambios en arquitectura
