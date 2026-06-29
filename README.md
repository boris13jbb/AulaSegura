# AulaSegura

AulaSegura es una aplicacion de escritorio para Windows orientada al bloqueo y control de acceso web en entornos escolares, familiares o administrativos. Combina una interfaz WPF de administracion con un Worker Service que reaplica reglas sobre el archivo `hosts` y sirve una pagina local de bloqueo.

## Objetivo

Permitir que un administrador gestione listas negras, listas blancas, categorias, palabras clave, horarios, reglas, respaldos, reportes y configuracion basica desde una aplicacion local. La lista blanca tiene prioridad sobre la lista negra. Los cambios se sincronizan con `C:\Windows\System32\drivers\etc\hosts` cuando la aplicacion o el servicio se ejecutan con permisos de administrador.

## Tecnologias

- .NET 8
- WPF (`net8.0-windows`)
- Worker Service para Windows
- Entity Framework Core 8
- SQLite
- CommunityToolkit.Mvvm
- LiveChartsCore
- Serilog
- BCrypt.Net

## Requisitos Previos

- Windows 10/11
- .NET 8 SDK para desarrollo
- .NET 8 Desktop Runtime para ejecutar la app publicada
- PowerShell 5+ o PowerShell 7+
- Permisos de administrador para modificar el archivo `hosts` e instalar el servicio

## Instalacion

```powershell
git clone https://github.com/boris13jbb/AulaSegura.git
cd AulaSegura
dotnet restore AulaSegura.sln
dotnet build AulaSegura.sln
```

Ejecutar la aplicacion WPF:

```powershell
dotnet run --project src/AulaSegura.App/AulaSegura.App.csproj
```

Ejecutar el Worker Service en modo consola/desarrollo:

```powershell
dotnet run --project src/AulaSegura.Service/AulaSegura.Service.csproj
```

## Configuracion

Archivos principales:

- `src/AulaSegura.App/appsettings.json`
- `src/AulaSegura.Service/appsettings.json`
- `.env.example`

`.env.example` documenta variables de entorno, pero .NET no lo carga automaticamente sin una herramienta externa.

| Variable | Uso |
| --- | --- |
| `ConnectionStrings__DefaultConnection` | Ruta SQLite compartida por app y servicio. Por defecto usa `%PROGRAMDATA%\AulaSegura\aulasegura.db`. |
| `Database__RecreateWhenSchemaIsIncomplete` | Permite recrear la BD si el esquema esta incompleto. Mantener `false` salvo respaldo previo. |
| `SeedAdmin__Username` | Usuario inicial del administrador. |
| `SeedAdmin__Password` | Contrasena inicial. No commitear un valor real. |
| `SeedAdmin__Email` | Email inicial del administrador. |
| `SeedAdmin__FullName` | Nombre visible del administrador. |
| `AppSettings__CheckBlockingRulesIntervalSeconds` | Frecuencia del Worker para reaplicar reglas. |
| `AppSettings__BlockingPagePort` | Puerto local de la pagina de bloqueo. |

Si `SeedAdmin__Password` no esta configurada y no existe ningun administrador, AulaSegura genera una contrasena temporal y crea `first-run-admin.txt` junto al ejecutable. Ese archivo esta ignorado por Git y debe eliminarse despues del primer cambio de contrasena.

## Comandos Utiles

```powershell
dotnet restore AulaSegura.sln
dotnet build AulaSegura.sln
dotnet run --project src/AulaSegura.App/AulaSegura.App.csproj
dotnet run --project src/AulaSegura.Service/AulaSegura.Service.csproj
dotnet publish src/AulaSegura.App/AulaSegura.App.csproj -c Release
dotnet publish src/AulaSegura.Service/AulaSegura.Service.csproj -c Release
```

Actualmente no hay proyectos de pruebas ni scripts de lint configurados.

## Estructura

```text
AulaSegura/
|-- src/
|   |-- AulaSegura.App/             # Aplicacion WPF y ViewModels
|   |-- AulaSegura.Core/            # Entidades, interfaces, constantes y validadores
|   |-- AulaSegura.Infrastructure/  # EF Core, SQLite y servicios
|   |-- AulaSegura.Service/         # Worker Service y pagina local de bloqueo
|   `-- AulaSegura.Shared/          # Proyecto compartido reservado
|-- docs/                           # Documentacion tecnica y manuales
|-- scripts/                        # Scripts de instalacion y empaquetado
|-- .env.example                    # Variables de entorno documentadas
|-- .editorconfig                   # Estilo basico del repositorio
|-- .gitignore
`-- AulaSegura.sln
```

## Funcionalidades Actuales

- Login de administrador con BCrypt.
- Bloqueo temporal de cuenta por intentos fallidos.
- Gestion de sitios bloqueados y permitidos.
- Prioridad de lista blanca sobre lista negra.
- Categorias, horarios, reglas de bloqueo y palabras clave.
- Horarios efectivos: si una categoria tiene horarios activos, sus dominios solo se escriben al `hosts` dentro de esas ventanas.
- Busqueda y gestion de palabras clave por tipo (`Blocked`, `Allowed`, `Warning`).
- Dashboard con metricas, grafico y estado de permisos sobre `hosts`.
- Auditoria de acciones relevantes.
- Sincronizacion de reglas al archivo `hosts`.
- Pagina HTML local para indicar sitio bloqueado.
- Respaldos y restauracion desde la pantalla de configuracion.
- Exportacion CSV de reportes.

## Mejoras Aplicadas

- Limpieza de artefactos versionados: `.vs`, `bin`, `obj`, `dist`, paquetes generados y CSVs de reportes.
- Eliminacion de plantilla Worker duplicada en la raiz y archivos placeholder.
- `.gitignore` ampliado para logs, backups, `.env`, bases SQLite locales y reportes generados.
- `.env.example` agregado sin secretos reales.
- Configuracion compartida de SQLite mediante `%PROGRAMDATA%\AulaSegura\aulasegura.db`.
- Seed de administrador sin contrasena fija en codigo.
- Generacion de contrasena temporal local cuando no se configura `SeedAdmin__Password`.
- Validacion y normalizacion de dominios mas robusta.
- Prevencion de duplicados y reactivacion segura de registros inactivos.
- Sincronizacion de hosts despues de cambios en listas, horarios y restauraciones.
- Restauracion real de configuracion desde respaldos JSON.
- Cambio de contrasena usando el administrador autenticado.
- Validacion de contrasena consistente entre UI y backend.
- Manejo mas seguro de errores de login, arranque y configuracion.
- Reglas de bloqueo con `RuleType`, `Action`, `Value`, categoria incluida y borrado logico.
- Palabras clave con selector de enum correcto, busqueda funcional, validaciones y borrado logico.
- Conversores WPF con `ConvertBack` seguro para binding bidireccional.
- Dashboard conectado al servicio real de configuracion.
- Proteccion contra inyeccion HTML en la pagina de bloqueo.
- Inicializacion de BD sin borrado automatico por defecto si el esquema esta incompleto.

## Notas Para Desarrolladores

- La modificacion del archivo `hosts` requiere permisos de administrador.
- La base de datos local no debe commitearse.
- `Database__RecreateWhenSchemaIsIncomplete=true` puede borrar datos; usar solo con respaldo.
- Los documentos de `docs/` incluyen historico del desarrollo; el README es la referencia operativa principal.
- Antes de publicar una version, ejecutar `dotnet restore`, `dotnet build` y validar manualmente app + servicio en Windows con permisos de administrador.

## Posibles Mejoras Futuras

- Agregar proyectos de pruebas unitarias e integracion.
- Introducir migraciones EF Core en lugar de `EnsureCreated`.
- Implementar roles/permisos para multiples administradores.
- Agregar recuperacion segura de cuenta inicial.
- Mejorar instalador y empaquetado versionado.
- Agregar monitoreo de cambios externos en `hosts`.
- Agregar CI con build, tests y analisis estatico.
