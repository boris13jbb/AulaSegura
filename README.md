# AulaSegura Control Web

Aplicativo profesional de escritorio para Windows orientado al **bloqueo, control y administración del acceso a internet** en escuelas y hogares.

[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-Proprietary-red.svg)](LICENSE)
[![Platform](https://img.shields.io/badge/platform-Windows%2010%2F11-lightgrey.svg)](https://www.microsoft.com/windows)

---

## 📋 Descripción

**AulaSegura Control Web** es una solución completa que permite a padres de familia, tutores e instituciones educativas:

- ✅ Bloquear sitios web inapropiados (pornografía, apuestas, violencia)
- ✅ Controlar acceso a redes sociales y videojuegos
- ✅ Configurar horarios de bloqueo personalizados
- ✅ Administrar listas negras y blancas
- ✅ Generar reportes de actividad
- ✅ Proteger configuraciones con autenticación segura

---

## 🎯 Características Principales

### 🔒 Seguridad
- Autenticación con hash BCrypt (no texto plano)
- Bloqueo de cuenta tras intentos fallidos
- Contraseñas robustas validadas
- Auditoría completa de todas las acciones

### 🌐 Bloqueo Web
- Gestión de dominios bloqueados
- Listas blancas con prioridad configurable
- Categorías predefinidas (Adultos, Redes Sociales, etc.)
- Horarios de bloqueo por día/hora
- Aplicación automática vía archivo hosts

### 📊 Reportes y Logs
- Registro de todos los eventos
- Exportación a CSV/JSON
- Estadísticas de uso
- Historial de cambios

### 💾 Respaldo y Restauración
- Backup automático antes de cambios críticos
- Restauración de configuración
- Copia de seguridad del archivo hosts

---

## 🏗️ Arquitectura

El proyecto sigue **Clean Architecture** con separación clara de capas:

```
AulaSegura/
├── src/
│   ├── AulaSegura.Core/           # Entidades, interfaces, reglas de negocio
│   ├── AulaSegura.Infrastructure/ # SQLite, EF Core, repositorios, servicios
│   ├── AulaSegura.Service/        # Windows Service (background worker)
│   ├── AulaSegura.App/            # Aplicación WPF (interfaz gráfica)
│   └── AulaSegura.Shared/         # DTOs, utilidades compartidas
├── database/                       # Scripts SQL, migraciones
├── scripts/                        # PowerShell: install/uninstall service
└── docs/                           # Documentación técnica y manuales
```

---

## 🛠️ Tecnologías Utilizadas

| Tecnología | Versión | Propósito |
|------------|---------|-----------|
| .NET | 8.0 | Framework principal |
| Entity Framework Core | 8.0.11 | ORM para SQLite |
| SQLite | 3.x | Base de datos local |
| BCrypt.Net-Next | 4.0.3 | Hashing seguro de contraseñas |
| Serilog | 3.1.1 | Logging estructurado |
| WPF | .NET 8 | Interfaz gráfica |
| CommunityToolkit.Mvvm | 8.2.2 | Patrón MVVM |
| Windows Services | 8.0.1 | Servicio en segundo plano |

---

## 📦 Instalación

### Requisitos Previos
- Windows 10/11 (64-bit)
- .NET 8.0 Runtime o SDK
- Permisos de administrador (para instalación del servicio)

### Pasos de Instalación

1. **Clonar el repositorio**
   ```bash
   git clone <repository-url>
   cd AulaSegura
   ```

2. **Restaurar dependencias**
   ```bash
   dotnet restore AulaSegura.sln
   ```

3. **Compilar el proyecto**
   ```bash
   dotnet build AulaSegura.sln --configuration Release
   ```

4. **Aplicar migraciones de base de datos**
   ```bash
   cd src/AulaSegura.Infrastructure
   dotnet ef database update --startup-project ..\AulaSegura.Service\AulaSegura.Service.csproj
   ```

5. **Instalar el servicio Windows** (requiere administrador)
   ```powershell
   .\scripts\install-service.ps1
   ```

6. **Ejecutar la aplicación**
   ```bash
   cd src/AulaSegura.App
   dotnet run
   ```

---

## 🚀 Uso Rápido

### Primer Inicio

1. Ejecutar `AulaSegura.App.exe`
2. Iniciar sesión con credenciales por defecto:
   - **Usuario:** `admin`
   - **Contraseña:** `Admin@123`
3. Cambiar la contraseña inmediatamente desde Configuración

### Bloquear un Sitio Web

1. Ir a **Sitios Bloqueados** → **Agregar**
2. Ingresar dominio (ej: `facebook.com`)
3. Seleccionar categoría
4. Agregar motivo (opcional)
5. Click en **Guardar**
6. El sitio se bloqueará automáticamente en 60 segundos

### Configurar Horarios

1. Ir a **Horarios** → **Crear Horario**
2. Seleccionar categoría (ej: Redes Sociales)
3. Elegir días de la semana
4. Definir hora de inicio y fin
5. Activar horario

---

## 📖 Documentación

- [📘 Expediente Técnico Completo](Expediente_Tecnico_AulaSegura_Control_Web-1.md)
- [📗 Estado del Desarrollo](docs/ESTADO_DESARROLLO.md)
- [📙 Resumen Técnico de Implementación](docs/RESUMEN_TECNICO_IMPLEMENTACION.md)
- [📕 Manual de Usuario](docs/MANUAL_USUARIO.md) *(pendiente)*
- [📒 Manual Técnico](docs/MANUAL_TECNICO.md) *(pendiente)*
- [📔 Guía de Instalación](docs/INSTALACION.md) *(pendiente)*

---

## ⚠️ Limitaciones Conocidas

### 1. Bloqueo HTTPS
El archivo hosts solo bloquea resolución DNS. Sitios HTTPS pueden requerir:
- Proxy local con inspección SSL (fase futura)
- DNS filtrado (Cloudflare Family, CleanBrowsing)
- Políticas de grupo para deshabilitar DoH

### 2. Navegadores Portables
Navegadores que usan DNS propio (Chrome DoH) pueden evadir el bloqueo.

**Mitigación:** Configurar firewall rules y políticas de grupo.

### 3. Permisos Administrativos
La modificación del archivo hosts requiere elevación de privilegios.

**Solución:** El servicio Windows se ejecuta como SYSTEM.

---

## 🔐 Seguridad

### Mejores Prácticas Implementadas
- ✅ Contraseñas hasheadas con BCrypt (work factor 11)
- ✅ No almacenamiento de contraseñas en texto plano
- ✅ Bloqueo temporal de cuentas (30 min tras 5 intentos)
- ✅ Validación de entrada en todos los campos
- ✅ Principio de mínimo privilegio
- ✅ Auditoría completa de acciones
- ✅ Backup antes de cambios críticos

### Recomendaciones Adicionales
- Cambiar contraseña por defecto inmediatamente
- Usar contraseñas robustas (12+ caracteres)
- Actualizar Windows regularmente
- Complementar con firewall perimetral en escuelas

---

## 🧪 Desarrollo

### Estructura de Carpetas
```
src/
├── AulaSegura.Core/
│   ├── Entities/          # Entidades de dominio
│   ├── Interfaces/        # Contratos de servicios
│   ├── Constants/         # Constantes del sistema
│   └── Utilities/         # Helpers y validaciones
│
├── AulaSegura.Infrastructure/
│   ├── Data/              # DbContext y configuraciones
│   ├── Repositories/      # Implementación de repositorios
│   └── Services/          # Servicios de negocio
│
├── AulaSegura.Service/
│   └── Workers/           # Background workers
│
└── AulaSegura.App/
    ├── ViewModels/        # MVVM ViewModels
    ├── Views/             # Vistas XAML
    └── Converters/        # Value converters
```

### Compilación
```bash
# Debug
dotnet build AulaSegura.sln

# Release
dotnet build AulaSegura.sln --configuration Release

# Publicar
dotnet publish src/AulaSegura.App/AulaSegura.App.csproj -c Release -r win-x64 --self-contained
```

### Migraciones de Base de Datos
```bash
# Crear nueva migración
cd src/AulaSegura.Infrastructure
dotnet ef migrations add NombreMigracion --startup-project ..\AulaSegura.Service\AulaSegura.Service.csproj

# Aplicar migraciones
dotnet ef database update --startup-project ..\AulaSegura.Service\AulaSegura.Service.csproj

# Revertir última migración
dotnet ef database update PreviousMigration --startup-project ..\AulaSegura.Service\AulaSegura.Service.csproj
```

---

## 🤝 Contribuir

Este es un proyecto privado para uso institucional. Para reportar problemas o solicitar características, contactar al equipo de desarrollo.

---

## 📄 Licencia

Software propietario. Todos los derechos reservados.

---

## 👥 Equipo de Desarrollo

- **Arquitecto de Software:** [Nombre]
- **Desarrollador Backend:** [Nombre]
- **Desarrollador Frontend (WPF):** [Nombre]
- **Especialista en Ciberseguridad:** [Nombre]

---

## 📞 Soporte

Para soporte técnico o consultas:
- 📧 Email: soporte@aulasegura.local
- 📱 Teléfono: [Número de soporte]
- 🌐 Web: [URL de soporte]

---

## 🗺️ Roadmap

### Versión 1.0 (Actual)
- ✅ Arquitectura base
- ✅ Autenticación segura
- ✅ CRUD de sitios bloqueados
- ✅ Gestión de categorías
- ✅ Bloqueo vía archivo hosts
- ✅ Backup automático
- ⏳ Aplicación WPF (en desarrollo)
- ⏳ Windows Service (en desarrollo)

### Versión 2.0 (Futura)
- Proxy local con inspección HTTPS
- DNS filtrado integrado
- Panel web centralizado
- Sincronización en nube
- Active Directory integration
- Mobile app companion

---

**Última actualización:** 24 de abril de 2026  
**Versión:** 1.0.0-alpha  
**Estado:** En desarrollo activo
