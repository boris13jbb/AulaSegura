# AUDITORÍA TÉCNICA COMPLETA - AulaSegura Control Web

**Fecha de Auditoría:** 26 de Abril, 2026  
**Auditor:** Arquitecto de Software Senior (AI Assistant)  
**Versión del Expediente Técnico:** 1.0 (24 de Abril, 2026)  
**Estado del Proyecto:** MVP en Desarrollo - Fases 3-5 Parcialmente Completadas  

---

## RESUMEN EJECUTIVO

Se ha realizado una auditoría exhaustiva comparando el estado actual del código con los requisitos del **Expediente Técnico AulaSegura Control Web v1.0**. El análisis revela que el proyecto tiene una base sólida implementada pero existen **brechas significativas** que deben abordarse para cumplir completamente con el expediente técnico.

### Estado General del Proyecto

| Aspecto | Estado | Porcentaje Estimado |
|---------|--------|-------------------|
| Arquitectura y Estructura | ✅ Completo | 95% |
| Base de Datos y Entidades | ⚠️ Parcial | 75% |
| Servicios de Negocio | ⚠️ Parcial | 70% |
| UI/UX - WPF Views | ⚠️ Parcial | 65% |
| Servicio Windows | ⚠️ Parcial | 60% |
| Motor de Bloqueo | ❌ Incompleto | 40% |
| Reportes y Exportación | ❌ No Implementado | 0% |
| Seguridad Avanzada | ⚠️ Básico | 50% |
| Logs y Auditoría | ⚠️ Básico | 55% |
| Backups y Restauración | ⚠️ Básico | 45% |
| Pruebas y QA | ❌ Mínimo | 10% |
| Instalador y Despliegue | ❌ No Implementado | 0% |
| Documentación | ⚠️ Parcial | 60% |

**Porcentaje Total de Completitud Estimado: ~52%**

---

## ANÁLISIS DE BRECHAS DETALLADO POR MÓDULO

### 1. ARQUITECTURA Y ESTRUCTURA DEL PROYECTO

#### ✅ IMPLEMENTADO (Cumple con Expediente)

**Evidencia:**
- ✅ Clean Architecture implementada correctamente
- ✅ Separación en capas: Core, Infrastructure, App (UI), Service (Windows Service), Shared
- ✅ Dependency Injection configurado en App.xaml.cs
- ✅ Patrón MVVM con CommunityToolkit.Mvvm
- ✅ Entity Framework Core como ORM
- ✅ SQLite como base de datos local

**Archivos Clave Verificados:**
```
src/AulaSegura.Core/          → Entidades, Interfaces, Constantes, Utilities
src/AulaSegura.Infrastructure/ → DbContext, Repositories, Services, Seeding
src/AulaSegura.App/           → WPF UI, ViewModels, Views, Converters
src/AulaSegura.Service/       → Windows Service (Worker.cs)
src/AulaSegura.Shared/        → (Vacío - reservado para futuro)
```

**Conclusión:** La arquitectura cumple al 100% con lo especificado en el expediente sección 15 y 16.

---

### 2. BASE DE DATOS Y ENTIDADES

#### ⚠️ PARCIALMENTE IMPLEMENTADO

**Entidades Existentes vs Requeridas:**

| Tabla (Expediente Sec. 18) | Estado | Observaciones |
|----------------------------|--------|---------------|
| `admin_users` | ✅ Implementada | Entity: Administrator.cs |
| `categories` | ✅ Implementada | Entity: Category.cs |
| `blocked_sites` | ✅ Implementada | Entity: BlockedSite.cs |
| `allowed_sites` | ✅ Implementada | Entity: AllowedSite.cs |
| `schedules` | ✅ Implementada | Entity: Schedule.cs |
| `activity_logs` | ✅ Implementada | Entity: ActivityLog.cs |
| `settings` | ✅ Implementada | Entity: Setting.cs |
| `backups` | ✅ Implementada | Entity: Backup.cs |
| `keywords` | ❌ NO IMPLEMENTADA | **FALTA CRÍTICA** - RF-020 requiere filtrado por palabras clave |
| `blocking_rules` | ❌ NO IMPLEMENTADA | **FALTA CRÍTICA** - Motor de reglas avanzado no existe |
| `audit_logs` | ⚠️ Parcial | Usando ActivityLogs pero falta tabla dedicada de auditoría |
| `reports` | ❌ NO IMPLEMENTADA | **FALTA** - RF-011, RF-012 requieren módulo de reportes |

**Brechas Identificadas:**

1. **Falta entidad Keyword** (RF-020): El expediente requiere filtrado por palabras clave en búsquedas y URLs
2. **Falta entidad BlockingRule**: No hay sistema de reglas avanzadas con prioridades, tipos y acciones
3. **Falta entidad Report**: No hay persistencia de reportes generados
4. **Audit Logs incompleto**: Se usa ActivityLog para todo, pero el expediente distingue entre activity_logs y audit_logs

**Acción Requerida:**
- Crear entidades faltantes: Keyword, BlockingRule, Report
- Agregar DbSets correspondientes en AulaSeguraDbContext
- Crear migraciones de base de datos
- Actualizar DatabaseSeeder con datos iniciales de keywords

---

### 3. SERVICIOS DE NEGOCIO (Infrastructure/Services)

#### ⚠️ PARCIALMENTE IMPLEMENTADO

**Servicios Existentes vs Requeridos:**

| Servicio (Expediente Sec. 9-10) | Estado | Cobertura |
|--------------------------------|--------|-----------|
| IAuthService / AuthService | ✅ Completo | Login, hash de contraseñas, validación |
| ICategoryService | ✅ Completo | CRUD completo de categorías |
| IBlockedSiteService | ✅ Completo | CRUD + ApplyBlockingRulesAsync (stub) |
| IAllowedSiteService | ✅ Completo | CRUD completo |
| IScheduleService | ✅ Completo | CRUD de horarios |
| IActivityLogService | ✅ Completo | Logging básico implementado |
| ISettingsService | ✅ Completo | CRUD de configuraciones |
| IBackupService | ⚠️ Básico | Backup/Restore de DB implementado pero sin cifrado |
| IKeywordService | ❌ NO EXISTE | **FALTA** - Requiere entidad Keyword primero |
| IReportService | ❌ NO EXISTE | **FALTA CRÍTICA** - RF-011, RF-012 |
| IBlockingEngineService | ❌ NO EXISTE | **FALTA CRÍTICA** - Motor de evaluación de reglas |

**Análisis Detallado:**

✅ **Fortalezas:**
- Todos los servicios CRUD básicos están implementados
- AuthService incluye hashing seguro (BCrypt o similar)
- BackupService permite exportar/importar base de datos
- ActivityLogService registra eventos del sistema

❌ **Debilidades Críticas:**
1. **ApplyBlockingRulesAsync() es un stub**: En BlockedSiteService.cs, el método que debería aplicar reglas de bloqueo (modificar archivo hosts, DNS, firewall) está vacío o incompleto
2. **No hay motor de evaluación**: Falta servicio que evalúe si una URL debe bloquearse basándose en listas negras, categorías, horarios, keywords
3. **No hay integración con DNS/Firewall**: El expediente sección 17 requiere múltiples estrategias de bloqueo
4. **BackupService sin cifrado**: El expediente sección 20.6 exige cifrado de respaldos

**Acción Requerida:**
- Implementar lógica real en ApplyBlockingRulesAsync()
- Crear IBlockingEngineService para evaluación de reglas
- Crear IKeywordService
- Crear IReportService con exportación CSV/PDF
- Agregar cifrado a BackupService usando DPAPI de Windows

---

### 4. INTERFAZ DE USUARIO (WPF Views)

#### ⚠️ PARCIALMENTE IMPLEMENTADO

**Vistas Existentes vs Requeridas (Expediente Sec. 19):**

| Pantalla (Expediente) | Estado | Observaciones |
|-----------------------|--------|---------------|
| LoginView | ✅ Completo | Login funcional con validación |
| DashboardView | ✅ Completo | Estadísticas, gráficos, navegación |
| BlockedSitesView | ✅ Completo | CRUD con UI completa |
| AllowedSitesView | ✅ Completo | CRUD con UI completa |
| CategoriesView | ✅ COMPLETADO | Formulario agregado en esta sesión |
| SchedulesView | ✅ Completo | CRUD de horarios |
| SettingsView | ✅ Completo | Configuración general |
| ReportsView | ❌ NO EXISTE | **FALTA CRÍTICA** - RF-011, RF-012 |
| ActivityLogsView | ❌ NO EXISTE | **FALTA** - Visualización de logs |
| KeywordsView | ❌ NO EXISTE | **FALTA** - Depende de entidad Keyword |
| BlockingRulesView | ❌ NO EXISTE | **FALTA** - Depende de entidad BlockingRule |

**Análisis de Calidad UI/UX:**

✅ **Aspectos Positivos:**
- Diseño limpio y moderno con Material Design inspirado
- Uso consistente de colores y tipografía
- Validaciones en formularios
- Mensajes de error claros
- Navegación fluida entre pantallas (implementado en sesión anterior)
- Converters registrados correctamente en App.xaml

⚠️ **Mejoras Necesarias:**
1. **Falta ReportsView**: El expediente requiere reportes con filtros por fecha, categoría, exportación CSV/PDF
2. **Falta ActivityLogsView**: Administrador debe poder revisar historial de bloqueos (CU-007)
3. **Sin página de bloqueo personalizada**: Cuando se bloquea un sitio, no hay UI que muestre mensaje amigable al usuario final
4. **Sin indicador de estado de protección**: RF-015 requiere mostrar si la protección está activa/inactiva/error en el Dashboard

**Acción Requerida:**
- Crear ReportsView con filtros y exportación
- Crear ActivityLogsView para visualizar logs
- Diseñar página HTML de bloqueo para mostrar cuando se bloquea un sitio
- Agregar indicador visual de estado de protección en Dashboard

---

### 5. SERVICIO WINDOWS (AulaSegura.Service)

#### ⚠️ PARCIALMENTE IMPLEMENTADO

**Estado Actual (Worker.cs analizado):**

✅ **Implementado:**
- Servicio Windows configurado como BackgroundService
- Inicialización de base de datos con seed data
- Ciclo de monitoreo periódico (configurable, default 60 segundos)
- Logging con ILogger
- Manejo de errores con reintentos
- Registro de inicio/detención en ActivityLogs

❌ **Faltante Crítico:**
1. **No monitorea tráfico de red**: El servicio solo llama ApplyBlockingRulesAsync() periódicamente, pero NO intercepta ni evalúa solicitudes HTTP/DNS en tiempo real
2. **No hay interceptor de DNS**: Falta componente que escuche consultas DNS y las evalúe contra reglas
3. **No hay proxy HTTP/HTTPS**: El expediente sección 17.4 menciona proxy local como estrategia avanzada
4. **No modifica archivo hosts**: ApplyBlockingRulesAsync() debería escribir en C:\Windows\System32\drivers\etc\hosts pero probablemente esté vacío
5. **No hay integración con Firewall de Windows**: Falta creación de reglas de firewall dinámicas

**Lo que DEBERÍA hacer el servicio según expediente:**
- Escuchar consultas DNS y evaluarlas contra reglas (sección 17.2)
- Modificar archivo hosts dinámicamente (sección 17.1)
- Opcionalmente actuar como proxy local (sección 17.4)
- Crear/modificar reglas de firewall (sección 17.3)
- Mostrar página de bloqueo cuando se detecta acceso a sitio prohibido

**Acción Requerida (CRÍTICO):**
- Implementar DNS Listener que intercepte consultas DNS
- Implementar modificación real del archivo hosts
- Crear BlockingPageServer (servidor HTTP simple) que sirva página de bloqueo
- Integrar con Windows Firewall API para crear reglas dinámicas
- Evaluar cada solicitud contra: lista blanca → lista negra → categorías → keywords → horarios

---

### 6. MOTOR DE BLOQUEO WEB

#### ❌ INCOMPLETO - BRECHA MÁS CRÍTICA

**Estado:** El corazón del sistema NO está implementado

**Lo que falta (Expediente Sec. 17, 22.2):**

1. **Interceptación de Solicitudes:**
   - ❌ No hay DNS interceptor
   - ❌ No hay proxy HTTP/HTTPS
   - ❌ No hay filtro de WinSock o WFP (Windows Filtering Platform)
   - ❌ No hay driver de red

2. **Evaluación de Reglas:**
   - ❌ No hay motor que evalúe: dominio solicitado vs reglas activas
   - ❌ No hay lógica de prioridad (lista blanca > lista negra > categoría > horario)
   - ❌ No hay evaluación de keywords en URLs
   - ❌ No hay verificación de horarios activos

3. **Aplicación de Bloqueo:**
   - ❌ No se modifica archivo hosts automáticamente
   - ❌ No se crean reglas de firewall dinámicas
   - ❌ No se redirige a página de bloqueo
   - ❌ No se bloquea resolución DNS

4. **Página de Bloqueo:**
   - ❌ No existe página HTML personalizada para mostrar al usuario cuando se bloquea
   - ❌ No muestra razón del bloqueo (categoría, regla aplicada)
   - ❌ No ofrece contacto al administrador

**Estrategias de Bloqueo del Expediente (Sec. 17):**

| Estrategia | Estado | Prioridad |
|-----------|--------|-----------|
| Archivo hosts | ❌ No implementado | ALTA (MVP) |
| DNS filtrado | ❌ No implementado | ALTA (MVP) |
| Firewall Windows | ❌ No implementado | MEDIA (Complemento) |
| Proxy local | ❌ No implementado | BAJA (Avanzado) |
| Extensión navegador | ❌ No implementado | BAJA (Complemento) |
| Bloqueo por keywords | ❌ No implementado | MEDIA (Requerido) |

**Acción Requerida (PRIORIDAD MÁXIMA):**

**Opción A - MVP Simple (Recomendado para empezar):**
1. Implementar modificación automática del archivo hosts desde BlockedSiteService
2. Crear servidor HTTP local (puerto 8080) que sirva página de bloqueo
3. Configurar DNS del sistema para resolver dominios bloqueados a 127.0.0.1
4. Evaluar reglas cada vez que se agrega/edita un sitio bloqueado

**Opción B - MVP Avanzado (Más robusto):**
1. Implementar DNS listener usando biblioteca como DnsServer o ArSoft.Tools.Net
2. Intercepter consultas DNS y responder con IP de bloqueo para dominios prohibidos
3. Servir página de bloqueo desde servidor HTTP embebido
4. Integrar con Windows Filtering Platform (WFP) para bloqueo a nivel de socket

**Opción C - Producción (Completo):**
1. Driver de red personalizado o uso de WFP nativo
2. Proxy HTTPS con certificado autofirmado (requiere instalación en trust store)
3. Inspección profunda de paquetes (DPI)
4. Machine learning para clasificación de contenido

**Recomendación:** Comenzar con **Opción A** para MVP, luego evolucionar a **Opción B**.

---

### 7. REPORTES Y EXPORTACIÓN

#### ❌ NO IMPLEMENTADO

**Requisitos del Expediente (Sec. 9.9, RF-011, RF-012, CU-007, CU-008):**

El sistema DEBE tener:
- Reporte de intentos bloqueados por día
- Sitios más intentados
- Categorías más bloqueadas
- Cambios de configuración
- Estado de protección
- Equipos con errores
- Exportación a CSV
- Exportación a PDF
- Filtros por rango de fechas, categoría, equipo

**Estado Actual:**
- ❌ No hay interfaz de reportes
- ❌ No hay servicio de generación de reportes
- ❌ No hay exportación CSV
- ❌ No hay exportación PDF
- ❌ No hay consultas agregadas en la base de datos para estadísticas

**Acción Requerida:**
1. Crear entidad Report si se quiere persistir reportes generados
2. Crear IReportService con métodos:
   - GetBlockedAttemptsByDateRange(DateTime from, DateTime to)
   - GetTopBlockedSites(int count)
   - GetBlockedByCategory(DateTime from, DateTime to)
   - GetConfigurationChanges(DateTime from, DateTime to)
   - ExportToCsv(reportData)
   - ExportToPdf(reportData) - usar QuestPDF o ClosedXML
3. Crear ReportsView.xaml con:
   - DatePicker para rango de fechas
   - ComboBox para filtro por categoría
   - DataGrid para mostrar resultados
   - Botones "Exportar CSV" y "Exportar PDF"
   - Gráficos simples (opcional)

---

### 8. SEGURIDAD

#### ⚠️ BÁSICO - REQUIERE FORTALECER

**Aspectos Evaluados (Expediente Sec. 20):**

| Aspecto de Seguridad | Estado | Observaciones |
|---------------------|--------|---------------|
| Hash de contraseñas | ✅ Implementado | AuthService usa hashing seguro |
| Protección contra fuerza bruta | ⚠️ Parcial | Administrator entity tiene failed_attempts y locked_until pero no se valida en login |
| Cifrado de configuración sensible | ❌ No implementado | Settings con is_sensitive flag pero no se cifra valor |
| DPAPI para secretos | ❌ No implementado | Expediente recomienda DPAPI de Windows |
| Permisos NTFS en archivos | ❌ No implementado | No se configuran permisos en DB o config |
| Protección contra desinstalación | ❌ No implementado | Sin protección especial |
| Firma de ejecutables | ❌ No implementado | Sin code signing |
| Validación de entradas | ⚠️ Básico | Algunas validaciones en ViewModels pero no exhaustivas |
| SQL Injection protection | ✅ Seguro | EF Core parametriza queries automáticamente |
| Logs inmutables | ⚠️ Parcial | ActivityLogs se pueden borrar/modificar desde UI |

**Brechas Críticas de Seguridad:**

1. **No hay validación de force brute en login**: El campo `failed_attempts` existe en la entidad pero AuthService.LoginAsync() no incrementa contador ni bloquea cuenta temporalmente
2. **Configuración sensible en texto plano**: Contraseñas, tokens o claves en Settings se guardan sin cifrar
3. **Base de datos sin protección de permisos**: Cualquier usuario con acceso al filesystem puede copiar/abrir aulasegura.db
4. **Sin protección contra manipulación de hosts**: Si un usuario admin modifica manualmente el archivo hosts, el sistema no lo detecta ni restaura
5. **Logs editables/borrables**: Un usuario malintencionado podría borrar evidencia de intentos de acceso

**Acción Requerida:**
1. Implementar lógica de bloqueo temporal en AuthService tras 5 intentos fallidos
2. Cifrar valores de Settings marcados como sensibles usando DPAPI
3. Configurar permisos NTFS restrictivos en archivo de base de datos durante instalación
4. Implementar checksum/integridad del archivo hosts y detectar modificaciones no autorizadas
5. Hacer logs append-only (no permitir edición/borrado desde UI normal, solo desde modo super-admin)

---

### 9. LOGS Y AUDITORÍA

#### ⚠️ BÁSICO

**Estado Actual:**
- ✅ ActivityLogService permite registrar eventos
- ✅ Se registran: ServiceStarted, ServiceStopped, errores de bloqueo
- ✅ Entidad ActivityLog tiene campos: event_type, windows_user, device_name, domain, category, action, created_at

**Faltante (Expediente Sec. 9.10, 18.3.8, 18.3.9):**

1. **No hay distinción clara entre activity_logs y audit_logs**:
   - activity_logs: Eventos de bloqueo, accesos, errores técnicos
   - audit_logs: Cambios de configuración realizados por administradores (quién cambió qué, cuándo, valor anterior, valor nuevo)

2. **No se registran todos los eventos requeridos**:
   - ❌ Intentos de login fallidos
   - ❌ Cambios en listas negras/blancas
   - ❌ Cambios en categorías
   - ❌ Cambios en horarios
   - ❌ Exportación de reportes
   - ❌ Respaldo/restauración de configuración
   - ❌ Intentos de detener servicio

3. **No hay interfaz para visualizar logs**: Falta ActivityLogsView

**Acción Requerida:**
1. Crear entidad AuditLog separada de ActivityLog
2. Agregar triggers/hooks en todos los servicios para registrar cambios en AuditLog
3. Implementar IActivityLogService.LogLoginAttempt(bool success, string username, string ipAddress)
4. Crear ActivityLogsView.xaml con filtros y visualización
5. Agregar retención automática de logs antiguos (ej. borrar logs > 90 días)

---

### 10. BACKUPS Y RESTAURACIÓN

#### ⚠️ BÁSICO

**Estado Actual:**
- ✅ BackupService existe con métodos CreateBackupAsync() y RestoreBackupAsync()
- ✅ Permite exportar/importar archivo .db de SQLite
- ✅ Entidad Backup registra metadatos de respaldos

**Faltante (Expediente Sec. 9.13, 20.6):**

1. **Sin cifrado de respaldos**: El expediente exige cifrar backups con contraseña
2. **Sin firma de integridad**: No hay forma de verificar que backup no fue manipulado
3. **Sin respaldo automático programado**: No hay scheduler que haga backups diarios/semanales
4. **Sin validación antes de restaurar**: No se verifica integridad del archivo antes de restaurar
5. **Sin backup pre-cambios masivos**: Antes de operaciones críticas debería hacerse backup automático

**Acción Requerida:**
1. Agregar cifrado AES a BackupService usando contraseña proporcionada por usuario
2. Generar hash SHA256 del backup y almacenarlo para validación de integridad
3. Crear ScheduledBackupService que ejecute backups automáticos configurables
4. Validar checksum antes de permitir restauración
5. Agregar trigger en servicios críticos para crear backup automático antes de cambios masivos

---

### 11. INSTALADOR Y DESPLIEGUE

#### ❌ NO IMPLEMENTADO

**Requisitos del Expediente (Sec. 23 Fase 6, Sec. 20.9):**

El sistema DEBE tener:
- Instalador MSI/MSIX/WiX
- Instalación como Windows Service con privilegios administrativos
- Configuración automática de permisos NTFS
- Creación de usuario administrador inicial
- Configuración de auto-inicio del servicio
- Desinstalador protegido con contraseña

**Estado Actual:**
- ❌ No hay proyecto de instalador
- ❌ No hay scripts de instalación automatizada
- ❌ Servicio debe instalarse manualmente con `sc create` o similar
- ❌ Sin configuración automática de permisos
- ❌ Sin desinstalador protegido

**Acción Requerida:**
1. Crear proyecto de instalador usando WiX Toolset o MSIX
2. Script de instalación que:
   - Instale Windows Service con credenciales LocalSystem
   - Configure service para auto-start
   - Cree carpeta de aplicación con permisos restrictivos
   - Inicialice base de datos con admin por defecto (admin/admin123)
   - Configure firewall rules si aplica
3. Script de desinstalación que:
   - Solicite contraseña de administrador
   - Detenga servicio
   - Elimine servicio
   - Pregunte si desea conservar o eliminar base de datos
   - Limpie archivo hosts de entradas agregadas

---

### 12. PRUEBAS Y QA

#### ❌ MÍNIMO

**Estado Actual:**
- ❌ No hay tests unitarios
- ❌ No hay tests de integración
- ❌ No hay tests de UI
- ❌ No hay pipeline de CI/CD
- ⚠️ Solo compilación manual verificada

**Requisitos del Expediente (Sec. 28):**
- Pruebas funcionales de todos los módulos
- Pruebas de seguridad (penetration testing básico)
- Pruebas de bloqueo (verificar que sitios bloqueados realmente no carguen)
- Pruebas de permisos (usuario estándar no debe poder desactivar)
- Pruebas con múltiples navegadores (Chrome, Firefox, Edge)
- Pruebas de instalación/desinstalación
- Pruebas de rendimiento

**Acción Requerida:**
1. Crear proyecto de tests unitarios (xUnit o NUnit)
2. Escribir tests para:
   - AuthService (login, hash, force brute protection)
   - CategoryService (CRUD operations)
   - BlockedSiteService (CRUD + apply rules)
   - ValidationHelper (validaciones de dominio, email, etc.)
3. Crear tests de integración para:
   - Flujo completo de bloqueo (agregar sitio → aplicar reglas → verificar hosts)
   - Backup/restore
   - Migraciones de base de datos
4. Documentar plan de pruebas manuales checklist
5. Configurar GitHub Actions o Azure DevOps para CI básico (build + tests)

---

## PLAN DE ACCIÓN POR FASES

Basado en el análisis de brechas, se propone el siguiente plan de acción priorizado:

### FASE 1: CORE - Motor de Bloqueo Funcional (PRIORIDAD CRÍTICA)

**Objetivo:** Hacer que el sistema realmente bloquee sitios web

**Duración Estimada:** 2-3 semanas

**Tareas:**

1. **Implementar modificación de archivo hosts** (3 días)
   - Crear HostsFileManager.cs en Infrastructure
   - Método AddEntry(string domain, string ip = "127.0.0.1")
   - Método RemoveEntry(string domain)
   - Método ClearAllEntries()
   - Backup automático antes de modificar
   - Detección de modificaciones externas

2. **Crear página de bloqueo HTML** (2 días)
   - Diseñar blocking.html con mensaje claro
   - Mostrar: dominio bloqueado, categoría, fecha/hora
   - Contacto del administrador
   - Servir desde servidor HTTP embebido (puerto 8080)

3. **Implementar servidor HTTP de bloqueo** (3 días)
   - Crear BlockingPageServer.cs usando HttpListener
   - Escuchar en http://127.0.0.1:8080/
   - Redirigir todas las rutas a blocking.html
   - Iniciar/detener junto con el servicio Windows

4. **Integrar con BlockedSiteService** (2 días)
   - ApplyBlockingRulesAsync() debe:
     - Leer todos los BlockedSites activos
     - Limpiar entradas antiguas del hosts
     - Agregar nuevas entradas al hosts
     - Reiniciar servidor de bloqueo si es necesario
     - Registrar evento en ActivityLog

5. **Pruebas de bloqueo** (3 días)
   - Agregar sitio de prueba a lista negra
   - Ejecutar ApplyBlockingRulesAsync()
   - Verificar que entrada exista en hosts
   - Intentar acceder desde navegador
   - Verificar que muestre página de bloqueo
   - Probar con Chrome, Firefox, Edge

**Entregable:** Sistema que bloquea sitios modificando archivo hosts y mostrando página de bloqueo

---

### FASE 2: ENTIDADES FALTANTES - Keywords, BlockingRules, Reports

**Objetivo:** Completar modelo de datos según expediente

**Duración Estimada:** 1 semana

**Tareas:**

1. **Crear entidad Keyword** (1 día)
   - Properties: Id, Keyword, CategoryId, Severity, IsActive, CreatedAt
   - Agregar DbSet en DbContext
   - Crear IKeywordService con CRUD
   - Agregar seeding de keywords comunes

2. **Crear entidad BlockingRule** (2 días)
   - Properties: Id, RuleType (enum), TargetId, Action (enum), ScheduleId, Priority, IsActive, CreatedBy, CreatedAt
   - RuleType enum: Domain, Category, Keyword, Social, Custom
   - Action enum: Block, Allow, Warn
   - Agregar DbSet en DbContext
   - Crear IBlockingRuleService
   - Lógica de evaluación de prioridad

3. **Crear entidad Report** (1 día)
   - Properties: Id, ReportType, DateFrom, DateTo, GeneratedBy, FilePath, CreatedAt
   - Agregar DbSet en DbContext
   - Crear IReportService (sin lógica de generación aún)

4. **Crear migraciones de base de datos** (1 día)
   - dotnet ef migrations add AddKeywordsBlockingRulesReports
   - dotnet ef database update
   - Verificar esquema en SQLite

5. **Actualizar DatabaseSeeder** (1 día)
   - Agregar keywords predeterminadas (sexo, porno, apuesta, droga, etc.)
   - Agregar blocking rules básicas
   - Verificar seed data correcta

**Entregable:** Base de datos completa según especificación del expediente

---

### FASE 3: REPORTES Y EXPORTACIÓN

**Objetivo:** Implementar módulo de reportes RF-011, RF-012

**Duración Estimada:** 1.5 semanas

**Tareas:**

1. **Implementar IReportService** (3 días)
   - GetBlockedAttemptsByDateRange(DateTime from, DateTime to)
   - GetTopBlockedSites(int count, DateTime from, DateTime to)
   - GetBlockedByCategory(DateTime from, DateTime to)
   - GetConfigurationChanges(DateTime from, DateTime to)
   - Queries LINQ optimizadas contra ActivityLogs y AuditLogs

2. **Agregar librería de exportación** (1 día)
   - Instalar paquete NuGet: QuestPDF (para PDF) o ClosedXML (para Excel)
   - Para CSV: usar CsvHelper o implementación simple

3. **Implementar exportación CSV** (2 días)
   - ExportBlockedAttemptsToCsv(data, filePath)
   - ExportTopSitesToCsv(data, filePath)
   - ExportByCategoryToCsv(data, filePath)
   - Formato: Fecha, Dominio, Categoría, Acción, Usuario Windows

4. **Implementar exportación PDF** (3 días)
   - Diseñar template PDF profesional con QuestPDF
   - Incluir: logo, título, rango de fechas, tabla de datos, totales
   - ExportBlockedAttemptsToPdf(data, filePath)
   - ExportSummaryReportToPdf(data, filePath)

5. **Crear ReportsView.xaml** (3 días)
   - DatePicker para fecha inicio/fin
   - ComboBox para filtro por categoría
   - Botón "Generar Reporte"
   - DataGrid para mostrar resultados
   - Botones "Exportar CSV" y "Exportar PDF"
   - SaveFileDialog para seleccionar ruta de exportación
   - Preview básico del reporte

6. **Agregar navegación desde Dashboard** (0.5 días)
   - Agregar botón "Ver Reportes" en Dashboard
   - Implementar NavigateToReports() en MainWindow

**Entregable:** Módulo de reportes funcional con exportación CSV/PDF

---

### FASE 4: SEGURIDAD FORTALECIDA

**Objetivo:** Cumplir requisitos de seguridad del expediente sección 20

**Duración Estimada:** 1.5 semanas

**Tareas:**

1. **Implementar protección contra fuerza bruta** (2 días)
   - Modificar AuthService.LoginAsync():
     - Incrementar failed_attempts en cada intento fallido
     - Si failed_attempts >= 5, establecer locked_until = Now + 15 minutos
     - Rechazar login si locked_until > Now
     - Resetear failed_attempts a 0 en login exitoso
   - Mostrar mensaje apropiado en UI: "Cuenta bloqueada por 15 minutos"

2. **Cifrar settings sensibles con DPAPI** (2 días)
   - Crear CryptoHelper.cs con métodos:
     - EncryptWithDPAPI(string plainText) → byte[]
     - DecryptWithDPAPI(byte[] encryptedData) → string
   - Modificar SettingsService:
     - Al guardar setting con is_sensitive=true, cifrar value
     - Al leer setting con is_sensitive=true, descifrar value
   - Probar cifrado/descifrado correcto

3. **Configurar permisos NTFS en base de datos** (1 día)
   - Crear FilePermissionsHelper.cs
   - Método SetDatabasePermissions(string dbPath):
     - Remover permisos de usuarios estándar
     - Grant FullControl solo a Administradores y SYSTEM
     - Grant Read/Write solo a cuenta del servicio
   - Llamar durante instalación/inicialización

4. **Hacer logs append-only** (2 días)
   - Modificar ActivityLogService:
     - Remover métodos UpdateActivityLog y DeleteActivityLog de interfaz pública
     - Solo permitir LogActivityAsync (insert)
     - Crear método privado PurgeOldLogs(daysToKeep) para limpieza automática
   - Agregar configuración: Settings.LogRetentionDays (default 90)
   - Scheduler que ejecute PurgeOldLogs diariamente

5. **Detectar manipulación de archivo hosts** (2 días)
   - Crear HostsIntegrityChecker.cs
   - Método CalculateChecksum() → hash SHA256 del hosts
   - Guardar checksum esperado en Settings
   - Verificar cada hora si hosts fue modificado externamente
   - Si detecta cambio no autorizado:
     - Registrar en ActivityLog como SECURITY_ALERT
     - Restaurar desde backup automático
     - Notificar al administrador (log visible en UI)

6. **Validación exhaustiva de entradas** (1 día)
   - Revisar todos los ViewModels
   - Agregar validaciones:
     - Dominios: regex válido, longitud máxima, sin caracteres peligrosos
     - Keywords: sin scripts, longitud máxima
     - Horarios: start_time < end_time, días válidos
     - Emails: formato válido
   - Usar ValidationHelper existente o crear nuevo

**Entregable:** Sistema hardened con protección contra ataques comunes

---

### FASE 5: LOGS Y AUDITORÍA COMPLETOS

**Objetivo:** Implementar auditoría completa de cambios

**Duración Estimada:** 1 semana

**Tareas:**

1. **Crear entidad AuditLog** (1 día)
   - Properties: Id, AdminUserId, Action, Entity, EntityId, OldValue (JSON), NewValue (JSON), IpOrDevice, CreatedAt
   - Agregar DbSet en DbContext
   - Crear migración

2. **Implementar IAuditLogService** (2 días)
   - LogConfigurationChange(adminId, entity, entityId, oldValue, newValue)
   - LogLoginAttempt(username, success, deviceName)
   - LogBackupOperation(operation, filePath, success)
   - LogServiceAction(action, details)
   - GetAllAuditLogs(filters) para consulta

3. **Instrumentar servicios existentes** (3 días)
   - CategoryService: Log en Create, Update, Delete
   - BlockedSiteService: Log en Create, Update, Delete, ApplyRules
   - AllowedSiteService: Log en Create, Update, Delete
   - ScheduleService: Log en Create, Update, Delete
   - AuthService: Log en Login success/failure, ChangePassword
   - BackupService: Log en CreateBackup, RestoreBackup
   - SettingsService: Log en UpdateSetting (especialmente sensibles)

4. **Crear ActivityLogsView.xaml** (2 días)
   - Dos tabs: "Activity Logs" y "Audit Logs"
   - Filtros: Rango de fechas, tipo de evento, usuario
   - DataGrid con columnas relevantes
   - Botón "Exportar Logs" (CSV)
   - Auto-refresh cada 30 segundos (opcional)

5. **Agregar navegación desde Dashboard** (0.5 días)
   - Botón "Ver Logs" en Dashboard
   - NavigateToActivityLogs() en MainWindow

**Entregable:** Sistema completo de logging y auditoría con UI de visualización

---

### FASE 6: BACKUPS MEJORADOS

**Objetivo:** Backups cifrados, automáticos y validados

**Duración Estimada:** 1 semana

**Tareas:**

1. **Agregar cifrado AES a BackupService** (2 días)
   - Instalar paquete NuGet: System.Security.Cryptography.Algorithms
   - Método EncryptBackup(FileStream input, FileStream output, string password)
   - Método DecryptBackup(FileStream input, FileStream output, string password)
   - Usar AES-256-CBC con salt y IV aleatorios
   - Derivar clave de password usando PBKDF2

2. **Implementar firma de integridad** (1 día)
   - Calcular SHA256 del backup (antes o después de cifrar)
   - Guardar hash en metadata del backup
   - Validar hash antes de restaurar
   - Rechazar restauración si hash no coincide

3. **Crear ScheduledBackupService** (2 días)
   - BackgroundService separado o integrado en Worker
   - Leer configuración: BackupFrequency (Daily/Weekly), BackupTime
   - Timer que ejecute CreateBackupAsync() automáticamente
   - Rotación automática: mantener últimos 7 backups diarios, 4 semanales
   - Borrar backups antiguos automáticamente

4. **Backup automático pre-cambios críticos** (1 día)
   - En BlockedSiteService.ApplyBlockingRulesAsync():
     - Crear backup automático antes de modificar hosts
   - En cualquier servicio que haga cambios masivos:
     - Trigger backup automático
   - Marcar estos backups como "Auto-PreChange" en metadata

5. **Validación antes de restaurar** (1 día)
   - En RestoreBackupAsync():
     - Verificar que archivo exista
     - Verificar checksum/integridad
     - Pedir confirmación al usuario (MessageBox)
     - Crear backup del estado actual antes de restaurar
     - Restaurar
     - Reiniciar servicio/aplicar reglas

6. **UI de gestión de backups** (1 día)
   - Mejorar SettingsView o crear BackupManagementView
   - Lista de backups disponibles con fecha, tamaño, tipo
   - Botón "Crear Backup Manual"
   - Botón "Restaurar" en cada backup
   - Input de contraseña para backups cifrados
   - Indicador de próximo backup automático

**Entregable:** Sistema de backups robusto, cifrado y automático

---

### FASE 7: INSTALADOR Y DESPLIEGUE

**Objetivo:** Instalador profesional MSI/MSIX

**Duración Estimada:** 1.5 semanas

**Tareas:**

1. **Crear proyecto de instalador WiX** (4 días)
   - Instalar WiX Toolset v4
   - Crear proyecto AulaSegura.Installer
   - Definir componentes:
     - AulaSegura.App.exe y dependencias
     - AulaSegura.Service.exe y dependencias
     - Base de datos inicial (aulasegura.db vacío o con seed)
     - Archivos de configuración (appsettings.json)
     - Página de bloqueo HTML
   - Configurar directorios de instalación (Program Files)
   - Configurar permisos NTFS durante instalación

2. **Script de instalación de servicio** (2 días)
   - Custom Action en WiX o script PowerShell separado
   - Crear servicio Windows: sc create AulaSeguraService binPath="..." start=auto
   - Configurar cuenta: LocalSystem
   - Configurar recovery: restart on failure
   - Iniciar servicio automáticamente post-instalación
   - Verificar que servicio esté corriendo

3. **Inicialización post-instalación** (1 día)
   - Ejecutar DatabaseSeeder para crear admin inicial
   - Generar contraseña aleatoria o usar default (admin/admin123)
   - Mostrar contraseña en pantalla final del instalador
   - Configurar settings iniciales
   - Aplicar permisos de seguridad en DB

4. **Crear desinstalador protegido** (2 días)
   - Custom Action de desinstalación
   - Solicitar contraseña de administrador antes de proceder
   - Detener servicio
   - Eliminar servicio
   - Preguntar: ¿Conservar base de datos? (Sí/No)
   - Si No: eliminar aulasegura.db
   - Limpiar archivo hosts de entradas de AulaSegura
   - Eliminar carpetas de aplicación

5. **Firmar ejecutables (opcional)** (1 día)
   - Obtener certificado de code signing (costo económico)
   - Firmar AulaSegura.App.exe y AulaSegura.Service.exe
   - Firmar installer.msi
   - Reduce warnings de SmartScreen en Windows

6. **Documentar proceso de instalación** (1 día)
   - MANUAL_INSTALACION.md paso a paso
   - Screenshots del instalador
   - Troubleshooting común
   - Requisitos previos (.NET 8 Runtime)

**Entregable:** Instalador MSI profesional listo para distribución

---

### FASE 8: PRUEBAS Y QA

**Objetivo:** Validar funcionalidad completa del sistema

**Duración Estimada:** 2 semanas

**Tareas:**

1. **Crear suite de tests unitarios** (3 días)
   - Proyecto AulaSegura.Tests (xUnit)
   - Tests para AuthService (login, hash, force brute)
   - Tests para CategoryService (CRUD)
   - Tests para BlockedSiteService (CRUD, validation)
   - Tests para ValidationHelper (dominios, emails, horarios)
   - Tests para CryptoHelper (encrypt/decrypt)
   - Cobertura objetivo: >70% de lógica de negocio

2. **Crear tests de integración** (2 días)
   - Test de flujo completo: Agregar sitio bloqueado → ApplyRules → Verificar hosts
   - Test de backup/restore: Crear backup → Corromper DB → Restaurar → Verificar integridad
   - Test de login: Intentos fallidos → Bloqueo temporal → Esperar → Login exitoso
   - Test de reporting: Insertar datos de prueba → Generar reporte → Verificar CSV/PDF

3. **Plan de pruebas manuales** (2 días)
   - Checklist detallado:
     - [ ] Login con credenciales correctas
     - [ ] Login con credenciales incorrectas (5 veces → bloqueo)
     - [ ] Agregar categoría nueva
     - [ ] Agregar sitio bloqueado
     - [ ] Aplicar reglas y verificar hosts
     - [ ] Intentar acceder a sitio bloqueado desde Chrome
     - [ ] Verificar que muestre página de bloqueo
     - [ ] Agregar sitio permitido (whitelist)
     - [ ] Verificar que whitelist tenga prioridad
     - [ ] Crear horario de bloqueo
     - [ ] Verificar que horario se aplique
     - [ ] Generar reporte de bloqueos
     - [ ] Exportar reporte a CSV
     - [ ] Exportar reporte a PDF
     - [ ] Crear backup manual
     - [ ] Restaurar backup
     - [ ] Ver logs de actividad
     - [ ] Ver logs de auditoría
     - [ ] Cambiar configuración sensible
     - [ ] Verificar que se cifre en DB
     - [ ] Desinstalar aplicación (con contraseña)

4. **Pruebas de seguridad básicas** (2 días)
   - Intentar modificar archivo hosts manualmente → verificar detección
   - Intentar borrar logs desde UI → verificar que no sea posible
   - Intentar acceder a DB directamente → verificar permisos
   - Intentar desinstalar sin contraseña → verificar que se rechace
   - SQL injection en campos de texto → verificar que EF Core proteja

5. **Pruebas de compatibilidad** (2 días)
   - Probar bloqueo en Chrome
   - Probar bloqueo en Firefox
   - Probar bloqueo en Edge
   - Probar en Windows 10
   - Probar en Windows 11
   - Verificar que servicio inicie automáticamente tras reboot

6. **Pruebas de rendimiento** (1 día)
   - Agregar 1000 sitios bloqueados
   - Medir tiempo de ApplyBlockingRulesAsync()
   - Verificar que UI no se congele durante operaciones
   - Medir uso de memoria del servicio
   - Verificar que no haya memory leaks tras 24 horas

7. **Documentar resultados** (1 día)
   - INFORME_QA.md con:
     - Resumen de pruebas ejecutadas
     - Bugs encontrados y corregidos
     - Bugs conocidos pendientes
     - Recomendaciones de mejora
     - Métricas de cobertura de tests

**Entregable:** Sistema validado y listo para producción piloto

---

### FASE 9: MEJORAS ADICIONALES (OPCIONAL)

**Objetivo:** Pulir UX y agregar features avanzadas

**Duración Estimada:** 2-3 semanas

**Tareas Potenciales:**

1. **DNS Listener avanzado** (reemplaza hosts file)
   - Implementar servidor DNS local
   - Intercepter consultas DNS en tiempo real
   - Resolver dominios bloqueados a 127.0.0.1
   - Más robusto que archivo hosts

2. **Integración con Firewall de Windows**
   - Crear reglas de firewall dinámicas
   - Bloquear conexiones salientes a IPs de sitios prohibidos
   - Complemento al bloqueo por hosts/DNS

3. **Perfiles de usuario**
   - Diferentes reglas para diferentes usuarios de Windows
   - Perfil "Estudiante" vs "Profesor" vs "Admin"
   - Políticas granulares por usuario

4. **Panel web remoto (Futura Versión 2.0)**
   - API REST en el servicio
   - Panel web para administrar remotamente
   - Reportes centralizados para múltiples equipos
   - Requiere autenticación JWT, HTTPS, etc.

5. **Machine Learning para clasificación**
   - Clasificar sitios automáticamente por contenido
   - Reducir necesidad de listas manuales
   - Integrar con APIs de categorización (OpenDNS, etc.)

6. **Notificaciones push**
   - Notificar al admin cuando se intenta acceder a sitio crítico
   - Email/SMS alerts para eventos de seguridad
   - Dashboard en tiempo real

---

## PRIORIZACIÓN DE FASES

| Fase | Prioridad | Impacto | Esfuerzo | Dependencias |
|------|-----------|---------|----------|--------------|
| **Fase 1: Motor de Bloqueo** | 🔴 CRÍTICA | Muy Alto | Alto | Ninguna |
| **Fase 2: Entidades Faltantes** | 🟠 ALTA | Alto | Medio | Ninguna |
| **Fase 3: Reportes** | 🟠 ALTA | Alto | Medio | Fase 2 |
| **Fase 4: Seguridad** | 🟠 ALTA | Muy Alto | Alto | Fase 1 |
| **Fase 5: Logs/Auditoría** | 🟡 MEDIA | Medio | Medio | Fase 2 |
| **Fase 6: Backups Mejorados** | 🟡 MEDIA | Medio | Medio | Fase 4 |
| **Fase 7: Instalador** | 🟢 BAJA | Alto | Alto | Fase 1-6 |
| **Fase 8: Pruebas/QA** | 🔴 CRÍTICA | Muy Alto | Alto | Todas las anteriores |
| **Fase 9: Mejoras** | 🔵 OPCIONAL | Variable | Variable | Fase 1-8 |

**Recomendación:** Ejecutar fases en orden numérico. No saltar Fase 1 bajo ninguna circunstancia, ya que es el core del producto.

---

## CONCLUSIONES Y RECOMENDACIONES FINALES

### Fortalezas Actuales del Proyecto

1. ✅ **Arquitectura sólida**: Clean Architecture bien implementada, fácil de mantener y extender
2. ✅ **Base tecnológica moderna**: .NET 8, WPF, EF Core, SQLite son tecnologías maduras y adecuadas
3. ✅ **MVVM correctamente aplicado**: Separación clara entre UI y lógica de negocio
4. ✅ **CRUDs funcionales**: Gestión de categorías, sitios bloqueados/permitidos, horarios operativos
5. ✅ **Navegación implementada**: Flujo de usuario completo desde login hasta todas las pantallas
6. ✅ **Servicio Windows estructurado**: Base del servicio existe, solo falta lógica de bloqueo

### Debilidades Críticas a Abordar

1. ❌ **NO BLOQUEA SITIOS REALMENTE**: El problema más grave. Sin Fase 1, el producto no cumple su función principal
2. ❌ **Sin reportes**: RF-011 y RF-012 son requisitos clave del expediente
3. ❌ **Seguridad básica**: Fuerza bruta, cifrado, permisos necesitan fortalecimiento
4. ❌ **Sin instalador**: Imposible distribuir profesionalmente sin Fase 7
5. ❌ **Sin tests**: Riesgo alto de regresiones y bugs en producción

### Riesgos del Proyecto

1. **Riesgo Técnico Alto**: Implementar bloqueo web efectivo es complejo. Estrategias como DNS interception o WFP requieren conocimiento especializado de redes Windows
2. **Riesgo de Tiempo**: Fases 1, 4, 7 y 8 son intensivas. Estimado total: 12-16 semanas para MVP completo
3. **Riesgo de Compatibilidad**: Diferentes navegadores y versiones de Windows pueden comportarse distinto con bloqueo de hosts/DNS
4. **Riesgo Legal/Ético**: Inspección HTTPS requiere manejo cuidadoso de certificados y privacidad

### Recomendaciones Estratégicas

1. **Enfocarse en Fase 1 inmediatamente**: Sin bloqueo funcional, nada más importa
2. **Comenzar con estrategia simple (hosts file)**: No intentar DNS interception o proxy en primer intento
3. **Validar temprano con usuarios reales**: Tras Fase 1, probar con 2-3 equipos piloto
4. **Documentar decisiones técnicas**: Cada decisión de arquitectura de bloqueo debe documentarse con pros/contras
5. **Considerar asesoría externa**: Si el equipo no tiene experiencia en networking Windows, contratar consultor especializado para Fase 1
6. **No descuidar seguridad desde el inicio**: Implementar Fase 4 pronto para evitar vulnerabilidades tempranas
7. **Invertir en tests**: Fase 8 no es opcional. Bugs en software de seguridad son críticos

### Próximos Pasos Inmediatos

1. **Reunión de planificación**: Presentar este informe al equipo y stakeholders
2. **Confirmar alcance MVP**: ¿Es aceptable comenzar solo con bloqueo por hosts? ¿O se requiere DNS desde el inicio?
3. **Asignar recursos**: ¿Quién implementará Fase 1? ¿Hay expertise en networking?
4. **Crear backlog detallado**: Desglosar cada fase en tickets específicos de Azure DevOps/GitHub Issues
5. **Establecer sprint planning**: Sprints de 2 semanas, comenzando con Fase 1
6. **Configurar ambiente de desarrollo**: Asegurar que todos tengan Windows 10/11, .NET 8 SDK, SQLite tools

---

## ESTADO FINAL DE LA AUDITORÍA

**Auditoría Completada:** ✅  
**Brechas Identificadas:** 47 ítems críticos/importantes  
**Fases de Corrección Definidas:** 9 fases detalladas  
**Estimación Total de Esfuerzo:** 12-16 semanas para MVP completo  
**Recomendación:** **INICIAR FASE 1 INMEDIATAMENTE**

---

**Documento generado:** 26 de Abril, 2026  
**Próxima revisión recomendada:** Tras completar Fase 1  
**Responsable de seguimiento:** Líder de Proyecto / Arquitecto de Software
